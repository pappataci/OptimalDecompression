namespace ToPython

[<AutoOpen>]
module EnvironmentToPython = 
    open LEModel
    open InputDefinition
    open InitDescent

    let state2Vector (x : State<LEStatus>) : float[] = 
        [| leStatus2Risk x |]
        |> Array.append (leStatus2TissueTension x)
        |> Array.append  [|leState2Depth x|]   
        |> Array.append [|leStatus2ModelTime x |]
   
    let getEnvInitStateAndAscentLimiter( maxPDCS, 
                                         penaltyForExceedingRisk, 
                                         integrationTime, 
                                         controlToIntegrationTimeRatio,
                                         descentRate,
                                         maxDepth,
                                         bottomTime, 
                                         legDiscreteTime )  = 

        let maxRiskBound = pDCSToRisk maxPDCS
        let modelBuilderParams = { TimeParams = { IntegrationTime                  = integrationTime  // minute  
                                                  ControlToIntegrationTimeRatio    = controlToIntegrationTimeRatio 
                                                  MaximumFinalTime                 = penaltyForExceedingRisk }  // minute 
                                   LEParamsGeneratorFcn = USN93_EXP.fromConstants2ModelParamsWithThisDeltaT crossover rates threshold gains thalmanErrorHypothesis 
                                   StateTransitionGeneratorFcn = modelTransitionFunction 
                                   ModelIntegration2ModelActionConverter = targetNodesPartitionFcnDefinition 
                                   RewardParameters                      = { MaximumRiskBound  = maxRiskBound
                                                                             PenaltyForExceedingRisk = penaltyForExceedingRisk }  }
        
        let missionParameters = { DescentRate       = descentRate     // ft/min
                                  MaximumDepth      = maxDepth    // ft 
                                  BottomTime        = bottomTime    // min
                                  LegDiscreteTime   = legDiscreteTime      // min 
                                  InitialDepth      = 0.0 }    // ft
                                  |> System2InitStateParams

        let ( environment ,  initstate ,  _ , ascentLimiter ) = initializeEnvironment  (modelsDefinition , modelBuilderParams |> Parameters ) 
                                                                   shortTermRewardEstimator terminalStatePredicate infoLogger 
                                                                   (initialStateCreator , missionParameters )  ascentLimiterFcn     
        
        environment ,  initstate , ascentLimiter

    let private environmOutput2Tuple (  { EnvironmentFeedback = envResp} : EnvironmentOutput<LEStatus, obj>  )=
        (envResp.NextState, envResp.TransitionReward, envResp.IsTerminalState)

    let private getEnvironmentOutput(Environment environm: Environment<LEStatus, float, obj> )  (actualState: State<LEStatus> ) ( nextDepth : float ) = 
        nextDepth
        |> Control
        |> environm actualState

    let getNextEnvironmentResponse(  environm: Environment<LEStatus, float, obj>  , actualState: State<LEStatus> ,  nextDepth : float )  =        
        nextDepth
        |> getEnvironmentOutput environm actualState 
        |> environmOutput2Tuple

    let getRateDelimiter ( rateOfAscentLimiter : option< EnvironmentExperience<LEStatus, float> -> float > )
                         ( experience          : EnvironmentExperience<LEStatus , float >                  ) =
        match rateOfAscentLimiter with
        | None           -> MissionConstraints.descentRateLimit
        | Some rateLimit -> rateLimit experience

    let private envOutputAndDelimiter2Tuple (  { EnvironmentFeedback = envResp} : EnvironmentOutput<LEStatus, obj>  ) (ascentRateLimit :float)   = 
        (envResp.NextState, envResp.TransitionReward, envResp.IsTerminalState , ascentRateLimit)

    let getNextEnvResponseAndBoundForNextAction ( environm: Environment<LEStatus, float, obj>  ,
                                                  actualState: State<LEStatus> ,  nextDepth : float , 
                                                  rateOfAscentLimiter : option< EnvironmentExperience<LEStatus, float> -> float > ) = 
        let envOutput          = nextDepth
                                 |> getEnvironmentOutput  environm actualState 

        let ascentRateLimit    = (nextDepth|> Control)
                                 |> defEnvironmentExperience envOutput actualState 
                                 |> getRateDelimiter rateOfAscentLimiter
                                 |> round 
        
        (envOutput , ascentRateLimit) ||> envOutputAndDelimiter2Tuple 
        
    let private getGuassianNoiseWithLevel noiseLevel = 
        (new System.Random()).NextDouble() * noiseLevel

    let private perturbeSingleTissueTension noiseLevel (x:Tissue) =
        noiseLevel
        |>getGuassianNoiseWithLevel 
        |> Tension
        |> (+>) x  

    let private perturbeTissueTensions (tissueTensions:Tissue[]) noiseLevel = 
        tissueTensions
        |> Array.map (perturbeSingleTissueTension noiseLevel)

    let private perturbCurrentDepth noiseLevel (TemporalValue depthNTime: DepthInTime) = 
        let actualDepth = depthNTime.Value
        let increment =  match  abs(actualDepth) < 1.0e-6 with         
                         | true -> 0.0
                         | false -> getGuassianNoiseWithLevel noiseLevel
        { depthNTime with Value = actualDepth +  increment } 
        |>TemporalValue

    let private perturbLEPhysics (x:LEState) noiseLevel = 
        { TissueTensions = perturbeTissueTensions x.TissueTensions noiseLevel
          CurrentDepthAndTime = perturbCurrentDepth  noiseLevel x.CurrentDepthAndTime}

    let private perturbRisk (x:RiskInfo) noiseLevel = 
        let actualRiskIncrement = x.IntegratedRisks |> Array.sum
        
        let noisedRisks = x.IntegratedRisks
                          |> Array.map (fun actualRisk ->  (1.0 + abs(getGuassianNoiseWithLevel noiseLevel) ) * actualRisk
                                                           |> max   0.0 )  
        
        let cummulativeRiskIncrement = noisedRisks 
                                       |> Array.sum
                                       |> (+) -actualRiskIncrement

        { AccruedRisk       = x.AccruedRisk + cummulativeRiskIncrement 
          IntegratedRisks   = noisedRisks } // this increment first single risks and then perturb consequently the accrued risk

    let perturbState (State leStatus , physicsNoiseLevel, riskNoiseLevel) =  // have to use closure, since Python cannot use curried functions
        {LEPhysics = perturbLEPhysics leStatus.LEPhysics physicsNoiseLevel
         Risk      = perturbRisk leStatus.Risk riskNoiseLevel}
        |> State 
        
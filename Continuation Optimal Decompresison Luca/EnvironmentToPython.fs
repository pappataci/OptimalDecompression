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
                                                  MaximumFinalTime                 = maxFinalTime }  // minute 
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
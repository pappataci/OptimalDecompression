namespace ToPython

[<AutoOpen>]
module EnvironmentToPython = 
    open LEModel
    open InputDefinition

    let state2Vector (x : State<LEStatus>) : float[] = 
        [| leStatus2Risk x |]
        |> Array.append (leStatus2TissueTension x)
        |> Array.append  [|leState2Depth x|]   
        |> Array.append [|leStatus2ModelTime x |]
   
    let getEnvironmentAndInitState( maxPDCS, 
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
        
        let ( environment ,  initstate ,  _  ) = initializeEnvironment  (modelsDefinition , modelBuilderParams |> Parameters ) 
                                                                shortTermRewardEstimator terminalStatePredicate infoLogger (initialStateCreator , missionParameters ) 
         
        environment ,  initstate 

    let getNextEnvironmentResponse(Environment environm: Environment<LEStatus, float, obj>  , actualState: State<LEStatus> ,  nextDepth : float )  =
        
        let environmOutput2Tuple (  { EnvironmentFeedback = envResp} : EnvironmentOutput<LEStatus, obj>  )=
            (envResp.NextState, envResp.TransitionReward, envResp.IsTerminalState)
        
        nextDepth|> Control
        |> environm actualState 
        |> environmOutput2Tuple
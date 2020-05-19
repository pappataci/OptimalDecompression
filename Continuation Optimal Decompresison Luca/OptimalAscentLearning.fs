﻿[<AutoOpen>]
module OptimalAscentLearning

open ReinforcementLearning
open LEModel
open InitDescent

type DescentParams = { DescentRate      : float
                       MaximumDepth     : float
                       BottomTime       : float
                       LegDiscreteTime  : float
                       InitialDepth     : float  }

[<AutoOpen>]
module ModelDefinition =

    type TemporalParams = { IntegrationTime                   : float  
                            ControlToIntegrationTimeRatio     : int 
                            MaximumFinalTime                  : float }

    type TerminalRewardParameters = { MaximumRiskBound        : float 
                                      PenaltyForExceedingRisk : float   }

    type LEModelEnvParams =  { TimeParams                            : TemporalParams 
                               LEParamsGeneratorFcn                  : float -> LEModelParams 
                               StateTransitionGeneratorFcn           : LEModelParams -> LEStatus -> float -> LEStatus 
                               ModelIntegration2ModelActionConverter : int -> State<LEStatus> -> Action<float> -> seq<Action<float>> 
                               RewardParameters                      : TerminalRewardParameters }

    let targetNodesPartitionFcnDefinition (numberOfActions: int) (  initialState:State<LEStatus> ) 
        (Control targetDepth: Action<float>)  =

        let initDepth = initialState |>  leState2Depth
        let depthIncrement = targetDepth 
                             |> max 0.0
                             |> (+) -initDepth
                             |> (*) (   1./(float numberOfActions) ) 
                             
        Seq.init numberOfActions (fun idx ->  
                                        let actualIncrement = float (idx + 1 ) * depthIncrement 
                                        initDepth + actualIncrement
                                        |> Control)

 

    let defEnvironmentModels ( Parameters ( { TimeParams  =  timeParams
                                              LEParamsGeneratorFcn = leModelParamsGenerator
                                              StateTransitionGeneratorFcn = stateTransitionGeneratorFcn
                                              ModelIntegration2ModelActionConverter = integration2ActionModelConverter} ) ) = 
        let integrationModel = timeParams.IntegrationTime
                                |> leModelParamsGenerator
                                |> stateTransitionGeneratorFcn
                                |> fromValueFuncToStateFunc

        let actionModel     = integrationModel
                              |> defineModelOnSlowerDecisionTime ( integration2ActionModelConverter timeParams.ControlToIntegrationTimeRatio)
        
        {   IntegrationModel = integrationModel 
            ActionModel      = actionModel }

[<AutoOpen>]
module RewardDefinition = 
    
    let shortTermRewardOnTimeDifference (_:EnvironmentParameters<LEModelEnvParams>) initState (_ :Action<float>) nextState = 
        let initTime = leStatus2ModelTime initState
        let finalTime = leStatus2ModelTime nextState
        initTime - finalTime // it is minus duration: time length is a cost 

    let penaltyIfMaximumRiskIsExceeded (Parameters penaltyParams':EnvironmentParameters<LEModelEnvParams>) ( finalState:State<LEStatus>)  = 
         let penaltyParams = penaltyParams'.RewardParameters
         match   ( leStatus2Risk finalState >= penaltyParams.MaximumRiskBound ) with 
         | true -> -abs(penaltyParams.PenaltyForExceedingRisk) // penalty is strictly non positive
         | _ -> 0.0

    let defineShortTermRewardEstimator ( shortTermNonTerminalRewardFcn: 
                                            EnvironmentParameters<LEModelEnvParams> -> State<LEStatus> -> Action<float> -> State<LEStatus> -> float ) 
                                        terminalRewardFcn   =

        {InstantaneousReward = InstantaneousReward shortTermNonTerminalRewardFcn
         TerminalReward      =  terminalRewardFcn  }
     
[<AutoOpen>]
module FinalStateIdentification = 

    let defineFinalStatePredicate (Parameters envParams : EnvironmentParameters<LEModelEnvParams>)  =
        
        let modelParams  = envParams.TimeParams.IntegrationTime |> envParams.LEParamsGeneratorFcn
        let surfaceDepth = 0.0
        let surfaceN2Pressure = surfaceDepth 
                                |> depth2N2Pressure modelParams.ThalmanErrorHypothesis modelParams.FractionO2 
        let maximumTolerableRisk = envParams.RewardParameters.MaximumRiskBound
        let maximumSimulationTime = envParams.TimeParams.MaximumFinalTime

        let isFinalStatePredicate (    actualState: State<LEStatus> ) : bool = 
            let isEmergedAndNotAccruingRisk = leStatus2IsEmergedAndNotAccruingRisk actualState surfaceN2Pressure
            let simulationTime = leStatus2ModelTime actualState 
            let hasExceededMaximumTime = simulationTime >=  maximumSimulationTime
            let hasExceededMaximumRisk = (leStatus2Risk actualState) >= maximumTolerableRisk
            ( isEmergedAndNotAccruingRisk ||  hasExceededMaximumTime || hasExceededMaximumRisk ) 
        isFinalStatePredicate

[<AutoOpen>]
module GetStateAfterFixedLegImmersion = 

    let giveNextStateForThisModelNDepthNTimeNode  model (actualState: State<LEStatus>) nextDepth =
        nextDepth 
        |> Control 
        |> getNextStateFromActualStateModelNAction model actualState 

    let runModelAcrossSequenceOfNodesNGetFinalState (initState: State<LEStatus> )  model  (sequenceOfDepths:seq<float>)= 
        sequenceOfDepths
        |> Seq.fold (giveNextStateForThisModelNDepthNTimeNode model)
           initState

    let runModelThroughNodesNGetAllStates  (initState: State<LEStatus> )  model  (sequenceOfDepths:seq<float>)  =
        sequenceOfDepths
        |> Seq.scan (giveNextStateForThisModelNDepthNTimeNode model)
           initState

    let getInitialStateWithTheseParams  ({DescentRate = descentRate; MaximumDepth = maxDepth; BottomTime = bottomTime
                                          LegDiscreteTime =  fixedLegDiscretizationTime ; InitialDepth = initDepth }   )
                                        (model:Model<LEStatus, float>) = 
        
        let skipFirstIfEqualsToInitState (State aState) (aSeqOfNodes:seq<DepthInTime>) = 
            
            let temporalValueToDepth (TemporalValue x) = 
                x.Value

            let getDepthOfThisState (state:LEStatus)   =
                (state.LEPhysics.CurrentDepthAndTime)
                |> temporalValueToDepth
            
            let getDepthOfFirstNode (mySeqOfNodes:seq<DepthInTime>) : float = 
                mySeqOfNodes
                |> Seq.head
                |> temporalValueToDepth

            match (getDepthOfThisState aState = getDepthOfFirstNode aSeqOfNodes) with 
            | true   ->  aSeqOfNodes |> Seq.skip 1 
            | false  ->  aSeqOfNodes
     
        let initState =  initDepth
                        |> USN93_EXP.initStateFromInitDepth ( USN93_EXP.getLEOptimalModelParamsSettingDeltaT  fixedLegDiscretizationTime ) 
                        |> State

        let immersionLeg = bottomTime
                           |> defineFixedImmersion descentRate  maxDepth
        let seqOfNodes =  discretizeConstantDescentPath immersionLeg fixedLegDiscretizationTime 
                          |> skipFirstIfEqualsToInitState initState 

        seqOfNodes
        |> Seq.map (fun (TemporalValue x ) -> x.Value)
        |> runModelAcrossSequenceOfNodesNGetFinalState initState model
    
[<AutoOpen>]
module InfoLoggerDefinition = 

    // rate is positive when new depth is higher than previous depth (during descent phase)
    let maximumPositiveRateForDepthAndTissue bThalmannnHyp fo2Air (actualState: State<LEStatus> ) 
            (temporalParams : TemporalParams) =
         let maxTissuePressure = actualState
                                 |> leStatus2TissueTension
                                 |> Array.max
                                 |> n2Pressure2Depth bThalmannnHyp fo2Air

                             
     
         0.0
    let nullLogger<'I >   = (fun (_:EnvironmentParameters<LEModelEnvParams> ) (_: EnvironmentExperience<LEStatus, float>) -> 
                                None |> Log ) // null function used to build the EnvLogger 
                            |> EnvLogger 

// PYTHON PART
// Initialize Knowledge (Q Factor approximator)
// Initialize Learning Function

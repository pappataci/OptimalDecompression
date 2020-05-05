[<AutoOpen>]
module OptimalAscentLearning

open ReinforcementLearning
open LEModel
open InitDescent

type DescentParams = { DescentRate  : float
                       MaximumDepth : float
                       BottomTime   : float }

let pDCSToRisk pDCS = 
    -log(1.0-pDCS)

[<AutoOpen>]
module ModelDefinition =
    
    type TemporalParams = { IntegrationTime                   : float  
                            ControlToIntegrationTimeRatio     : int }

    let integrationTime = 0.1 // minute

    let leParamsWithIntegrationTime = integrationTime
                                      |> USN93_EXP.getLEOptimalModelParamsSettingDeltaT 
     
    let getNextState = modelTransitionFunction leParamsWithIntegrationTime
    let integrationModel = fromValueFuncToStateFunc getNextState

    let targetNodesPartitionFcnDefinition (numberOfActions: int) (State initialState:State<LEStatus> ) 
        (Control targetDepth: Action<float>)  =

        let initDepth = initialState |>  leState2Depth
        let depthIncrement = targetDepth 
                             |> max 0.0
                             |> (*) (   1./(float numberOfActions) ) 
                             
        Seq.init numberOfActions (fun idx ->  
                                        let actualIncrement = float (idx + 1 ) * depthIncrement 
                                        initDepth + actualIncrement
                                        |> Control)
    
    let actionToIntegrationTimeRation = 10
    
    let decisionalModel = integrationModel 
                         |> defineModelOnSlowerDecisionTime (targetNodesPartitionFcnDefinition actionToIntegrationTimeRation)  
    
    type LEModelEnvParams =  { TimeParams                            : TemporalParams 
                               LEParamsGeneratorFcn                  : float -> LEModelParams 
                               StateTransitionGeneratorFcn           : LEModelParams -> LEStatus -> float -> LEStatus 
                               ModelIntegration2ModelActionConverter : int -> State<LEStatus> -> Action<float> -> seq<Action<float>> }

    let getModelBuilderForEnvironment(modelParams : LEModelEnvParams) = 

        let defineDecisionalModel ( Parameters ( {TimeParams  =  timeParams
                                                  LEParamsGeneratorFcn = leModelParamsGenerator
                                                  StateTransitionGeneratorFcn = stateTransitionGeneratorFcn
                                                  ModelIntegration2ModelActionConverter = integration2ActionModelConverter} ) ) = 
            timeParams.IntegrationTime
            |> leModelParamsGenerator
            |> stateTransitionGeneratorFcn 
            |> fromValueFuncToStateFunc
            |> defineModelOnSlowerDecisionTime ( integration2ActionModelConverter timeParams.ControlToIntegrationTimeRatio)
    
        ( defineDecisionalModel |> ModelDefiner , 
          modelParams |> Parameters ) 

[<AutoOpen>]
module RewardDefinition = 
    
    type TerminalRewardParameters = { MaximumRiskBound        : float 
                                      PenaltyForExceedingRisk : float   }

    let shortTermNonTerminalReward initState (_ :Action<float>) nextState = 
        let initTime = leStatus2ModelTime initState
        let finalTime = leStatus2ModelTime nextState
        initTime - finalTime // it is minus duration: time length is a cost 

    let defineTerminalRewardFunction (penaltyParams:TerminalRewardParameters) ( finalState:State<LEStatus>)  = 
         match   ( leStatus2Risk finalState >= penaltyParams.MaximumRiskBound ) with 
         | true -> -abs(penaltyParams.PenaltyForExceedingRisk) // penalty is strictly non positive
         | _ -> 0.0

    let defineShortTermRewardEstimator (shortTermNonTerminalRewardFcn:State<LEStatus> -> Action<float> -> State<LEStatus> -> float) 
                                       (penaltyParams:TerminalRewardParameters) =

        {InstantaneousReward = InstantaneousReward shortTermNonTerminalRewardFcn
         TerminalReward      =  defineTerminalRewardFunction penaltyParams}
     
//type TerminalStatePredicate<'S> = | StatePredicate of (State<'S> -> bool)    

    //let isTerminalState 

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

    let getInitialStateWithTheseParams  ({DescentRate = descentRate; MaximumDepth = maxDepth; BottomTime = bottomTime} :DescentParams)
           fixedLegDiscretizationTime  initDepth (model:Model<LEStatus, float>) = 
        
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
        |> runModelThroughNodesNGetAllStates initState model
        |> Seq.last

module EnvironmentDefinition = 
    
    let nullLogger = InfoLogger (fun (_,_,_,_,_) -> None ) 
    let model = ModelDefinition.integrationModel
    
    // to be refined with only interesting parameters ( e.g.: TimeParams ) 
    let modelBuilderParams = { TimeParams = { IntegrationTime                  = 0.1  // minute  
                                              ControlToIntegrationTimeRatio    = 10  } 
                               LEParamsGeneratorFcn = USN93_EXP.getLEOptimalModelParamsSettingDeltaT 
                               StateTransitionGeneratorFcn = modelTransitionFunction 
                               ModelIntegration2ModelActionConverter = targetNodesPartitionFcnDefinition }

    

        


// PYTHON PART
// Initialize Knowledge (Q Factor approximator)
// Initialize Learning Function
// Define reward function
// Define finalState

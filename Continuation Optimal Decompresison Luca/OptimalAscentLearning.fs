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

module ModelDefinition =
    
    let integrationTime = 0.1 // minute

    let leParamsWithIntegrationTime = integrationTime
                                      |> USN93_EXP.getLEOptimalModelParamsSettingDeltaT 
     
    let getNextState = modelTransitionFunction leParamsWithIntegrationTime
    let model = fromValueFuncToStateFunc getNextState


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
        //, seqOfNodes


module EnvironmentDefinition = 
    let nullLogger = InfoLogger (fun (_,_,_,_,_) -> None ) 



//let testModel (Model model:Model<LEStatus, float> ) (  initState: State<LEStatus>) = 
//    let test = model initState
//    test


// PYTHON PART
// Initialize Knowledge (Q Factor approximator)
// Initialize Learning Function
// Define reward function
// Define finalState

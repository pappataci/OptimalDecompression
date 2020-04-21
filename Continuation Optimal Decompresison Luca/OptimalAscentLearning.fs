[<AutoOpen>]
module OptimalAscentLearning

open ReinforcementLearning
open LEModel
open InitDescent

let pDCSToRisk pDCS = 
    -log(1.0-pDCS)

module ModelDefinition =
    
    let integrationTime = 0.1 // minute

    let leParamsWithIntegrationTime = integrationTime
                                      |> USN93_EXP.getLEOptimalModelParamsSettingDeltaT 
     
    let getNextState = modelTransitionFunction leParamsWithIntegrationTime
    let model = fromValueFuncToStateFunc getNextState
    //let model' = defineModel leParamsWithIntegrationTime  defineModelTransitionFunction

    // let model 

module InitProfilSequence = 
     
    let defaultDescentRate = 60.0
    let defaultMaximumDepth = 120.0
    let defaultBottomTime = 30.0

module GetStateAfterFixedLegImmersion = 
    open InitProfilSequence

    let initDepth = 0.0

    //let initState = initDepth
    //                |>  (USN93_EXP.initStateFromInitDepth USN93_EXP.fromConstants2ModelParams ) // default model Params )
    //                |>  State

    let fixedDescentPart = defineFixedImmersion defaultDescentRate defaultMaximumDepth defaultBottomTime
    
    let descentSequence = discretizeConstantDescentPath fixedDescentPart ModelDefinition.integrationTime

    let getStateAfterFixedImmersion sequenceOfDepthNTime initState = 
        sequenceOfDepthNTime
        |> Seq.fold (fun   leStatus  (TemporalValue seqDepthTime )  ->  
                    ModelDefinition.getNextState  leStatus   seqDepthTime.Value  )  initState

    //let getInitialState (descRate:float) (maxDepth:float) (bottomTime:float) initDepth (transitionModel) = 


module EnvironmentDefinition = 
    let nullLogger = InfoLogger (fun (_,_,_,_,_) -> None ) 



//let testModel (Model model:Model<LEStatus, float> ) (  initState: State<LEStatus>) = 
//    let test = model initState
//    test


// PSEUDO CODE:
// Get First State (run model until given mission point)

// PYTHON PART
// Initialize Knowledge (Q Factor approximator)
// Initialize Learning Function
// Define reward function
// Define finalState

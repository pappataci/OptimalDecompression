[<AutoOpen>]
module OptimalAscentLearning

open Gas
open ReinforcementLearning
open LEModel
open InitDescent

let pDCSToRisk pDCS = 
    -log(1.0-pDCS)

module ModelDefinition =

    let thalmanHyp = true
    let integrationTime = 0.1 // minute

    let defaultLEModelParams = integrationTime
                               |> USN93_EXP.fromConstants2ModelParamsWithThisDeltaT
                               |> USN93_EXP.setThalmanHypothesis thalmanHyp
     
    let getNextState = updateLEStatus defaultLEModelParams
    let model = fromValueFuncToStateFunc getNextState
    
module GetStateAfterFixedLegImmersion = 
    let initDepth = 0.0
    let initState = initDepth
                    |>  (USN93_EXP.initStateFromInitDepth USN93_EXP.fromConstants2ModelParams ) // default model Params )
                    |>  State

    let getStateAfterFixedImmersion fixedLegParams legDiscretizationTime getNextState initState = 
        let sequenceOfDepthNTime = discretizeConstantDescentPath fixedLegParams legDiscretizationTime
        sequenceOfDepthNTime
        |> Seq.fold (fun   leStatus  (TemporalValue seqDepthTime )  ->  getNextState  leStatus   seqDepthTime.Value  )  initState

    
    

module EnvironmentDefinition = 
    let nullLogger = InfoLogger (fun (_,_,_,_,_) -> None ) 



//let testModel (Model model:Model<LEStatus, float> ) (  initState: State<LEStatus>) = 
//    let test = model initState
//    test


// PSEUDO CODE:
// Create model 
// Get First State (run model until given mission point)

// PYTHON PART
// Initialize Knowledge (Q Factor approximator)
// Initialize Learning Function
// Define reward function
// Define finalState

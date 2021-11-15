(*
This script is intended to test that the actions created from the tables are 
equivalent to running the model through the tables, as a sequence of <DepthTime>
*)

#load "Logger.fs"
#load "SeqExtension.fs"
#load "Gas.fs"
#load "ELModelCommon.fs"
#load "TrinomialModel.fs"
#load "TableDataInputs.fs"
#load "TableReader.fs"
#load "ProfileIntegrator.fs"
#load "MissionDefinerFromTables.fs"
#load "SurfaceTableCreator.fs"
#load "Diagnostics.fs"
#load "TableToDiscreteActionsSeq.fs"


//let initialConditions, depthProfiles = getTableOfInitialConditions table9FileName



//let vecIndex = 1
//let initSeq = getAscentProfileFromSingleDepthProfile initialConditions.[vecIndex]  depthProfiles.[vecIndex]
    
//let actions = initSeq 
//                |> toVectorOfActions


//printfn " INIT CONDITION %A"   initialConditions.[vecIndex].InitAscentNode
//printfn " TABLE ASCENT %A"   ( depthProfiles.[vecIndex] |> Seq.toArray ) 
//printfn " ACTIONS %A" actions

//let vectorsOfActions = Array.map2 getAscentProfileFromSingleDepthProfile initialConditions  depthProfiles
//                       |> Array.map toVectorOfActions


let getTableInitialConditionsAndTableStrategies tableFileName = 
    let initialConditions, depthProfiles = getTableOfInitialConditions tableFileName
    let vectorsOfActions = Array.map2 getAscentProfileFromSingleDepthProfile initialConditions  depthProfiles
                           |> Array.map toVectorOfActions
    initialConditions , vectorsOfActions


let initialConditions, vectorsOfActions = getTableInitialConditionsAndTableStrategies table9FileName
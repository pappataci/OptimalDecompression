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



//let vectorsOfActions = Array.map2 getAscentProfileFromSingleDepthProfile initialConditions  depthProfiles
//                       |> Array.map toVectorOfActions



//let initialConditions, vectorsOfActions = getTableInitialConditionsAndTableStrategies table9FileName

let tableFileName = table9FileName
let initialConditions, depthProfiles = getTableOfInitialConditions tableFileName

let offendingProfileLbl = 128
let initCond = initialConditions.[offendingProfileLbl]
let depthProf = depthProfiles.[offendingProfileLbl] |> Seq.toArray

let ascentProfile = getAscentProfileFromSingleDepthProfile initCond depthProf



toVectorOfActions ascentProfile // has a bug, as expected

// try to isolate just the two last steps (assuming everything else is functional)
//let ascentProfVec = ascentProfile|>Seq.toArray

//let prevDepth, actDepth = ascentProfVec.[2..]
//                           |> (fun x -> x.[0].Depth , x.[1].Depth)
                           
//getActionForAscent  prevDepth actDepth
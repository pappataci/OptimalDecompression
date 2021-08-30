#load "SeqExtension.fs"
#load "Gas.fs"
#load "ELModelCommon.fs"
#load "TrinomialModel.fs"
#load "TableDataInputs.fs"
#load "TableReader.fs"
#load "ProfileIntegrator.fs"
#load "MissionDefinerFromTables.fs"
#load "SurfaceTableCreator.fs"
//open ProfileIntegrator
open ModelRunner

let profilingOutput  = fileName
                                            |> getDataContent
                                            |> Array.map data2SequenceOfDepthAndTime


let solutions = profilingOutput |> Array.Parallel.map  ( fun( x,   _ )  -> runModelOnProfile x ) 

let tableInitialConditions = profilingOutput |> Array.Parallel.map getInitialConditionAndTargetForTable

let tensionToRiskTable = solutions |> Array.Parallel.map getTensionToRiskAtSurface

let tensions, risks = tensionToRiskTable |> Array.unzip


let pressureDistributions = tensions
                            |> initPresssures
                            |> Array.unzip3

let press0 , press1 , press2 = pressureDistributions

let range (x : 'T[]) = 
    x|> Array.min ,  x |> Array.max

[|press0; press1 ; press2|]
|> Array.map range

press2 |> Array.findIndex ( fun x -> x > 1.3779)

//let getTensionDistributionForThisTissue  (tensToRiskTable:(TissueTension[]*double)[]) = 
//    tensToRiskTable
//    |> Array.mapi (fun i (tissueTensionsVec, risk)  -> tissueTensionsVec)
press0 |> Array.sort
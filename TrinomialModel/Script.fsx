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

let tensionToRiskTable = solutions |> Array.Parallel.map getTensionToIndividualRisksAtSurface

let tensions, risks = tensionToRiskTable |> Array.unzip

let overAllSurfaceToGoRisk = risks
                             |> Array.map (fun r -> r |> Array.sum)

let pressureDistributions = tensions
                            |> initPressures
                            |> Array.unzip3


let press0 , press1 , press2 = pressureDistributions

let risks0 , risks1, risks2 = [| 0 .. ((risks|>Array.length ) - 1 )  |]
                              |> Array.map ( fun i -> risks.[i].[0] , risks.[i].[1], risks.[i].[2] )
                              |> Array.unzip3


let range (x : 'T[]) = 
    x|> Array.min ,  x |> Array.max

let pressRanges = [|press0; press1 ; press2|]
                  |> Array.map range

let individualRisksRanges = [|risks0 ; risks1; risks2|]
                            |> Array.map range

let overallRiskRange = range overAllSurfaceToGoRisk

let totalRisks = solutions
                 |> Array.Parallel.map (fun seqOfNode -> seqOfNode |> Seq.last |> (fun n -> n.TotalRisk))

let totalRiskRange = range totalRisks
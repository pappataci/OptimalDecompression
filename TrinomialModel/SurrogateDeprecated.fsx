#r @"C:\Program Files (x86)\ILNumerics\ILNumerics Ultimate VS\bin\ILNumerics.Core.dll"
#r @"C:\Users\glddm\source\repos\DecompressionL20190920A\FuncApprox\bin\Debug\FuncApprox.dll"
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
open FuncApprox
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

//let adimMapper = new Kriging1DAdimMapper( press0, risks0)
let pressureMappers = Array.map2 (fun (x:double[]) (y:double[]) -> new Kriging1DAdimMapper(x, y) ) 
                        [|press0;press1;press2|] [|risks0; risks1 ; risks2|]

let surfaceRiskDefiner( mappers: Kriging1DAdimMapper[]) =
    let actualEstimator(pressures:float[]) = 
        Array.map2 (fun (x: Kriging1DAdimMapper) y -> x.EstimateMapValue(y) ) mappers  pressures
    actualEstimator

let riskApproximator = surfaceRiskDefiner pressureMappers

// test riskApproximator
riskApproximator([|1.0;1.5;1.3|])

solutions.[125] |> Seq.item 6

let surrogateSurfaceRunner  (surfRiskApprox:double[]->double[]) (initNode:Node) : Node = 
    
    let initTensions = initNode.TissueTensions |> Array.map (fun (Tension t) -> t)
    let initWeightedRisk = initNode.AccruedWeightedRisk
    let wightedRiskIncreaseAtSurf = surfRiskApprox  initTensions
    let updatedWeigthedRisk = Array.map2 (+) initWeightedRisk  wightedRiskIncreaseAtSurf
    let finalTotalRisk = updatedWeigthedRisk |> Array.sum

    {initNode with EnvInfo = {initNode.EnvInfo with Time = infinity}
                   IntegratedWeightedRisk = updatedWeigthedRisk
                   TotalRisk = finalTotalRisk } 

let surrogateSurfaceWithTableApprox = surrogateSurfaceRunner riskApproximator

let completeSurrogateModel :seq<DepthTime> -> seq<Node> = 
    runModelOnProfileGen  surrogateSurfaceWithTableApprox

let solutions' = profilingOutput |> Array.Parallel.map  ( fun( x,   _ )  -> runModelOnProfile x ) 
    
let surrogateSolutions = profilingOutput |> Array.Parallel.map  ( fun( x,   _ )  -> completeSurrogateModel x ) 
#r @"C:\Users\glddm\source\repos\DecompressionL20190920A\packages\Extreme.Numerics.7.0.15\lib\net46\Extreme.Numerics.dll"
#load "Diagnostics.fs"
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
#load "MinimalTimeSearcher.fs"


open ModelRunner
open MinimalSearcher
let tableInitialConditions  , seqDepthAndTimeFromTables = getTableOfInitialConditions  table9FileName

// define dumb guesser
let initBrekFraction = 0.5
let initPowerCoeff = 1.0
let initTau = 0.1

let initialGuesser = createStaticInitialGuesser(initBrekFraction, initPowerCoeff, initTau)

//let optimizedVsTableComparison missionMetrics initialGuesser 

let optimizeAndCompare = getOptimizedVsTableComparison  initialGuesser

let tableEntry = 1

let missionMetrics = tableInitialConditions.[tableEntry] 


let comparison =  optimizeAndCompare missionMetrics

//let oneCurveVsTableComparison = tableInitialConditions.[0..10]
//                                |> Array.map optimizeAndCompare

let numberOfEntries = tableInitialConditions |> Seq.length 

let comparisons  =
    seq { for x in 0 .. (numberOfEntries - 1 )   do 
                                             printfn "solving %A" x
                                             let comparison = optimizeAndCompare tableInitialConditions.[x]
                                             printfn "solved in %A" comparison.SearchTime
                                             yield  comparison  }
    |> Seq.toArray


let better, worse = comparisons |> Array.partition (fun x -> x.OptimalVsOriginalPercRiskDiff <= 10.0 && x.OptimalValues.[2] >= 0.0 && x.OptimalVsOriginalAscentTimeDiff <= 0.0 )
better |> Seq.sortBy ( fun x -> x.OptimalVsOriginalAscentTimeDiff) |> Seq.map ( fun x -> x.OptimalVsOriginalAscentTimeDiff) |> Seq.toArray


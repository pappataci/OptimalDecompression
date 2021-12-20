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

open MinimalSearcher
let tableInitialConditions  , seqDepthAndTimeFromTables = getTableOfInitialConditions  table9FileName

// define dumb guesser
let initBrekFraction = 0.5
let initPowerCoeff = 1.0
let initTau = 0.1

let initialGuesser = createStaticInitialGuesser(initBrekFraction, initPowerCoeff, initTau)

//let optimizedVsTableComparison missionMetrics initialGuesser 

let optimizeAndCompare = getOptimizedVsTableComparison  initialGuesser

//let tableEntry = 1

//let missionMetrics = tableInitialConditions.[tableEntry] 


//let comparison =  optimizeAndCompare missionMetrics

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

let strategyToString (strategy : seq<DepthTime>) (optionalTimeOffset:Option<double>)  =
    let timeOffset = match optionalTimeOffset with
                    | Some time -> time
                    | None -> 0.0
    let shiftTime x = x + timeOffset
    strategy
    |> Seq.map ( fun x -> (shiftTime x.Time).ToString() + ", " + x.Depth.ToString() )

let outputFolder =  @"C:\Users\glddm\Documents\Duke\Research\OptimalAscent\AscentData\"

let tableStrategyToDisk fileName tableSeqDepthAndTime   = 
    writeStringSeqToDisk  fileName  ( strategyToString tableSeqDepthAndTime None )

let optimizedStrategyToDisk fileName optimalAscentCurve (tableInitialCondition:TableMissionMetrics) = 
    let timeOffset = tableInitialCondition.MissionInfo.BottomTime |> Some 
    writeStringSeqToDisk fileName  ( strategyToString optimalAscentCurve timeOffset )

type CurveComparer = | Comparer of TableMissionMetrics * seq<DepthTime> * OptimalSolutionResult 

let dataComparisons = Array.zip3 tableInitialConditions seqDepthAndTimeFromTables comparisons
                      |>  Array.map Comparer

let vectorToString (vec: seq<'T> ) = 
    vec 
    |> Seq.map (fun x -> (x.ToString() + " ") )

let tableVsOptimalCurveToDisk fileName (optimalStrategy:OptimalSolutionResult) (initCondition: TableMissionMetrics) = 
    // optimal values 
    // final weighted risk for optimal strategy
    // final total risk for optimal strategy
    // riskIncrement
    // ascent time
    // time increment
    // Bottom Time
    // Max Depth

    let outputString = seq{yield! (vectorToString optimalStrategy.OptimalValues)
                           yield!   optimalStrategy.OptimalCurveSolution
                                    |> Seq.last 
                                    |> (fun x -> x.AccruedWeightedRisk ) 
                                    |> vectorToString  
                           yield! seq{optimalStrategy.OptimalRisk.ToString()
                                      optimalStrategy.TableRisk.ToString()
                                      optimalStrategy.OptimalVsOriginalPercRiskDiff.ToString()
                                      optimalStrategy.OptimalAscentTime.ToString()
                                      optimalStrategy.OptimalVsOriginalAscentTimeDiff.ToString()
                                      optimalStrategy.MissionInfo.BottomTime.ToString()
                                      optimalStrategy.MissionInfo.MaximumDepth.ToString()
                                      } 
                            }
    writeStringSeqToDisk  fileName outputString


let createOutputFromDataComparison (arrayIndx: int) (Comparer (initCondition, tableStrategy, optimalStrategy) )   = 
    
    let getFileNameWithKw keyword = outputFolder + keyword + arrayIndx.ToString() + ".txt"
    
    let tableFileName = getFileNameWithKw "tableCurve"  
    let optCurveFileName =getFileNameWithKw    "optCurve" 
    let results = getFileNameWithKw "results"

    tableStrategyToDisk tableFileName tableStrategy
    optimizedStrategyToDisk optCurveFileName optimalStrategy.OptimalAscentCurve initCondition

    tableVsOptimalCurveToDisk results optimalStrategy initCondition
    //tableStrategyToDisk  


// dump data to disk
dataComparisons
|> Array.mapi createOutputFromDataComparison

//writeStringSeqToDisk  @"C:\Users\glddm\Documents\Duke\Research\OptimalAscent\Logs\strat1.txt"  ( strategyToString None seqDepthAndTimeFromTables.[0])
createOutputFromDataComparison 0 dataComparisons.[0]
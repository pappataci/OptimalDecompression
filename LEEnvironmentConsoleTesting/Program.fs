// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.
open System
open Result2CSV

let pressAnyKey() = Console.Read() |> ignore

[<EntryPoint>]
let main _ = 

    //let targetDepth = 30.0
    
    //let seqStepsMaxTarget = seq{1;2} // seq{1;2;3;4}
    //let seqMaxDepth = seq{60.0 .. 60.0 .. 180.0 } // seq{60.0 .. 30.0 .. 180.0}
    //let seqBottomTimes = seq{30.0 .. 60.0 .. 150.0}
    //let seqConstDepth = seq{20.0 .. -10.0 .. 0.0}
    //let seqTimeAtConstDepth = seq{ 1 .. 30 .. 200}
    //let seqAscentRates = seq{-30.0 ;   -10.0 ; -5.0  }
    let targetDepth' = 45.0
    let seqBottomTimes' = seq{30.0 .. 30.0 .. 300.0}

    let initConstDepth = targetDepth' - 5.0

    let seqConstDepth' = seq{initConstDepth .. -5.0 .. max (initConstDepth - 25.0)   1.  }
    let seqTimeAtConstDepth' = seq{ 1 .. 10 .. 200}
    let seqAscentRates = seq{-30.0 .. 5.0 .. -5.0} 
    
    let results , immmersionData , strategyOutput  =  solveThisAscentwithInitEndAscent seqBottomTimes' seqConstDepth' seqTimeAtConstDepth'  seqAscentRates (targetDepth': float )
    //let results = solveThisAscentForThisTargetDepth targetDepth  seqStepsMaxTarget seqMaxDepth seqBottomTimes  seqConstDepth seqTimeAtConstDepth seqAscentRates
   
    let grouppedResults = results
                            |> results2Groups
                            |> Seq.collect getRidOfSubOptimalSolutionsForThisGroup

    let fileName = 
        "completeResults" +  ( targetDepth' |> string ) + "Complete.csv"

    grouppedResults
    |> writeResultsToDisk fileName (Some  "TwoPiecesAscent" ) 
    printfn "Completed"
    //results
    //|> myCsvBuildTable
    //|> saveToCsv @"C:\Users\glddm\Desktop\TwoLegResults30.csv"

    pressAnyKey()
    //let commonSimulationParameters = {MaxPDCS = 0.32 ; MaxSimTime = 1000.0 ; PenaltyForExceedingRisk  = 1.0 ; RewardForDelivering = 10.0; PenaltyForExceedingTime = 0.5 ;
    //                                  IntegrationTime = 0.1; ControlToIntegrationTimeRatio = 10; DescentRate = 60.0; MaximumDepth = 20.0 ; BottomTime = 10.0;  LegDiscreteTime = 0.1}

    //printfn"insert number of elements"
    //let maxInputsString = Console.ReadLine()
    //let maxInputs = maxInputsString |> Double.Parse
    //let inputsStrategies =  [|0.0 .. maxInputs|] |> Array.map (fun x -> ( commonSimulationParameters , Seq.initInfinite (fun _ -> x )  |> Ascent ) |> StrategyInput   )

    //printfn "init"
    //let stopWatch = System.Diagnostics.Stopwatch.StartNew()
    //let testSequential = inputsStrategies 
    //                     |> Array.map simulateStrategy
    //let durSequentialMap = stopWatch.Elapsed.TotalMilliseconds
    //printfn "Seq Done"
    //printfn "SequenceTime time: %A" (   durSequentialMap )
    
    //Console.Read() |> ignore
    
    //stopWatch.Restart()
    //let testParallel = inputsStrategies 
    //                   |> Array.Parallel.map simulateStrategy
    //let durParallel = stopWatch.Elapsed.TotalMilliseconds
    //printfn "parallel done"

    //printfn "Parallel time: %A" durParallel 
    
    //let envResponse = testParallel |> Array.last |> (fun (Output x , _ ) -> x ) 
    //printfn "%A" (envResponse|>Array.last) 





    //stopWatch.Restart()
    //let nessosResult = inputsStrategies
    //                    |> ParStream.ofArray
    //                    |> ParStream.map simulateStrategy
    //                    |> ParStream.toArray
    //let nessosTime = stopWatch.Elapsed.TotalMilliseconds 
    //printfn "Nessos Done"
    //printfn "Async Time: %A" nessosTime
    //Console.Read() |> ignore
    
    0

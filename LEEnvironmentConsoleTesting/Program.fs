// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.
open System
open AscentSimulator
open Nessos.Streams

let pressAnyKey() = Console.Read() |> ignore

[<EntryPoint>]
let main _ = 
    
    let commonSimulationParameters = {MaxPDCS = 0.32 ; MaxSimTime = 1000.0 ; PenaltyForExceedingRisk  = 1.0 ; RewardForDelivering = 10.0; PenaltyForExceedingTime = 0.5 ;
                                      IntegrationTime = 0.1; ControlToIntegrationTimeRatio = 10; DescentRate = 60.0; MaximumDepth = 20.0 ; BottomTime = 10.0;  LegDiscreteTime = 0.1}

    printfn"insert number of elements"
    let maxInputsString = Console.ReadLine()
    let maxInputs = maxInputsString |> Double.Parse
    let inputsStrategies =  [|0.0 .. maxInputs|] |> Array.map (fun x -> ( commonSimulationParameters , Seq.initInfinite (fun _ -> x )  |> Ascent ) |> StrategyInput   )

    printfn "init"
    let stopWatch = System.Diagnostics.Stopwatch.StartNew()
    let testSequential = inputsStrategies 
                         |> Array.map simulateStrategy
    let durSequentialMap = stopWatch.Elapsed.TotalMilliseconds
    printfn "Seq Done"
    printfn "SequenceTime time: %A" (   durSequentialMap )
    
    Console.Read() |> ignore
    
    stopWatch.Restart()
    let testParallel = inputsStrategies 
                       |> Array.Parallel.map simulateStrategy
    let durParallel = stopWatch.Elapsed.TotalMilliseconds
    printfn "parallel done"

    printfn "Parallel time: %A" durParallel 
    
    let envResponse = testParallel |> Array.last |> (fun (Output x , _ ) -> x ) 
    printfn "%A" (envResponse|>Array.last) 

    //testSequential |> Array.last |> Seq.last |> printfn "%A"
    //testParallel |> Array.last   |>Seq.last  |> printfn "%A"

    Console.Read() |> ignore

    //stopWatch.Restart()
    //let testParallelAsync = inputsStrategies 
    //                            |> applyInParallelAndAsync simulateStrategy  
        
    //let asyncTime = stopWatch.Elapsed.TotalMilliseconds 
    //printfn "Async Done"
    //printfn "Async Time: %A" (   asyncTime )

    stopWatch.Restart()
    let nessosResult = inputsStrategies
                        |> ParStream.ofArray
                        |> ParStream.map simulateStrategy
                        |> ParStream.toArray
    let nessosTime = stopWatch.Elapsed.TotalMilliseconds 
    printfn "Nessos Done"
    printfn "Async Time: %A" nessosTime
    Console.Read() |> ignore

    0

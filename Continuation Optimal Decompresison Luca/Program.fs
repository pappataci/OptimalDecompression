open Result2CSV
open TwoStepsSolIl
open System.Diagnostics
open InputDefinition
open LEModel


let getLastTimeFromAscentHistory (x:StrategyResults) =
    let history = x.AscentHistory
    match history with
    | Some h -> h 
                |> Seq.last 
                |> Seq.last
                |>  leStatus2ModelTime

    | None -> 0.0

let getTotalRiskResult aResult = 
    match aResult with
    | Some x -> printfn "%A" (x.AscentResults.TotalRisk, x.AscentParams.TimeToSurface , getLastTimeFromAscentHistory x.AscentResults,
                              x.AscentParams.Exponent, x.AscentParams.BreakFraction)
    | None -> printfn "No result"


[<EntryPoint>]
let main argv =
    
    let integrationTime, controlToIntegration = 0.1 , 1 
    let integrationTimeSettings = integrationTime, controlToIntegration

    //Initial Condition Grid Definition
    let bottomTimes = [|30.0 .. 10.0 .. 60.0|] |> Array.toSeq
    let maxDepths = [|90.0 ; 105.0 ; 120.0  |] |> Array.toSeq
    let probsBound =  [|3.3e-2|]  // for now we inspect only the give probability bound
    let initCondsGrid = create3DGrid bottomTimes maxDepths probsBound

    //Parameter Grid Definition
    let breakFracSeq = [ 0.01 .. 0.1 .. 0.99 ]@[ 0.99 ]
                       |> List.toSeq   
    let exponents = [ -3.0 .. 0.25 .. 2.0 ] |> List.toSeq
    let paramsGrid = create2DGrid breakFracSeq exponents 

    //Candidate Surface Times
    let timesToSurfVec = [1.0 ; 2.0; 5.0; 20.0; 50.0] @ [100.0 .. 50.0 .. 500.0] 
                         |> Array.ofList
  
    let resultsParallel = initCondsGrid
                         |> Array.Parallel.map ( tryFindSolutionWithIncreasingTimesSeq integrationTimeSettings paramsGrid timesToSurfVec )

    resultsParallel
    |> dumpBruteForceAnalysisToDisk 


    System.Console.Read() |> ignore
    0
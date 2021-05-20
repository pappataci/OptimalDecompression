open Result2CSV
open TwoStepsSolIl
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

let mainOld _ =
    
    let integrationTime, controlToIntegration = 0.1 , 1 
    let integrationTimeSettings = integrationTime, controlToIntegration

    //Initial Condition Grid Definition
    let bottomTimes = [|10.0 .. 10.0 .. 90.0|] |> Array.toSeq
    let maxDepths = [|210.0 .. 15.0 ..  300.0  |] |> Array.toSeq
    let probsBound =  [|3.3e-2|]  // for now we inspect only the give probability bound
    let initCondsGrid = create3DGrid bottomTimes maxDepths probsBound

    //Parameter Grid Definition
    let breakFracSeq = [ 0.01 .. 0.1 .. 0.99 ]@[ 0.99 ]
                       |> List.toSeq   
    let exponents = [ 0.25 .. 0.25 .. 3.0 ] |> List.toSeq
    let paramsGrid = create2DGrid breakFracSeq exponents 

    //Candidate Surface Times
    let timesToSurfVec = [1.0 ; 2.0; 5.0; 20.0; 50.0] @ [100.0 .. 10.0 .. 1000.0] 
                         |> Array.ofList
  
    let resultsParallel = initCondsGrid
                         |> Array.Parallel.map ( tryFindSolutionWithIncreasingTimesSeq integrationTimeSettings paramsGrid timesToSurfVec )


    let resultsTable = resultsParallel
                       |> resultsToInputForWriter

    resultsTable
    |> resultsTableToDisk "initResultDeepestDives_3Dot3Prob.csv"

    printfn "results written to disk"

    System.Console.Read() |> ignore
    0

[<EntryPoint>]
let main _ = 

    let integrationTime, controlToIntegration = 0.1 , 1 
    let integrationTimeSettings = integrationTime, controlToIntegration
    let peakPressure = 1.5
    let noRiskPressure = 0.7
    
    let initPressures =  [| [|peakPressure;noRiskPressure ; noRiskPressure|] ; 
                            [|noRiskPressure;peakPressure;noRiskPressure|] ;
                            [|noRiskPressure;noRiskPressure;peakPressure|] ; 
                            [|peakPressure;peakPressure;peakPressure|] |]

    let simulator = simulateSurfaceWithInitPressures (integrationTime, controlToIntegration) 
    let press = initPressures
                |> Array.map simulator
        //Array.take 3 
       
    let riskCompute getter = press 
                            |> getter 
                            |> Array.map  ( Seq.last >>  (fun (State  x) -> x.Risk.AccruedRisk) ) 
                            |> Array.sum

    let riskUnc = riskCompute (Array.take 3)
    let riskCoupled = riskCompute (fun x ->   [| x |> Array.last|] )  
    //printfn "unc %A" riskUnc
    //printfn "coupled  %A" riskCoupled

    //printfn "difference %A" (riskUnc - riskCoupled)

    //let riskCoupled = press |> 

    0 
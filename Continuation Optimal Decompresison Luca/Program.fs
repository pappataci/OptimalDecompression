open Result2CSV
open TwoStepsSolIl
open System.Diagnostics
open InputDefinition
open LEModel

[<EntryPoint>]
let main argv =
    
    let integrationTime, controlToIntegration = 0.1 , 1 
    
    let pDCS = 3.3e-2
    let bottomTime = 60.0
    let maximumDepth = 120.0

    //let bottomTimes = [|30.0 .. 30.0 .. 150.0|] |> Array.toSeq
    //let maxDepths = [|60.0 .. 30.0 .. 300.0|] |> Array.toSeq
    //let probsBound = [|3.2e-2|] |> Array.toSeq // for now just solve for the desired probability
    
    //let initConditionsGrid = create3DGrid bottomTimes maxDepths probsBound
    

    let initCondition = [|bottomTime; maximumDepth; pDCS|]
    let integrationTimeSettings = integrationTime, controlToIntegration

    let deltaTimeSurface =  [1.0] @ [ 5.0 .. 50.0  .. 1000.0]

    // parameter definition for brute force solution
    let breakFracSeq = [ 0.01 .. 0.1 .. 0.99 ]@[ 0.99 ]
                       |> List.toSeq   
    let exponents = [ -3.0 .. 0.25 .. 2.0 ] |> List.toSeq
    let paramsGrid = create2DGrid breakFracSeq exponents 

    //let solveForThisSurfaceTime integrationTimeSettings optimizationParams  maxAllowedRisk initCondition (timeToSurface:float) =
    //    optimizationParams
    //    |> Seq.map  (simulateStratWithParams integrationTimeSettings  initCondition  timeToSurface) 
    //    |> SeqExtension.takeWhileWithLast ( hasExceededMaxRisk maxAllowedRisk )
    //    |> Seq.tryLast
    //    |> getLastIfValid maxAllowedRisk

    let timeToSurface = 11.0 

    let hasExceededMaxAllowedRisk  (initCondition:float[]) = initCondition.[2]
                                                             |> pDCSToRisk
                                                             |> hasExceededMaxRisk


     //FIRST COMPUTATION: Seq.whileLast (all lazy):
     //compute until risk bound is satisfied in a lazy way
    let tryFindSolutionWithAllParams integrationTimeSettings optimizationParams (initCondition:float[]) hasExceededMyMaxAllowedRisk  timeToSurface  =  
        //let hasExceededMyMaxAllowedRisk = hasExceededMaxAllowedRisk initCondition
        optimizationParams 
        |> Seq.map (simulateStratWithParams integrationTimeSettings  initCondition  timeToSurface) 
        |> SeqExtension.takeWhileWithLast hasExceededMyMaxAllowedRisk
        |> Seq.tryLast
        |> getSimulationResultIfNot hasExceededMyMaxAllowedRisk
 
    //let testLazy = tryFindSolutionWithAllParams integrationTimeSettings paramsGrid initCondition  timeToSurface
                    
    let tryFindSolutionWithIncreasingTimesSeq integrationTimeSettings paramsGrid  (timesToSurfVec:float[]) (initCondition:float[])=
        let hasExceededMyMaxAllowedRisk = hasExceededMaxAllowedRisk initCondition
        timesToSurfVec
        |> Seq.map (tryFindSolutionWithAllParams integrationTimeSettings paramsGrid initCondition hasExceededMyMaxAllowedRisk )
        |> SeqExtension.takeWhileWithLast Option.isNone
        |> Seq.last
        |> getSimulationResultIfNot hasExceededMyMaxAllowedRisk

    let timesToSurfVec = [1.0 ; 2.0; 5.0; 20.0; 50.0] @ [100.0 .. 50.0 .. 500.0] 
                         |> Array.ofList
   
    //let results = tryFindSolutionWithIncreasingTimesSeq integrationTimeSettings paramsGrid  (timesToSurfVec) (initCondition)


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


    let bottomTimes = [|30.0 .. 10.0 .. 60.0|] |> Array.toSeq
    let maxDepths = [|90.0 ; 105.0 ; 120.0  |] |> Array.toSeq
    let probsBound =  [|3.3e-2|]
    let initCondsGrid = create3DGrid bottomTimes maxDepths probsBound

    printfn "sequential timing for %A initial conditions" ( initCondsGrid |> Seq.length ) 

    let sw = Stopwatch.StartNew()

    let results = initCondsGrid
                  |> Array.map(  tryFindSolutionWithIncreasingTimesSeq integrationTimeSettings paramsGrid  (timesToSurfVec) )
    
    let elapsedSeq = (sw.ElapsedMilliseconds |> float )/1000.0

    sw.Restart()

    let resultsParallel = initCondsGrid
                         |> Array.Parallel.map(  tryFindSolutionWithIncreasingTimesSeq integrationTimeSettings paramsGrid  (timesToSurfVec) )
    

    let elapsedParallel = (sw.ElapsedMilliseconds |> int )/1000

    printfn "Sequential time vs parallel time %A "   (elapsedSeq  , elapsedParallel )

    printfn "done!"

    //printfn "Length testLazy %A" (testLazy|>Seq.length )
    printfn "Length params %A" (paramsGrid |> Seq.length)


    System.Console.Read() |> ignore
    0
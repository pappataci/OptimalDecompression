open Result2CSV
open TwoStepsSolIl
open System.Diagnostics
open InputDefinition

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
    let breakFracSeq = [ 0.01 .. 0.25 .. 0.99 ]@[ 0.99 ]
                       |> List.toSeq   
    let exponents = [ -2.0 .. 0.5 .. 2.0 ] |> List.toSeq
    let paramsGrid = create2DGrid breakFracSeq exponents 

    let solveForThisSurfaceTime integrationTimeSettings optimizationParams  maxAllowedRisk initCondition (timeToSurface:float) =
        optimizationParams
        |> Seq.map  (simulateStratWithParams integrationTimeSettings  initCondition  timeToSurface) 
        |> SeqExtension.takeWhileWithLast ( hasExceededMaxRisk maxAllowedRisk )
        |> Seq.tryLast
        |> getLastIfValid maxAllowedRisk

    // FIRST COMPUTATION: Seq.whileLast (all lazy):
    // compute until risk bound is satisfied in a lazy way
    //let solveProblemFcn integrationTimeSettings optimizationParams (initCondition:float[])  =  
        //let maxAllowedRisk = pDCSToRisk initCondition.[2]
        //optimizationParams 
        //|> Seq.map (simulateStratWithParams integrationTimeSettings  initCondition  timeSurface) 
        //|> Seq.toArray
        //|> SeqExtension.takeWhileWithLast ( hasExceededMaxRisk maxAllowedRisk )
        //|> Seq.tryLast
        //|> getLastIfValid maxAllowedRisk
 
    0
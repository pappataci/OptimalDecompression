open Result2CSV
open TwoStepsSolIl


[<EntryPoint>]
let main argv =
    let integrationTime, controlToIntegration = 0.1 , 1 
    
    let pDCS = 3.3e-2
    
    let maximumDepth = 120.0

    let bottomTimes = [|30.0 .. 30.0 .. 150.0|] |> Array.toSeq
    let maxDepths = [|60.0 .. 30.0 .. 300.0|] |> Array.toSeq
    let probsBound = [|3.2e-2|] |> Array.toSeq // for now just solve for the desired probability
    
    let initConditionsGrid = create3DGrid bottomTimes maxDepths probsBound
    
    // parameter definition for brute force solution
    let breakFracSeq = [ 0.01 .. 0.15 .. 0.99 ]@[ 0.99 ]
                       |> List.toSeq   
    let exponents = [ -3.0 .. 0.5 .. 2.0 ] |> List.toSeq
    let deltaTimeSurface =  [1.0] @ [ 5.0 .. 50.0  .. 1000.0]
    let paramsGrid = create3DGrid breakFracSeq exponents deltaTimeSurface

    let solveAscentProblemWithTheseGrids (paramsGrid:float[][]) (initCondGrid:float[][]) (integrationTime, controlToIntegration) = 
        let toTuple (x:float[]) = 
            x.[0], x.[1], x.[2]

        let createOutputName (x:float[]) = 
            let bt, maxDepth, probBound = toTuple x 
            printfn "solving %A" x
            "BT_" +  (sprintf "%i" (int bt) ) + "_" + "MD_"  
            + (sprintf "%i" (int maxDepth) ) + "PB" + sprintf "%.1f" (probBound*100.0) + ".csv"
        
        initCondGrid
        |> Array.map  ( fun initCond -> let outputName = createOutputName initCond
                                        initCond
                                        |> toTuple   
                                        |> getOptimalForThisInputCondition  paramsGrid (integrationTime, controlToIntegration) 
                                        |>  bruteForceToDisk outputName   )
           
    //let example = initConditionsGrid |> Array.take 2
    solveAscentProblemWithTheseGrids  paramsGrid  initConditionsGrid  (integrationTime, controlToIntegration)
    |> ignore
 
    0
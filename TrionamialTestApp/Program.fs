open ModelRunner

[<EntryPoint>]
let main _ =
    

    //open ModelRunner
    
    let profilingOutput  = table9FileName
                                                |> getDataContent
                                                |> Array.map data2SequenceOfDepthAndTime
    
    printfn "%A" profilingOutput.[0]    
    //let solutions = profilingOutput |> Array.Parallel.map  ( fun( x,   _ )  -> runModelOnProfile x ) 
    
    //let tableInitialConditions = profilingOutput |> Array.Parallel.map getInitialConditionAndTargetForTable
    
    //let tensionToRiskTable = solutions |> Array.Parallel.map getTensionToRiskAtSurface
    
    //let tensions, risks = tensionToRiskTable |> Array.unzip
    
    
    //let pressureDistributions = tensions
    //                            |> initPressures
    //                            |> Array.unzip3
    
    //let press0 , press1 , press2 = pressureDistributions
    
    //let range (x : 'T[]) = 
    //    x|> Array.min ,  x |> Array.max
    
    //let ranges= [|press0; press1 ; press2|]
    //            |> Array.map range
    
    printfn "Done!" |> ignore
    System.Console.Read() |> ignore
    0 // return an integer exit code

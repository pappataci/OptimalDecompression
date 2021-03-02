
open TwoStepsSolution

open System

[<EntryPoint>]
let main argv =
    
    let pDCS = 5.0e-2
    
    let bottomTime = 30.0
    let maximumDepth = 120.0
    let integrationTime, controlToIntegration = 0.1 , 1 
    
    
    let initGuess = ConstantInitGuess (0.3 , 0.3)

    let out = findOptimalAscentForThisDive (integrationTime, controlToIntegration)  (bottomTime, maximumDepth , pDCS )  initGuess
    
    printfn "%A" out 
    printfn "%A" lastOptimalSurfaceTime
    Console.Read() |> ignore 

    0
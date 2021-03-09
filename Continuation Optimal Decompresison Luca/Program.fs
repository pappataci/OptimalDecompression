
open TwoStepsSolution

open System
open Extreme.Mathematics

[<EntryPoint>]
let main argv =
    
    let pDCS = 3.2e-2
    
    let bottomTime = 30.0
    let maximumDepth = 120.0
    let integrationTime, controlToIntegration = 0.1 , 1 
    
    let initialGuesss =    Vector.Create(1.0 , 0.3 ,  300.0 )
                           |>  ConstantInitGuess
    
    //let solution, report  =   initialGuesss
    //                          |> findOptimalAscentForThisDive (integrationTime, controlToIntegration)  (bottomTime, maximumDepth , pDCS ) 
    
    //let bottomTimes = [|30.0 .. 5.0 .. 100.0|]
    
    
    //let solutionsAtDifferentTimes  = bottomTimes 
    //                                 |> Array.mapi (fun i  bottomTime -> printfn "%A" i    
    //                                                                     findOptimalAscent3DProblem (integrationTime, controlToIntegration)  (bottomTime, maximumDepth , pDCS )  initialGuesss )
    let bottomTime = 100.0
    let result =  findOptimalAscent3DProblem (integrationTime, controlToIntegration)  (bottomTime, maximumDepth , pDCS )  initialGuesss 
    
    0
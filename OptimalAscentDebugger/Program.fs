// Learn more about F# at https://fsharp.org
// See the 'F# Tutorial' project for more help.

open InitDescent
open System
open Extreme.Mathematics
open AscentSimulator
open AscentOptimizer


[<EntryPoint>]
let main argv =
    printfn "%A" argv
    let maxPDCS , maxSimTime = 0.032 , 50000.0
    let rlDummyParam = 0.0 
    let integrationTime , controlToIntegration = 0.1 , 10
    let maximumDepth , bottomTime = 60.0 , 120.0
    let targetDepth = 0.0
    
    
    let simParams = { MaxPDCS = maxPDCS ; MaxSimTime = maxSimTime; PenaltyForExceedingRisk = rlDummyParam;  RewardForDelivering = rlDummyParam; PenaltyForExceedingTime = rlDummyParam; 
                  IntegrationTime = integrationTime; ControlToIntegrationTimeRatio = controlToIntegration; DescentRate = MissionConstraints.ascentRateLimit; MaximumDepth = maximumDepth; 
                  BottomTime = bottomTime; LegDiscreteTime = integrationTime} 
    
    //let initialGuess' =  Vector.Create (-20.0, 50.0 ,  0.0,  30.0 , 1.0,  // first leg with constant times 
    //                                   -20.0, 25.0 , 0.1  , 18.0,  1.5,  // second leg
    //                                   -8.0 , 12.0 , 0.3  , 2.5  )       // third leg 

    let initialGuess =  Vector.Create (-30.0, 0.0 ,  0.0,  0.0 ,  30.0,  // first leg with constant times 
                                        -10.0, 0.0 , 5.0  , 00.0,   31.5,  // second leg
                                        -25.0 , 0.0 , 1.3  , 32.5  )       // third leg 



    try 
        let testn = getOptimalSolutionForThisMission  simParams   targetDepth  initialGuess  None
        printfn "%A" testn
         
    with 
        | Failure(msg)-> printfn "%A"  msg
    0
#r @"C:\Users\glddm\source\repos\DecompressionL20190920A\packages\Extreme.Numerics.7.0.15\lib\net46\Extreme.Numerics.dll"
#r @"C:\Users\glddm\source\repos\DecompressionL20190920A\packages\Microsoft.ML.Probabilistic.0.3.1912.403\lib\netstandard2.0\Microsoft.ML.Probabilistic.dll"
#load "ReinforcementLearning.fs"
#load "PredefinedDescent.fs"
#load "Gas.fs"
#load "LEModel.fs"
#load "OptimalAscentLearning.fs"
#load "InputDefinition.fs"
#load "EnvironmentToPython.fs"
#load "SeqExtension.fs"
#load "AscentSimulator.fs"
#load "AscentBuilder.fs"
#load "OneLegStrategy.fs"

//open ReinforcementLearning
open InitDescent
//open LEModel
//open InputDefinition
//open System
//open Extreme.Mathematics
//open Extreme.Mathematics.Optimization
//open AscentSimulator
//open AscentBuilder

open OneLegStrategy

 // input example

let integrationTime, controlToIntegration = 0.1 , 1
let controlTime = integrationTime * (float controlToIntegration)
let  maxSimTime = 15000.0

// inputs specific to mission 
let maximumDepth = 30.0 // ft
let bottomTime   = 30.0 // minutes
let maxPDCS = 3.3e-2

// small test
let leInitState, myEnv =  initStateAndEnvDescent maxSimTime  (integrationTime, controlToIntegration)   maximumDepth  bottomTime
  
let breakOut = 0.0
let linearSlope = -30.0
let tay , tanhInitDerivative = -0.97 , -30.0


let ascentStrategyExample = createAscentTrajectory controlTime ( bottomTime, maximumDepth ) ( linearSlope, breakOut  , tay , tanhInitDerivative)



let out = getTimeAndAccruedRiskForThisStrategy leInitState  ascentStrategyExample myEnv


// idea is: precreate sequence of strategies and then ordere them according to final time; then see which one does not violate current risk bound

//1) define discrete vectors for free parameters : linearSlope, breakOut, tay; for now tanhInitDerivative is assumed to be equal to linearSlope
//1 bis) create all inputs
//2) create a seq.while function which is executed until current residual risk bound is respected
//3) spit out results to CSV 


let linearSlopeStep = -1.0 // ft/min 
let breakoutStep , maxBreakOut = 0.05 , 0.99
let minTay , tayStep , maxTay = -0.9 , 0.05 , 0.0

let linearSlopeValues =   [-0.01 ..  linearSlopeStep .. MissionConstraints.ascentRateLimit ] @ [ MissionConstraints.ascentRateLimit ]
                          |> Seq.ofList 

let breakoutValues = [ 0.0 .. breakoutStep .. maxBreakOut] |> Seq.ofList

let tayValues = [minTay .. tayStep .. maxTay ] @ [maxTay ] |> Seq.ofList

let actualStrategyInputs  = seq { for linearSlope in linearSlopeValues do 
                                        for breakOut in breakoutValues do 
                                            for tay in tayValues -> (linearSlope, breakOut, tay, linearSlope) }

let createStrategiesForAllInputs controlTime ( bottomTime, maximumDepth )  actualStrategyInputs =
    actualStrategyInputs
    |> Seq.map (createAscentTrajectory controlTime ( bottomTime, maximumDepth )  )


let getStrategyTime (aStrategy:seq<float*float>)  controlTime  =
    
    let initTime = aStrategy                
                   |> Seq.head
                   |> fst

    let lastTime = aStrategy
                   |> Seq.last
                   |> fst

    lastTime - initTime + controlTime 

let allStrategies = createStrategiesForAllInputs controlTime (bottomTime, maximumDepth )  actualStrategyInputs
                    |> Seq.toArray

//getSeqOfDepthsForLinearAscentSection  (initTime:float , initDepth) (slope:float) (breakOut:float) controlTime 

//let computeSurfaceTimeForThisStrategy controlTime ( bottomTime, maximumDepth ) ( linearSlope, breakOut  , tay , tanhInitDerivative) =
    

//let createStrategiesForThisInitCondition controlTime (bottomTim, maximumDepth) (linearSlopes: seq<float> ,   breakoutValues: seq<float> , tayValues: seq<float> ) = 
    
    

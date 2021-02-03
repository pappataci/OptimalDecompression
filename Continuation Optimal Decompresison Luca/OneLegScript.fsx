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
//open InitDescent
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
let maximumDepth = 200.0 // ft
let bottomTime   = 40.0 // minutes
let maxPDCS = 3.3e-2

// small test
let leInitState, myEnv =  initStateAndEnvDescent maxSimTime  (integrationTime, controlToIntegration)   maximumDepth  bottomTime
  
let breakOut = 0.99
let linearSlope = -0.001
let tay , tanhInitDerivative = -0.97 , -30.0


let ascentStrategyExample = createAscentTrajectory controlTime ( bottomTime, maximumDepth ) ( linearSlope, breakOut ) (tay , tanhInitDerivative)


let out = getTimeAndAccruedRiskForThisStrategy leInitState  ascentStrategyExample myEnv



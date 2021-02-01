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

open ReinforcementLearning
open InitDescent
open LEModel
open System
open Extreme.Mathematics
open Extreme.Mathematics.Optimization
open AscentSimulator
open AscentBuilder

let initStateAndEnvAfterAscent maxSimTime  (integrationTime, controlToIntegration)   maximumDepth  bottomTime  = 
    let strategyOutput, myEnv = getInitConditionAfterDescentPhase (integrationTime, controlToIntegration, integrationTime ) (Some maxSimTime) 1   maximumDepth  bottomTime maximumDepth 
    let (Output initialConditionAfterDescent) = strategyOutput 
    let leState = initialConditionAfterDescent
                  |> Array.head
                  |> ( fun (state , _ , _ , _)  -> state ) 
    leState , myEnv
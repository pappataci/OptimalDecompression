#r @"C:\Users\glddm\source\repos\DecompressionL20190920A\packages\Microsoft.ML.Probabilistic.0.3.1912.403\lib\netstandard2.0\Microsoft.ML.Probabilistic.dll"
#r @"C:\Users\glddm\source\repos\DecompressionL20190920A\packages\FSharp.Collections.ParallelSeq.1.1.3\lib\net45\FSharp.Collections.ParallelSeq.dll"

#load "Learner.fs"
#load "ReinforcementLearning.fs"
#load "Gas.fs"
#load "PredefinedDescent.fs"
#load "LEModel.fs"
#load "OptimalAscentLearning.fs"
#load "IOUtilities.fs"
#load "InputDefinition.fs"
#load "EnvironmentToPython.fs"
#load "AsyncHelpers.fs"
#load "SeqExtension.fs"
#load "AscentSimulator.fs"
#load "TwoLegAscent.fs"

//open ReinforcementLearning
open InitDescent
open LEModel
open AscentSimulator


let stepsFromMaxToTarget ,                         initialDepthParams                    ,              twoLegAscentParams                                 , maxAscentRate  = 
              4          ,  {MaxDepth = 120.0 ; BottomTime = 50.0 ; TargetDepth = 70.0 } , { ConstantDepth  = 30.0   ;  TimeStepsAtConstantDepth  = 150   } ,    Some -30.0

let   resultVector , immersionAnalytics   , simulationOutput     = solveThis2LegAscent stepsFromMaxToTarget initialDepthParams twoLegAscentParams maxAscentRate 


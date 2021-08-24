#r @"C:\Users\glddm\source\repos\DecompressionL20190920A\packages\Extreme.Numerics.7.0.15\lib\net46\Extreme.Numerics.dll"
#r @"C:\Users\glddm\source\repos\DecompressionL20190920A\packages\Microsoft.ML.Probabilistic.0.3.1912.403\lib\netstandard2.0\Microsoft.ML.Probabilistic.dll"
#r @"C:\Users\glddm\source\repos\DecompressionL20190920A\packages\FSharp.Data.3.3.3\lib\net45\FSharp.Data.dll"

#r @"C:\Program Files (x86)\ILNumerics\ILNumerics Ultimate VS\bin\ILNumerics.Computing.dll"
#r @"C:\Program Files (x86)\ILNumerics\ILNumerics Ultimate VS\bin\ILNumerics.Core.dll"
#r @"C:\Program Files (x86)\ILNumerics\ILNumerics Ultimate VS\bin\ILNumerics.numpy.dll"
#r @"C:\Program Files (x86)\ILNumerics\ILNumerics Ultimate VS\bin\ILNumerics.Toolboxes.Optimization.dll"

#load "SeqExtension.fs"
#load "ReinforcementLearning.fs"
#load "PredefinedDescent.fs"
#load "Gas.fs"
#load "LEModel.fs"
#load "OptimalAscentLearning.fs"
#load "InputDefinition.fs"
#load "EnvironmentToPython.fs"
#load "AscentSimulator.fs"
#load "SeqExtension.fs"
#load "AscentSimulator.fs"
#load "AscentBuilder.fs"
#load "OneLegStrategy.fs"
#load "Result2CSV.fs"
#load "TwoStepsSolIl.fs"

open LEModel
open AscentSimulator
open OneLegStrategy
open TwoStepsSolIl
open InputDefinition

let integrationTime , controlToIntegration = 0.1, 10
let maximumDepth = 30.0
let bottomTime = 371.0



let initState , myEnv = initStateAndEnvDescent maxSimTime (integrationTime, controlToIntegration) maximumDepth bottomTime

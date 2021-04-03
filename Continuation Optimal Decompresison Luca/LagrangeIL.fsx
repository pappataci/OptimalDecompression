
#r @"C:\Users\glddm\source\repos\DecompressionL20190920A\packages\Extreme.Numerics.7.0.15\lib\net46\Extreme.Numerics.dll"
#r @"C:\Users\glddm\source\repos\DecompressionL20190920A\packages\Microsoft.ML.Probabilistic.0.3.1912.403\lib\netstandard2.0\Microsoft.ML.Probabilistic.dll"
#r @"C:\Users\glddm\source\repos\DecompressionL20190920A\packages\FSharp.Data.3.3.3\lib\net45\FSharp.Data.dll"

#r @"C:\Program Files (x86)\ILNumerics\ILNumerics Ultimate VS\bin\ILNumerics.Computing.dll"
#r @"C:\Program Files (x86)\ILNumerics\ILNumerics Ultimate VS\bin\ILNumerics.Core.dll"
#r @"C:\Program Files (x86)\ILNumerics\ILNumerics Ultimate VS\bin\ILNumerics.numpy.dll"
#r @"C:\Program Files (x86)\ILNumerics\ILNumerics Ultimate VS\bin\ILNumerics.Toolboxes.Optimization.dll"


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
#load "Result2CSV.fs"
#load "TwoStepsSolution.fs"
#load "TwoStepsSolIl.fs"


open ILNumerics 
open type ILMath
open type ILNumerics.Toolboxes.Optimization

open Extreme.Mathematics


open TwoStepsSolIl

let pDCS = 3.2e-2
let maximumDepth = 120.0

let integrationTime, controlToIntegration = 0.1 , 1 

let initialGuess = [|0.6;0.1 ; 1.0|] 


let bottomTime = 60.0

let result =  findOptimalAscentGen (integrationTime, controlToIntegration)  (bottomTime, maximumDepth , pDCS )  initialGuess 

let strategyResult, optimalParams = result 

strategyResult.TotalRisk
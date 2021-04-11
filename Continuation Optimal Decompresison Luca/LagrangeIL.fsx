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
//#load "TwoStepsSolution.fs"
#load "TwoStepsSolIl.fs"

open InputDefinition

open TwoStepsSolIl


// parameter definition for brute force solution
let breakFracSeq = [ 0.01 .. 0.1 .. 0.99 ]@[ 0.99 ]
                   |> List.toSeq

let exponents = [ -3.0 .. 0.1 .. 2.0 ] |> List.toSeq

let deltaTimeSurface =  [1.0] @ [ 5.0 .. 10.0  .. 200.0]

let allInputs = createInputForSim breakFracSeq exponents deltaTimeSurface

let pDCS = 3.3e-2
let maximumDepth = 120.0

let integrationTime, controlToIntegration = 0.1 , 1 

let bottomTime = 60.0

//let resultsToArray (inputVec:float[], result:StrategyResults) =
//    (inputVec.[0], inputVec.[1], inputVec.[2], result.AscentTime, result.AscentRisk, result.SurfaceRisk,
//     result.TotalRisk, result.InitTimeAtSurface)

//let getOptimalForThisInputCondition (bottomTime, maximumDepth, pDCS) =
//    let maxAllowedRisk = pDCSToRisk pDCS
//    allInputs
//    |> getAllSolutionsForThisProblem  (integrationTime, controlToIntegration) (bottomTime, maximumDepth, pDCS)
//    |> Array.zip allInputs 
//    |> Array.filter (fun  (inputVec, result )  -> result.TotalRisk < maxAllowedRisk )
//    |> Array.sortBy ( fun (inputV, res) -> res.AscentTime)
//    |> Array.map resultsToArray

let test = allInputs |> Array.groupBy ( fun x -> x.[2])
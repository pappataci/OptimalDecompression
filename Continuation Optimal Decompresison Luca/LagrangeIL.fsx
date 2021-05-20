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

//Initial Condition Grid Definition
//let bottomTimes = [|30.0 .. 10.0 .. 60.0|] |> Array.toSeq
//let maxDepths = [|90.0 ; 105.0 ; 120.0  |] |> Array.toSeq
//let probsBound =  [|3.3e-2|]  // for now we inspect only the give probability bound
//let initCondsGrid = create3DGrid bottomTimes maxDepths probsBound

////Parameter Grid Definition
//let breakFracSeq = [ 0.01 .. 0.1 .. 0.99 ]@[ 0.99 ]
//                   |> List.toSeq   
//let exponents = [ -3.0 .. 0.25 .. 2.0 ] |> List.toSeq
//let paramsGrid = create2DGrid breakFracSeq exponents 

////Candidate Surface Times
//let timesToSurfVec = [1.0 ; 2.0; 5.0; 20.0; 50.0] @ [100.0 .. 50.0 .. 500.0] 
//                     |> Array.ofList
  
//let resultsParallel = initCondsGrid
//                     |> Array.Parallel.map 
//                       ( tryFindSolutionWithIncreasingTimesSeq integrationTimeSettings paramsGrid timesToSurfVec )
         
//let resultsTable = resultsParallel
//                   |> resultsToInputForWriter // called out' in FSI


let integrationTime, controlToIntegration = 0.1 , 1 
let integrationTimeSettings = integrationTime, controlToIntegration
let peakPressure = 1.5
let noRiskPressure = 0.7 // irrelevant as long as it is less than ambient

let initPressures =  [| [|peakPressure;noRiskPressure ; noRiskPressure|] ; 
                        [|noRiskPressure;peakPressure;noRiskPressure|] ;
                        [|noRiskPressure;noRiskPressure;peakPressure|] ; 
                        [|peakPressure;peakPressure;peakPressure|] |]

let simulator = simulateSurfaceWithInitPressures integrationTimeSettings
let press = initPressures
            |> Array.map simulator
    //Array.take 3 
   
let riskCompute getter = press 
                        |> getter 
                        |> Array.map  ( Seq.last >>  (fun (State  x) -> x.Risk.AccruedRisk) ) 
                        |> Array.sum

let riskUnc = riskCompute (Array.take 3)
let riskCoupled = riskCompute (fun x ->   [| x |> Array.last|] ) 


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
                        |> List.toSeq

//let allInputs = create3DGrid breakFracSeq exponents deltaTimeSurface



let integrationTime, controlToIntegration = 0.1 , 1 
let integrationTimeSettings = integrationTime, controlToIntegration

let pDCS = 3.3e-2
let maximumDepth = 120.0
let bottomTime = 30.0

let initCondition =  (bottomTime, maximumDepth, pDCS) 





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

#load "SeqExtension.fs"
open SeqExtension

let getSolutionWithTheseParams (integrationTime, controlToIntegration )  initCondition timeToSurface =
    {AscentTime  = 0.0 
     AscentRisk  = 0.0
     SurfaceRisk = 0.0
     TotalRisk   = 0.0 
     InitTimeAtSurface = 0.0 
     AscentHistory = None} |> Some 


//let getOptimalSolForThisInputCondition (breakExpParams:float[][]) (integrationTime, controlToIntegration) (initCondition:float[]) (deltaTimeToSurface:float[])=
//// sketching the algorithm
//    //let pDCS = initCondition.[2]
//    //let maxAllowedRisk = pDCSToRisk pDCS
//    let (getSolWithThisSurfaceTime: float-> Option<StrategyResults> ) = getSolutionWithTheseParams (integrationTime, controlToIntegration )  initCondition
//    let res = deltaTimeToSurface
//              |> SeqExtension.takeWhileWithLast (getSolWithThisSurfaceTime >> Option.isNone) 

  

let optimizationParams = create2DGrid breakFracSeq exponents|> Seq.toArray


let timeSurface = 100.0 // toy number
 
 


let solveProblemFcn integrationTimeSettings optimizationParams (initCondition:float[])  =  
    let maxAllowedRisk = pDCSToRisk initCondition.[2]
    optimizationParams 
    |> Array.map (simulateStratWithParams integrationTimeSettings  initCondition  timeSurface) 
    //|> Seq.toArray
    //|> SeqExtension.takeWhileWithLast ( hasExceededMaxRisk maxAllowedRisk )
    //|> Seq.tryLast
    //|> getLastIfValid maxAllowedRisk


let bottomTimes = [|30.0 .. 30.0 .. 150.0|] |> Array.toSeq
let maxDepths = [|60.0 .. 30.0 .. 300.0|] |> Array.toSeq
let probsBound = [|3.2e-2|] |> Array.toSeq // for now just solve for the desired probability

let initConditionsGrid = create3DGrid bottomTimes maxDepths probsBound


let testSeq = initConditionsGrid  
              |> Array.map ( solveProblemFcn integrationTimeSettings (optimizationParams  |> Array.take 250 )  )

let testParallel = initConditionsGrid 
                   |> Array.Parallel.map ( solveProblemFcn integrationTimeSettings (optimizationParams |> Array.take 250 )  )

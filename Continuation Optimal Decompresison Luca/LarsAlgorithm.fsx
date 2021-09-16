#r @"C:\Users\glddm\source\repos\DecompressionL20190920A\packages\Extreme.Numerics.7.0.15\lib\net46\Extreme.Numerics.dll"
#r @"C:\Users\glddm\source\repos\DecompressionL20190920A\packages\Microsoft.ML.Probabilistic.0.3.1912.403\lib\netstandard2.0\Microsoft.ML.Probabilistic.dll"
#r @"C:\Users\glddm\source\repos\DecompressionL20190920A\packages\FSharp.Data.3.3.3\lib\net45\FSharp.Data.dll"



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

//open System
//open InitDescent
//open LEModel
open Extreme.Mathematics
//open Extreme.Mathematics.Optimization
//open Extreme.Mathematics.LinearAlgebra
//open Extreme.Mathematics.EquationSolvers
//open InputDefinition

open TwoStepsSolution

let pDCS = 3.2e-2

let maximumDepth = 120.0
let integrationTime, controlToIntegration = 0.1 , 1 

let initialGuesss =    Vector.Create(0.6 , 0.3 ,  1.5 )
                       |>  ConstantInitGuess

//let solution, report  =   initialGuesss
//                          |> findOptimalAscentForThisDive (integrationTime, controlToIntegration)  (bottomTime, maximumDepth , pDCS ) 

//let bottomTimes = [|30.0 .. 5.0 .. 100.0|]


//let solutionsAtDifferentTimes  = bottomTimes 
//                                 |> Array.mapi (fun i  bottomTime -> printfn "%A" i    
//                                                                     findOptimalAscent3DProblem (integrationTime, controlToIntegration)  (bottomTime, maximumDepth , pDCS )  initialGuesss )


let bottomTime = 100.0
let result =  findOptimalAscent3DProblem (integrationTime, controlToIntegration)  (bottomTime, maximumDepth , pDCS )  initialGuesss 

result
 
let getOptimizer result  = match result with        
                           | Bounce x -> new   Extreme.Mathematics.Optimization.PowellOptimizer()
                           | Optimized (_ , opt ) -> opt

let opt = getOptimizer result 

let otherVec = Vector.Create(0.6,0.3, 175.0)

opt.ObjectiveFunction.Invoke(opt.Extremum )
opt.ObjectiveFunction.Invoke(otherVec )

#r @"C:\Users\glddm\source\repos\DecompressionL20190920A\packages\Extreme.Numerics.7.0.15\lib\net46\Extreme.Numerics.dll"
#r @"C:\Users\glddm\source\repos\DecompressionL20190920A\packages\Microsoft.ML.Probabilistic.0.3.1912.403\lib\netstandard2.0\Microsoft.ML.Probabilistic.dll"

#load "SeqExtension.fs"
#load "PredefinedDescent.fs"
#load "ReinforcementLearning.fs"
#load "Gas.fs"
#load "LEModel.fs"
#load "AscentBuilder.fs"
 
open Extreme.Mathematics
open LEModel
open AscentBuilder

let integrationTime = 0.1
let initTime = 102.0  // mins
let maxDepth = 60.0 // ft
let targetDepth = 5.0

let myParams   = Vector.Create (-10.0, 50.0 , 0.0,  30.0 , 12.0,  // first leg with constant times 
                                -20.0, 25.0 , -1.0, 18.0,  1.5,  // second leg
                                -8.0 , 12.0 , 1.5  , 2.5  )       // third leg 

let testInitState = createFictitiouStateFromDepthTime (initTime, maxDepth) 

let out = createThreeLegAscentWithTheseBCs testInitState targetDepth integrationTime myParams
          |> Seq.toArray
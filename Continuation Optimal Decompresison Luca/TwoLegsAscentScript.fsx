#r @"C:\Users\glddm\source\repos\DecompressionL20190920A\packages\Microsoft.ML.Probabilistic.0.3.1912.403\lib\netstandard2.0\Microsoft.ML.Probabilistic.dll"
#r @"C:\Users\glddm\source\repos\DecompressionL20190920A\packages\FSharp.Data.3.3.3\lib\net45\FSharp.Data.dll"

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
#load "Result2CSV.fs"

//open ReinforcementLearning
open InitDescent
open LEModel
open AscentSimulator


//let stepsFromMaxToTarget ,                         initialDepthParams                    ,              twoLegAscentParams                                 , maxAscentRate  = 
//              4          ,  {MaxDepth = 120.0 ; BottomTime = 50.0 ; TargetDepth = 70.0 } , { ConstantDepth  = 30.0   ;  TimeStepsAtConstantDepth  = 150   } ,    Some -30.0

//let   resultVector , immersionAnalytics   , simulationOutput     = solveThis2LegAscent stepsFromMaxToTarget initialDepthParams twoLegAscentParams maxAscentRate 




                             

// INPUT DATA

let targetDepth = 30.0

let seqStepsMaxTarget = seq{1;2;3;4}
let seqMaxDepth = seq{60.0 .. 30.0 .. 180.0}
let seqBottomTimes = seq{30.0 .. 30.0 .. 300.0}
let seqConstDepth = seq{20.0 .. -10.0 .. 0.0}
let seqTimeAtConstDepth = seq{ 1 .. 10 .. 200}
let seqAscentRates = seq{-30.0 ; -20.0 ; -10.0 ; -5.0  }

let results = solveThisAscentForThisTargetDepth targetDepth  seqStepsMaxTarget seqMaxDepth seqBottomTimes  seqConstDepth seqTimeAtConstDepth seqAscentRates

//(1, { MaxDepth = 60.0
//      BottomTime = 30.0
//      TargetDepth = 30.0 }, { ConstantDepth = 20.0
//                              TimeStepsAtConstantDepth = 1 }, Some -30.0) |> ( fun ( x,y , z,  q) -> solveThis2LegAscent x y z q  ) 

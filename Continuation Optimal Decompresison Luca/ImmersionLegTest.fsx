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

open InitDescent
open LEModel
open AscentSimulator
open OneLegStrategy
open TwoStepsSolIl
open InputDefinition
//discretizeConstantDescentPath

let descentParams = {DescentRate = 75.0
                     MaximumDepth = 30.0}
let descentLegParams = {DescentLegParams = descentParams
                        BottomTime = 371.0}

let deltaTime = 0.1
let initialTimeDepth = {Time = 0.0 ; Value = 0.0} |> TemporalValue
let descentRate = descentParams.DescentRate
//let out = discretizeConstantDescentPath descentLegParams deltaTime

let sequenceOfDescendingDepthsAtConstantRate descentRate initialTimeDepth =
    let updateDepthWithThisRate descentRate initDepth = initDepth + descentRate * deltaTime
    let updateDepthAtDescendingRate = updateDepthWithThisRate descentRate

    let updateTime initTime = initTime + deltaTime

    let folder ( TemporalValue {Value = actualDepth ; Time = actualTime} )  _  = 
        TemporalValue { Value = updateDepthAtDescendingRate actualDepth; 
          Time =  updateTime  actualTime } 

    {1.0 .. infinity}
    |>  Seq.scan folder initialTimeDepth // initial time depth

let initialDepth = {Time = 0.0 ; Value = 0.0} 

let hasNotReachedTheBottom (TemporalValue aDepthInTime) =
    aDepthInTime.Value <= (descentLegParams.DescentLegParams.MaximumDepth + tolerance)

let descendingSequenceOfTemporalDepths =  
    sequenceOfDescendingDepthsAtConstantRate descentLegParams.DescentLegParams.DescentRate  (TemporalValue initialDepth)
    |> Seq.takeWhile hasNotReachedTheBottom

let constantDepthAtMaxDepth = 
    let initialDepthTemporalNode = descendingSequenceOfTemporalDepths |> Seq.last 
    let descendingRate = 0.0 
    let untilBottomTimeIsReached (TemporalValue temporalValue) =
        temporalValue.Time <= (descentLegParams.BottomTime + tolerance )
    sequenceOfDescendingDepthsAtConstantRate descendingRate initialDepthTemporalNode
    |> Seq.takeWhile untilBottomTimeIsReached
    |> Seq.skip 1  // get rid of common node

let out = sequenceOfDescendingDepthsAtConstantRate descentRate initialTimeDepth

let discreteDescent = discretizeConstantDescentPath descentLegParams deltaTime
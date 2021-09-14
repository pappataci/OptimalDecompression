#r @"C:\Users\glddm\source\repos\DecompressionL20190920A\packages\Extreme.Numerics.7.0.15\lib\net46\Extreme.Numerics.dll"


#load "SeqExtension.fs"
#load "Gas.fs"
#load "ELModelCommon.fs"
#load "TrinomialModel.fs"
#load "TableDataInputs.fs"
#load "TableReader.fs"
#load "ProfileIntegrator.fs"
#load "MissionDefinerFromTables.fs"
#load "SurfaceTableCreator.fs"
#load "MinimalTimeSearcher.fs"

open ModelRunner
open MinimalSearcher
open Extreme.Mathematics
open Extreme.Mathematics.Optimization
open Extreme.Mathematics.LinearAlgebra
open Extreme.Mathematics.EquationSolvers

let profilingOutput  = fileName
                                |> getDataContent
                                |> Array.map data2SequenceOfDepthAndTime

let _ , missionInfos = profilingOutput |> Array.unzip

let solutions = profilingOutput |> Array.Parallel.map  ( fun( x,   _ )  -> runModelOnProfile x ) 

//let tableInitialConditions' = profilingOutput |> Array.Parallel.map getInitialConditionAndTargetForTable
let tableInitialConditions = Array.map2 getInitialConditionsFromSolution solutions missionInfos

type InitialGuesser = | InitialGuessFcn of (Node -> DenseVector<float> ) 
type TrajectoryGenerator = | TrajGen of ( double -> Node  ->  DenseVector<float> -> Trajectory ) // decision time -> initialNode -> curveParams -> seq Of Depth and Time
    

let decisionTime = 1.0
let initialNode = tableInitialConditions.[330].InitAscentNode

let breakFraction = 0.6
let powerCoeff = 11.0
let tau = 5.5

let curveParams = Vector.Create(breakFraction, powerCoeff, tau)

let curveGen = linPowerCurveGenerator decisionTime initialNode curveParams


let curveText (curveGen:seq<DepthTime>) = 
    curveGen
    |> Seq.map (fun x -> x.Time.ToString() + ",  " + x.Depth.ToString())


let curveDescriptions = curveGen |> curveText

open System.IO
File.WriteAllLines(@"C:\Users\glddm\Desktop\New folder\text.txt" , curveDescriptions)

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

open ModelRunner

open Extreme.Mathematics
open Extreme.Mathematics.Optimization
open Extreme.Mathematics.LinearAlgebra
open Extreme.Mathematics.EquationSolvers

// 

let profilingOutput  = fileName
                                |> getDataContent
                                |> Array.map data2SequenceOfDepthAndTime

let _ , missionInfos = profilingOutput |> Array.unzip

let solutions = profilingOutput |> Array.Parallel.map  ( fun( x,   _ )  -> runModelOnProfile x ) 

//let tableInitialConditions' = profilingOutput |> Array.Parallel.map getInitialConditionAndTargetForTable
let tableInitialConditions = Array.map2 getInitialConditionsFromSolution solutions missionInfos

type InitialGuesser = | InitialGuessFcn of (Node -> DenseVector<float> ) 
type TrajectoryGenerator = | TrajGen of ( double -> Node  ->  DenseVector<float> -> Trajectory ) // decision time -> initialNode -> curveParams -> seq Of Depth and Time

let linPowerCurveGenerator  (decisionTime:double) (initialNode:Node) (curveParams:DenseVector<float>) : seq<DepthTime> = 
    
    let initialDepth = initialNode.EnvInfo.Depth
    let targetDepth  = curveParams.[0] * initialDepth

    let getNextLinDepthTimeWTargetDepth (currentDepthAndTime:DepthTime) targetDepth = 
        (targetDepth - initialDepth)/ascentRate

    let getNextLineDepthTime (currentDepthTime:DepthTime) deltaT = 
        
        let tentativeTargetDepth = currentDepthTime.Depth + ascentRate * deltaT
        match tentativeTargetDepth with 
        | x when x < 0.0 -> { Depth  = 0.0
                              Time = -currentDepthTime.Depth / ascentRate } 
                              
        | _ -> {Depth = tentativeTargetDepth 
                Time = currentDepthTime.Time + deltaT}
    

    //let getNextNonlinDepthTimeWTargetDepth 

         

    
    seq{{Time = 0.0; Depth = 0.0}} // dummy result 



//let linear

let runOptimizationForThisTableEntry (tableEntry:TableMissionMetrics) 
                                     (InitialGuessFcn initialGuessFcn)
                                     (TrajGen trajectoryGenerator )  = 
    
    let initialNode = tableEntry.InitAscentNode
    let initialGuess = initialGuessFcn initialNode
    


    0.0
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
    
    let breakFraction = curveParams.[0]
    let powerCoeff = curveParams.[1] // this has to be positive
    let tau = curveParams.[2] // this has to be positive
 
    let initialDepth = initialNode.EnvInfo.Depth
    let endLinearPartDepth  = breakFraction  * initialDepth
    let endLinearPartTime   = (endLinearPartDepth - initialDepth)/ascentRate

    let endLinearPartDepthTime = {Time = endLinearPartTime; Depth = endLinearPartDepth} 

    let getNextLinDepthTimeWTargetDepth (currentDepthAndTime:DepthTime) targetDepth = 
        (targetDepth - initialDepth)/ascentRate


    let getNextLinDepthTime deltaT (currentDepthTime:DepthTime)  =      
        let tentativeTargetDepth = currentDepthTime.Depth + ascentRate * deltaT

        match tentativeTargetDepth with 
        | x when x < 0.0 -> { Depth  = 0.0
                              Time = -currentDepthTime.Depth / ascentRate } 
                              
        | _ -> {Depth = tentativeTargetDepth 
                Time = currentDepthTime.Time + deltaT}
    
    let getNextCurveDepthTime  (currentDepthTime: DepthTime)  deltaT=
        if currentDepthTime.Depth < 0.0 then 
            let evaluateCurveAtThisTime (evaluationTime:double) = 
                endLinearPartDepth - endLinearPartDepth *  ( ( (evaluationTime - endLinearPartTime) / tau ) ** powerCoeff )
        
            let currentCurveValue = evaluateCurveAtThisTime (currentDepthTime.Time)
            let nextCurveValueAtT = evaluateCurveAtThisTime (currentDepthTime.Time + deltaT)

            let curveIncrement = max( nextCurveValueAtT - currentCurveValue) ascentRate*deltaT  

            let nextCandidateDepth = curveIncrement + currentDepthTime.Depth

            let candidateDepthTime = {Time = currentDepthTime.Time + deltaT
                                      Depth = curveIncrement + currentDepthTime.Depth}

            match nextCandidateDepth with
            | x when x < 0.0 -> { Depth = 0.0 
                                  Time = -currentDepthTime.Depth / curveIncrement  }
            | _ -> {Depth = candidateDepthTime.Depth
                    Time = currentDepthTime.Time + deltaT}
        else
            {Depth = -999.9
             Time  = currentDepthTime.Time }
        
    let nonlinearDecisionSeq = (fun _ -> decisionTime)
                               |> Seq.initInfinite 
                               |> Seq.scan getNextCurveDepthTime endLinearPartDepthTime
                               |> Seq.takeWhile ( fun x -> x.Depth >= 0.0 )
    
    seq{yield endLinearPartDepthTime 
        yield! nonlinearDecisionSeq }
    


//let linear

let runOptimizationForThisTableEntry (tableEntry:TableMissionMetrics) 
                                     (InitialGuessFcn initialGuessFcn)
                                     (TrajGen trajectoryGenerator )  = 
    
    let initialNode = tableEntry.InitAscentNode
    let initialGuess = initialGuessFcn initialNode
    


    0.0
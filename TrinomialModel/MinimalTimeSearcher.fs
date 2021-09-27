namespace MinimalSearcher
open Extreme.Mathematics
open Extreme.Mathematics.Optimization
open Extreme.Mathematics.LinearAlgebra
//open Extreme.Mathematics.EquationSolvers


[<AutoOpen>]
module MinimalTimeSearcher = 
    
    type InitialGuesser = | InitialGuessFcn of (Node -> DenseVector<float> ) 
    

    // values are scaled
    let powerCoeffScale = 10.0
    let tauScale = 100.0

    let linPowerCurveGenerator  (decisionTime:double) (initialNode:Node) (curveParams:Vector<float>) : seq<DepthTime> = 
        
        let breakFraction = curveParams.[0] //between 0 and 1
        let powerCoeff = curveParams.[1] * powerCoeffScale// this has to be positive
        let tau = curveParams.[2] * tauScale// this has to be positive //TO DO: add linear time (that is the minimum time to go to surface)
 
        let initialDepth = initialNode.EnvInfo.Depth
        let endLinearPartDepth  = breakFraction  * initialDepth
        let endLinearPartTime   = (endLinearPartDepth - initialDepth)/ascentRate

        let endLinearPartDepthTime = {Time = endLinearPartTime; Depth = endLinearPartDepth} 
    
        let getNextCurveDepthTime  (currentDepthTime: DepthTime)  deltaT=
            if currentDepthTime.Depth > 0.0 then 
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
                                      Time = currentDepthTime.Time - currentDepthTime.Depth / curveIncrement  }
                | _ -> {Depth = candidateDepthTime.Depth
                        Time = currentDepthTime.Time + deltaT}
            else
                {Depth = -999.9
                 Time  = currentDepthTime.Time }

        let nonlinearDecisionSeq = (fun _ -> decisionTime)
                                   |> Seq.initInfinite 
                                   |> Seq.scan getNextCurveDepthTime endLinearPartDepthTime
                                   |> Seq.takeWhile ( fun x -> x.Depth >= 0.0 )
                                   |> Seq.skip 1
    
        seq{(*yield initialNode.EnvInfo*)
            yield endLinearPartDepthTime 
            yield! nonlinearDecisionSeq }

    let curveStrategyToString (curveStrategy:seq<DepthTime>) = 
        curveStrategy
        |> Seq.map (fun x -> x.Time.ToString() + ",  " + x.Depth.ToString())


[<AutoOpen>]
module SimParams = 
    let decisionTime = 1.0  // [min]




[<AutoOpen>]
module OptimizationParams = 
    let penaltyForExceedingRisk = 100000.0
    let penaltyForViolatingConstraints = 100000.0
    
    let breakLower, breakUpper = 0.001, 0.9999
    let powerLower, powerUpper = 0.0001, 1.5 
    let tauLower, tauUpper   = 0.01,  50.0

[<AutoOpen>]
module ConstraintsDefinition = 

    let penaltyForRisk (remainingRisk) = 
        if (remainingRisk  >= 0.0) 
            then 0.0
        else 
            penaltyForExceedingRisk

    let createFunctionBounds (lowerBound, upperBound) (penaltyLower, penaltyUpper) = 
        let inner (inputArg:float) = 
            match inputArg with
            | x when x < lowerBound -> penaltyLower
            | x when x > upperBound -> penaltyUpper
            | _ -> 0.0
        inner

[<AutoOpen>]
module StrategyToDisk = 
    open System.IO

    let writeStringSeqToDisk fileName (stringSeq:seq<string>) = 
        File.WriteAllLines(fileName , stringSeq)
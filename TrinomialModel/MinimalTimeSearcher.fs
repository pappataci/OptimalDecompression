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
    let tauLower, tauUpper   = 0.01,  15.0
    let defaultPenalty = 100000.0
    
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

    let createFunctionBoundsDefaultPenalty (lowerBound, upperBound) = 
        createFunctionBounds (lowerBound, upperBound) (defaultPenalty, defaultPenalty)


    let variableLimits = [| breakLower, breakUpper  
                            powerLower, powerUpper 
                            tauLower, tauUpper     |]
    
    let listOfValidators = variableLimits
                           |> Array.map createFunctionBoundsDefaultPenalty

    
    let costValidator (listOfValidators: seq<float -> float> )  
                      (actualCost:Vector<float> ->double) = 
    
        let validatedCost (strategyParams:Vector<float>)  = 
            let penaltiesFromInput = strategyParams
                                     |> Seq.map2 (fun f x -> f x ) listOfValidators 
                                     |> Seq.sum 
            match penaltiesFromInput with 
            | 0.0 -> actualCost  strategyParams
            | _ -> penaltiesFromInput
                   
        validatedCost

[<AutoOpen>]
module OptimizerSettings = 
    
    open ModelRunner

    let optimizerMaxIterations = 5000
    let contractionFactor, expansionFactor, reflectionFactor = 0.75 , 
                                                               1.75 , 
                                                               -1.75

    let getNelderMead(initGuess:Vector<float>)  objectiveFunction   = 
        let nm = NelderMeadOptimizer()
        nm.ExtremumType  <- ExtremumType.Minimum
        nm.Dimensions <- initGuess.Length
        nm.InitialGuess <- initGuess
        nm.ObjectiveFunction <- objectiveFunction
        nm.MaxIterations <- optimizerMaxIterations
        nm.ContractionFactor <- contractionFactor
        nm.ExpansionFactor <- expansionFactor
        nm.ReflectionFactor <- reflectionFactor
        nm

    let targetFcnDefinition (initialNode:Node) (riskBound:double) :System.Func<Vector<float> , double>= 
        let unconstrainedCostComputation (strategyParams:Vector<float> ) =
    
            let solution = strategyParams
                            |> linPowerCurveGenerator decisionTime initialNode 
                            |> runModelOnProfile initialNode
            let (ascentTime, totalAccruedRisk) = getSimulationMetric(solution) 
            let cost = ascentTime + penaltyForRisk (riskBound - totalAccruedRisk)
            //addToLogger(seq{yield strategyParams.ToString(); yield ascentTime.ToString(); 
            //                yield totalAccruedRisk.ToString(); 
            //                yield cost.ToString() })
            cost
    
        let validatedCost = costValidator listOfValidators unconstrainedCostComputation            
        System.Func<_,_> validatedCost

    let defineObjectiveFunction (missionMetrics:TableMissionMetrics) = 
        let initialMissionNode = missionMetrics.InitAscentNode
        let riskBound = missionMetrics.TotalRisk
        targetFcnDefinition initialMissionNode riskBound

    let solveCurveGenProblem (InitialGuessFcn initialGuesser) (missionMetrics:TableMissionMetrics) = 
        let initialMissionNode = missionMetrics.InitAscentNode
        let initialGuess = initialGuesser initialMissionNode
        let objectiveFunction =  defineObjectiveFunction missionMetrics
        let optimizer = getNelderMead initialGuess objectiveFunction
        optimizer.FindExtremum() |> ignore  // (optimal search: internal state of the optimizer is affected)
        optimizer

    let createStaticInitialGuesser (breakFraction:float, powerCoeff, tau) : InitialGuesser = 
        let curveParams = Vector.Create(breakFraction, powerCoeff, tau)
        (fun _ -> curveParams) 
        |> InitialGuessFcn

[<AutoOpen>]
module OptimizerVsTableComparison = 
    open Utilities
    open ModelRunner

    type OptimalSolutionResult = {OptimalVsOriginalAscentTimeDiff: double
                                  SearchTime: double
                                  MissionInfo : MissionInfo
                                  OptimalRisk : double
                                  TableRisk : double 
                                  OptimalVsOriginalPercRiskDiff : double
                                  OptimalAscentTime : double
                                  OptimalAscentCurve: seq<DepthTime>
                                  OptimalCurveSolution: seq<Node>
                                  OptimalValues: DenseVector<float> }

    let getOptimizedVsTableComparison initialGuesser (tableMissionMetrics:TableMissionMetrics) = 
    
        let missionInfo = tableMissionMetrics.MissionInfo
        let optimizeMission = solveCurveGenProblem initialGuesser

        let optimizer , searchTime = timeThis optimizeMission tableMissionMetrics
        let initialNode = tableMissionMetrics.InitAscentNode
        let optimalCurve = linPowerCurveGenerator decisionTime initialNode  optimizer.Extremum
        let modelSolutionOnOptimalCurve = runModelOnProfile initialNode optimalCurve
        let optimalAscentTime, optimalRisk = getSimulationMetric modelSolutionOnOptimalCurve
        let optimalVsOriginalAscent = percentComparison optimalAscentTime missionInfo.TotalAscentTime
    
        {OptimalVsOriginalAscentTimeDiff = optimalVsOriginalAscent
         SearchTime = searchTime
         MissionInfo = missionInfo
         OptimalRisk = optimalRisk
         TableRisk = tableMissionMetrics.TotalRisk
         OptimalVsOriginalPercRiskDiff = percentComparison optimalRisk tableMissionMetrics.TotalRisk
         OptimalAscentTime = optimalAscentTime
         OptimalAscentCurve = optimalCurve
         OptimalCurveSolution = modelSolutionOnOptimalCurve
         OptimalValues = optimizer.Extremum}

[<AutoOpen>]
module StrategyToDisk = 
    open System.IO

    let writeStringSeqToDisk fileName (stringSeq:seq<string>) = 
        File.WriteAllLines(fileName , stringSeq)
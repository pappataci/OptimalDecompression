#r @"C:\Users\glddm\source\repos\DecompressionL20190920A\packages\Extreme.Numerics.7.0.15\lib\net46\Extreme.Numerics.dll"
#load "Logger.fs"


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
open Logger

let profilingOutput  = table9FileName
                                |> getDataContent
                                |> Array.map data2SequenceOfDepthAndTime

let seqDepthAndTime, missionInfos = profilingOutput |> Array.unzip


let solutions = seqDepthAndTime |> Array.Parallel.map  runModelOnProfileUsingFirstDepthAsInitNode  

let tableInitialConditions = Array.map2 getInitialConditionsFromSolution solutions missionInfos

type InitialGuesser = | InitialGuessFcn of (Node -> DenseVector<float> ) 
type TrajectoryGenerator = | TrajGen of ( double -> Node  ->  DenseVector<float> -> Trajectory ) // decision time -> initialNode -> curveParams -> seq Of Depth and Time
    
//let decisionTime = 1.0



//let curveGen = linPowerCurveGenerator decisionTime initialNode curveParams

//let strategyCurve = curveGen |> curveStrategyToString
//let outputStrategyFileName = @"C:\Users\glddm\Desktop\New folder\text.txt"
//strategyCurve
//|>  writeStringSeqToDisk   outputStrategyFileName

let getSimulationMetric(simSolution : seq<Node>) = 

    simSolution
    |> Seq.last
    |> (fun lastNode -> lastNode.AscentTime , lastNode.TotalRisk)
    
let createFunctionBounds (lowerBound, upperBound) (penaltyLower, penaltyUpper) = 
    let inner (inputArg:float) = 
        match inputArg with
        | x when x < lowerBound -> penaltyLower
        | x when x > upperBound -> penaltyUpper
        | _ -> 0.0
    inner



let breakLower, breakUpper = 0.001, 0.9999
let powerLower, powerUpper = 0.0001, 1.5 
let tauLower, tauUpper   = 0.01,  50.0

let defaultPenalty = 100000.0

let boundsForBreakPoint = createFunctionBounds (breakLower, breakUpper) (defaultPenalty , defaultPenalty )
let boundsForPowerCoeff = createFunctionBounds (powerLower, powerUpper ) (defaultPenalty , defaultPenalty )
let boundsForTau = createFunctionBounds (tauLower, tauUpper ) (defaultPenalty , defaultPenalty )

let listOfValidators = [|boundsForBreakPoint ; boundsForPowerCoeff ; boundsForTau|]

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

let targetFcnDefinition (initialNode:Node) (riskBound:double) :System.Func<Vector<float> , double>= 
    let costComputation (strategyParams:Vector<float> ) =

        let solution = strategyParams
                        |> linPowerCurveGenerator decisionTime initialNode 
                        |> runModelOnProfileUsingFirstDepthAsInitNode
        let (ascentTime, totalAccruedRisk) = getSimulationMetric(solution) 
        let cost = ascentTime + penaltyForRisk (riskBound - totalAccruedRisk)
        addToLogger(seq{yield strategyParams.ToString(); yield ascentTime.ToString(); 
                        yield totalAccruedRisk.ToString(); 
                        yield cost.ToString() })
        cost

    let validatedCost = costValidator listOfValidators costComputation
        
    System.Func<_,_> validatedCost


//let getPowellOptimizer(initGuess:Vector<float>)  objectiveFunction   = 
    
//    let pw = PowellOptimizer()
//    pw.ExtremumType  <- ExtremumType.Minimum
//    pw.Dimensions <- initGuess.Length
//    pw.InitialGuess <- initGuess
//    pw.ObjectiveFunction <- objectiveFunction
//    pw.MaxIterations <- 500000
//    pw

//let getNelderMead(initGuess:Vector<float>)  objectiveFunction   = 
    
//    let nm = NelderMeadOptimizer()
//    nm.ExtremumType  <- ExtremumType.Minimum
//    nm.Dimensions <- initGuess.Length
//    nm.InitialGuess <- initGuess
//    nm.ObjectiveFunction <- objectiveFunction
//    nm.MaxIterations <- 5000

//    nm.ContractionFactor <- 0.75
//    nm.ExpansionFactor <- 1.75
//    nm.ReflectionFactor <- -1.75
//    nm



//let defineObjectieFunction (missionMetrics:TableMissionMetrics) = 
//    let initialMissionNode = missionMetrics.InitAscentNode
//    let riskBound = missionMetrics.TotalRisk
//    targetFcnDefinition initialMissionNode riskBound

//let solveCurveGenProblem (InitialGuessFcn initialGuesser) (missionMetrics:TableMissionMetrics) = 
//    let initialMissionNode = missionMetrics.InitAscentNode
//    let initialGuess = initialGuesser initialMissionNode
//    let objectiveFunction =  defineObjectieFunction missionMetrics

//    let optimizer = getNelderMead initialGuess objectiveFunction

//    let optimalSolution = optimizer.FindExtremum()
//    optimizer


let breakFraction = 0.2
let powerCoeff = 0.4
let tau = 0.05;

let curveParams = Vector.Create(breakFraction, powerCoeff, tau)

let initialGuesser = (fun _ -> curveParams) |> InitialGuessFcn

 
let missionMetrics = tableInitialConditions.[10]
let initialMissionNode = missionMetrics.InitAscentNode
let riskBound = missionMetrics.TotalRisk
let initialGuess = (fun _ -> curveParams)   initialMissionNode

let objectiveFunction = targetFcnDefinition initialMissionNode riskBound
//let optimizerWithData = solveCurveGenProblem initialGuesser missionMetrics
//let optimalResult = optimizerWithData.Result

let tableEntry = 120

let initialNode = tableInitialConditions.[tableEntry].InitAscentNode


let testCurve = linPowerCurveGenerator decisionTime initialNode  curveParams


let testSolution = runModelOnProfileUsingFirstDepthAsInitNode testCurve


// Debugging 

let depthTimeSeq, missionInfo = profilingOutput.[tableEntry]
depthTimeSeq |> Seq.toArray

depthTimeSeq
|> runModelOnProfileUsingFirstDepthAsInitNode
|> Seq.toArray
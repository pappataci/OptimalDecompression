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

let profilingOutput  = table9FileName
                                |> getDataContent
                                |> Array.map data2SequenceOfDepthAndTime

let _ , missionInfos = profilingOutput |> Array.unzip

let solutions = profilingOutput |> Array.Parallel.map  ( fun( x,   _ )  -> runModelOnProfile x ) 

let tableInitialConditions = Array.map2 getInitialConditionsFromSolution solutions missionInfos

type InitialGuesser = | InitialGuessFcn of (Node -> DenseVector<float> ) 
type TrajectoryGenerator = | TrajGen of ( double -> Node  ->  DenseVector<float> -> Trajectory ) // decision time -> initialNode -> curveParams -> seq Of Depth and Time
    
//let decisionTime = 1.0
let initialNode = tableInitialConditions.[330].InitAscentNode

let breakFraction = 0.3
let powerCoeff = 0.2
let tau = 205.5

let curveParams = Vector.Create(breakFraction, powerCoeff, tau)

//let curveGen = linPowerCurveGenerator decisionTime initialNode curveParams

//let strategyCurve = curveGen |> curveStrategyToString
//let outputStrategyFileName = @"C:\Users\glddm\Desktop\New folder\text.txt"
//strategyCurve
//|>  writeStringSeqToDisk   outputStrategyFileName

let getSimulationMetric(simSolution : seq<Node>) = 
    let numberOfNodes = simSolution |> Seq.length 
    let lastNode = simSolution|> Seq.last 
    let previousToLast = simSolution |> Seq.item ( numberOfNodes - 2 )
    previousToLast.EnvInfo.Time ,  lastNode.TotalRisk
    
let targetFcnDefinition (initialNode:Node) (riskBound:double) :System.Func<Vector<float> , double>= 
    let costComputation (strategyParams:Vector<float> ) =
        let solution = strategyParams
                        |> linPowerCurveGenerator decisionTime initialNode 
                        |> runModelOnProfile
        let (ascentTime, totalAccruedRisk) = getSimulationMetric(solution)
        
        ascentTime + penaltyForRisk (riskBound - totalAccruedRisk) 
        
    System.Func<_,_> costComputation


let getPowellOptimizer(initGuess:Vector<float>)  objectiveFunction   = 
    
    let pw = PowellOptimizer()
    pw.ExtremumType  <- ExtremumType.Minimum
    pw.Dimensions <- initGuess.Length
    pw.InitialGuess <- initGuess
    pw.ObjectiveFunction <- objectiveFunction
    pw

//let optimizeThisProfile (TrajGen trajectoryGenerator) (InitialGuessFcn initGuessProvider)

let solveCurveGenProblem (InitialGuessFcn initialGuesser) (missionMetrics:TableMissionMetrics)  = 
    let initialMissionNode = missionMetrics.InitAscentNode
    let riskBound = missionMetrics.TotalRisk
    let initialGuess = initialGuesser initialMissionNode
    let objectiveFunction = targetFcnDefinition initialMissionNode riskBound

    let optimizer = getPowellOptimizer initialGuess objectiveFunction

    let optimalSolution = optimizer.FindExtremum()
    optimizer
    //let functionValue = optimizer.ValueAtExtremum

let initialGuesser = (fun _ -> curveParams) |> InitialGuessFcn

let missionMetrics = tableInitialConditions.[10]
let initialMissionNode = missionMetrics.InitAscentNode
let riskBound = missionMetrics.TotalRisk
let initialGuess = (fun _ -> curveParams)   initialMissionNode
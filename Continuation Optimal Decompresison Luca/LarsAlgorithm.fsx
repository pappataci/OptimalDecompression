#r @"C:\Users\glddm\source\repos\DecompressionL20190920A\packages\Extreme.Numerics.7.0.15\lib\net46\Extreme.Numerics.dll"
#r @"C:\Users\glddm\source\repos\DecompressionL20190920A\packages\Microsoft.ML.Probabilistic.0.3.1912.403\lib\netstandard2.0\Microsoft.ML.Probabilistic.dll"
#r @"C:\Users\glddm\source\repos\DecompressionL20190920A\packages\FSharp.Data.3.3.3\lib\net45\FSharp.Data.dll"
#load "ReinforcementLearning.fs"
#load "PredefinedDescent.fs"
#load "Gas.fs"
#load "LEModel.fs"
#load "OptimalAscentLearning.fs"
#load "InputDefinition.fs"
#load "EnvironmentToPython.fs"
#load "SeqExtension.fs"
#load "AscentSimulator.fs"
#load "AscentBuilder.fs"
#load "OneLegStrategy.fs"
#load "Result2CSV.fs"

open FSharp.Data
open System
open InitDescent
open LEModel
open Extreme.Mathematics
open Extreme.Mathematics.Optimization
open Extreme.Mathematics.LinearAlgebra
open Extreme.Mathematics.EquationSolvers
open InputDefinition

// general settings
let integrationTime, controlToIntegration = 0.1 , 1 
let maxSimTime = 5.0e4  // this is irrelevant

let computeInitialGuess (bottomTime, maxDepth, pDCS) = 
    Vector.Create(0.5, 0.3) // external parameters are breakFraction and exponent


let getExponentialPart linearAscent deltaTimeToSurface  exponent (bottomTime:float, maximumDepth:float ) controlTime =
    
    let initTime, initDepth = match Seq.isEmpty linearAscent with
                              | true  -> bottomTime, maximumDepth
                              | false -> linearAscent |> Seq.last 
    
    let timeSteps =  ( deltaTimeToSurface / controlTime )  
                     |> int 
    
    let incrementTime idx = initTime + float(idx  ) * controlTime
    let estimatedDiscreteSurfTime = incrementTime timeSteps 
    let breakOutToSurf = estimatedDiscreteSurfTime - initTime

    let idx2TimeIncrement idx = (controlTime * (float  idx  ) )  

    let timeIncrementPercent idx = (idx2TimeIncrement idx) / (breakOutToSurf)  
    let linearPath idx =  MissionConstraints.ascentRateLimit * (idx2TimeIncrement idx ) + initDepth  
    let exponentialPath idx = initDepth  - initDepth * ( ( timeIncrementPercent idx )  ** exponent )

    Seq.init timeSteps ( fun idxSeq  -> let idx = idxSeq  + 1 
                                        let actualTime = incrementTime idx
                                        let actualDepth =  max   (linearPath idx)   (exponentialPath idx )
                                        actualTime   , actualDepth )

let generateAscentStrategy (initState:State<LEStatus>) (solutionParams:Vector<double>) deltaTimeToSurface controlTime = 
    let bottomTime = initState |> leStatus2ModelTime
    let maximumDepth = initState |> leStatus2Depth
    
    let breakFraction = solutionParams.[0]
    let exponent = solutionParams.[1]

    let linearAscent =  getSeqOfDepthsForLinearAscentSection  (bottomTime, maximumDepth)  MissionConstraints.ascentRateLimit (breakFraction*maximumDepth) controlTime
    let exponentialPart = getExponentialPart linearAscent deltaTimeToSurface exponent (bottomTime, maximumDepth ) controlTime

    seq { yield! linearAscent 
          yield! exponentialPart}



let targetFcn initState myEnv residualRiskBound solutionParams controlTime  = 
    let computeActualPDCSOfThisAscent actualSurfaceTime =
        // compute trajectory with params initState myEnv solutionParams actualSurfaceTime
        // compute residualRisk for this trajectory

        0.2
    Func<_,_> computeActualPDCSOfThisAscent 

let setupResidualRiskProblem initState myEnv residualRiskBound solutionParams  controlTime = 
    let bisectionSolver = BisectionSolver()
    let targetFunction = targetFcn initState myEnv residualRiskBound solutionParams controlTime 
    bisectionSolver.TargetFunction <- targetFunction
    bisectionSolver


let defineSurfaceTimeFcn initState myEnv residualRiskBound controlTime =

    let timeToSurfaceFcn (solutionParams : Vector<float>) = 
        let bisectionSolver = setupResidualRiskProblem initState myEnv residualRiskBound solutionParams controlTime 
        0.0
    
    Func<_,_> timeToSurfaceFcn

let setUpOptimizationProblem(initGuess:DenseVector<float>)  computeSurfaceTime = 
   let pw = PowellOptimizer()
   pw.ExtremumType  <- ExtremumType.Minimum
   pw.Dimensions <- initGuess.Length
   pw.InitialGuess <- initGuess
   pw.ObjectiveFunction <- computeSurfaceTime
   pw

let optimizeAscentForThisInitState  residualRiskBound  myEnv  initialGuess initState controlTime  =

    let computeSurfaceTime = defineSurfaceTimeFcn initState myEnv  residualRiskBound controlTime
     
    let pw = setUpOptimizationProblem initialGuess computeSurfaceTime 
    let optimalParams  =  pw.FindExtremum()
    let solutionReport = pw.SolutionReport
    optimalParams, solutionReport


// external fcn (then write down subfunctions)
let findOptimalAscentForThisDive (integrationTime, controlToIntegration)  (bottomTime, maximumDepth , pDCS )  =
    
    let controlTime = controlToIntegration 
                      |> float 
                      |> (*) integrationTime

    let initialGuess =  (bottomTime, maximumDepth , pDCS )  
                        |> computeInitialGuess    
    
    let initState , myEnv = initStateAndEnvDescent maxSimTime (integrationTime, controlToIntegration) maximumDepth bottomTime

    let residualRiskBound = pDCSToRisk pDCS

    initState  
    |>  optimizeAscentForThisInitState residualRiskBound  myEnv initialGuess controlTime


//let createTestingEnvironment (integrationTime, controlToIntegration)  = 
//// these are fictitious values just to get out the environment: since the initial state is replaced 
//    let  maxSimTime = 15000.0
//    let maximumDepth = 120.0 // ft
//    let bottomTime   = 4.0 // minutes
//    let _, myEnv =  initStateAndEnvDescent maxSimTime  (integrationTime, controlToIntegration)   maximumDepth  bottomTime
//    myEnv

// testing 

let maximumDepth  = 120.0
let bottomTime = 30.0

let initState , myEnv = initStateAndEnvDescent maxSimTime (integrationTime, controlToIntegration) maximumDepth bottomTime

//generateAscentStrategy (initState:State<LEStatus>) (solutionParams:Vector<double>) deltaTimeToSurface controlTime 

let breakFraction = 0.3
let exponent      = 2.5

let solutionParams = Vector.Create(breakFraction, exponent)

let deltaTimeToSurface = 12.0
let controlTime = integrationTime * (float controlToIntegration)

let ascentStrategy = generateAscentStrategy (initState ) (solutionParams ) deltaTimeToSurface controlTime 

strategyToDisk "testAscent.txt" None ascentStrategy
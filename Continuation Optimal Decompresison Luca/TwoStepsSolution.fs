module TwoStepsSolution

open System
open InitDescent
open LEModel
open Extreme.Mathematics
open Extreme.Mathematics.Optimization
open Extreme.Mathematics.LinearAlgebra
open Extreme.Mathematics.EquationSolvers
open InputDefinition

type InitialGuess = | ConstantInitGuess of (float*float)
                    | InitialConditionGuess of  ( (float*float*float) -> DenseVector<float> ) 

let maximumAscentTime = 2000.0
let maxSimTime = 5.0e4  // this is irrelevant

let mutable (lastOptimalSurfaceTime:Result<float,float>) = 0.0 |> Ok  // this is ugly, but necessary for now 

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
    
    let actualValue = solutionParams.[0]

    let breakFraction =  max ( min  actualValue   1.0) 0.0 
    
    let exponent = solutionParams.[1]

    let linearAscent =  getSeqOfDepthsForLinearAscentSection  (bottomTime, maximumDepth)  MissionConstraints.ascentRateLimit (breakFraction*maximumDepth) controlTime
    let exponentialPart = getExponentialPart linearAscent deltaTimeToSurface exponent (bottomTime, maximumDepth ) controlTime

    seq { yield! linearAscent 
          yield! exponentialPart}


let generateTargetFcn initState environment residualRiskBound solutionParams controlTime  = 
    let computeActualPDCSOfThisAscent deltaTimeToSurface =
        let ascentTrajectory = generateAscentStrategy initState solutionParams deltaTimeToSurface controlTime
        let strategyResults = getTimeAndAccruedRiskForThisStrategy environment initState ascentTrajectory
        strategyResults.TotalRisk - residualRiskBound

    Func<_,_> computeActualPDCSOfThisAscent 

let getMinimumAscentTimeFromBreakFraction initState (solutionParams: Vector<double>) = 
    let initDepth = initState|> leStatus2Depth
   

    let breakFraction =  max ( min  solutionParams.[0]   1.0) 0.0 
    let initExponentDescentDepth = initDepth * breakFraction
    -initExponentDescentDepth / MissionConstraints.ascentRateLimit

let setupResidualRiskProblem initState myEnv residualRiskBound solutionParams  controlTime = 
    
    let bisectionSolver = BisectionSolver()
    let targetFunction = generateTargetFcn initState myEnv residualRiskBound solutionParams controlTime 
    bisectionSolver.TargetFunction <- targetFunction
    bisectionSolver.LowerBound <- getMinimumAscentTimeFromBreakFraction  initState  solutionParams
    bisectionSolver.UpperBound <- maximumAscentTime // minutes
    bisectionSolver

let findSolutionWithSolver (solver:BisectionSolver) = 
    // this function has to be called after the solver has be built with all necessary datad
    // (i.e.: LowerBound, UpperBound, and TargetFunction)

    let valueFcnAtLower = solver.TargetFunction.Invoke solver.LowerBound
    let valueFcnAtUpper = solver.TargetFunction.Invoke solver.UpperBound

    match ( (valueFcnAtLower * valueFcnAtUpper)  < 0.0  )  with
    | true ->  solver.Solve() |> Ok 
    | false -> if (valueFcnAtLower > 0.0) then    
                    Ok solver.LowerBound
               else 
                    Error solver.LowerBound


let getSurfaceTimeFromBisectionSolution   = 
    function  
    | Ok okValue -> (okValue:float)
    | Error errorValue -> errorValue

let defineSurfaceTimeFcn initState myEnv residualRiskBound controlTime =

    let timeToSurfaceFcn (solutionParams : Vector<float>) = 
        let bisectionSolver = setupResidualRiskProblem initState myEnv residualRiskBound solutionParams controlTime 
        let bisectionSolverSolution = findSolutionWithSolver(bisectionSolver)

        // dump solution to global var
        lastOptimalSurfaceTime <- bisectionSolverSolution // necessary for solution report        
        getSurfaceTimeFromBisectionSolution bisectionSolverSolution

    Func<_,_> timeToSurfaceFcn

let setUpOptimizationProblem(initGuess:DenseVector<float>)  computeSurfaceTime gradient = 
   
   //let bfgs = QuasiNewtonOptimizer(QuasiNewtonMethod.Bfgs)

   //bfgs.InitialGuess <- initGuess
   //bfgs.ExtremumType <- ExtremumType.Minimum
   //bfgs.ObjectiveFunction <- computeSurfaceTime
   //bfgs.FastGradientFunction <- gradient
   //bfgs

   let pw = PowellOptimizer()
   pw.ExtremumType  <- ExtremumType.Minimum
   pw.Dimensions <- initGuess.Length
   pw.InitialGuess <- initGuess
   pw.ObjectiveFunction <- computeSurfaceTime
   
   pw

let optimizeAscentForThisInitState  residualRiskBound  myEnv  initialGuess controlTime initState   =
    let computeSurfaceTime = defineSurfaceTimeFcn initState myEnv  residualRiskBound controlTime

    let (gradient: Func<Vector<float>,Vector<float>, Vector<float>> )  = FunctionMath.GetNumericalGradient  computeSurfaceTime

    let pw = setUpOptimizationProblem initialGuess computeSurfaceTime  gradient
    let optimalParams  =  pw.FindExtremum()
    let solutionReport = pw.SolutionReport
    optimalParams, solutionReport


//let findOptimalAscentForThisDiveWithPenalty residualRiskBound myEnv initialGuess controlTime initState = 
    
let getOptimizationParams (integrationTime, controlToIntegration)  (bottomTime, maximumDepth , pDCS )  getInitialGuess = 
    
    let computeInitialGuess   = 
            match getInitialGuess with
            | ConstantInitGuess (breakFraction, exponent)  -> (fun (_,_,_) -> Vector.Create( breakFraction, exponent ) )
            | InitialConditionGuess guess -> guess 

    let controlTime = controlToIntegration 
                      |> float 
                      |> (*) integrationTime

    let initialGuess =  (bottomTime, maximumDepth , pDCS )  
                        |> computeInitialGuess 

    let initState , myEnv = initStateAndEnvDescent maxSimTime (integrationTime, controlToIntegration) maximumDepth bottomTime

    let residualRiskBound = pDCSToRisk pDCS

    initState, residualRiskBound , myEnv , initialGuess,  controlTime 

// external fcn (then write down subfunctions)
let findOptimalAscentForThisDive (integrationTime, controlToIntegration)  (bottomTime, maximumDepth , pDCS )  getInitialGuess    =

    let initState, residualRiskBound, myEnv, initialGuess, controlTime = 
        getOptimizationParams (integrationTime, controlToIntegration)  (bottomTime, maximumDepth , pDCS )  getInitialGuess
    
    initState  
    |>  optimizeAscentForThisInitState residualRiskBound  myEnv initialGuess controlTime
[<AutoOpen>]
module OneLegStrategy

open ReinforcementLearning
open InitDescent
open LEModel
open InputDefinition
open AscentSimulator

open Extreme.Mathematics.EquationSolvers
open Extreme.Mathematics

type StrategyResults = {AscentTime  : float 
                        AscentRisk  : float 
                        SurfaceRisk : float
                        TotalRisk   : float 
                        InitTimeAtSurface : float 
                        AscentHistory : Option<seq<seq<State<LEStatus>>>>}


let initStateAndEnvDescent maxSimTime  (integrationTime, controlToIntegration)   maximumDepth  bottomTime  = 
    let strategyOutput, myEnv = getInitConditionAfterDescentPhase (integrationTime, controlToIntegration, integrationTime ) (Some maxSimTime) 1   maximumDepth  bottomTime maximumDepth 
    let (Output initialConditionAfterDescent) = strategyOutput 
    let leState = initialConditionAfterDescent
                  |> Array.head
                  |> ( fun (state , _ , _ , _)  -> state ) 
    leState , myEnv

let  getSeqOfDepthsForLinearAscentSection  (initTime:float , initDepth) (slope:float) (targetDepth:float) controlTime  = 
   // output is seq of depths 
   let increment = controlTime * slope
   let depths = [|initDepth + increment .. increment .. targetDepth  |]
                |> Seq.ofArray

   let times = ( fun sampleNumber ->  initTime + (float(sampleNumber + 1 ))* controlTime)
               |> Seq.init  (depths|> Seq.length )  

   Seq.zip  times depths 

type GeneralAscentParams = { LinearSlope                    : float 
                             BreakFraction                  : float 
                             WaitingTime                    : Option<int>
                             Tay                            : Option<float>
                             TanhInitDerivative             : Option<float> }

let setOptionArgToDefaultIfNone  optionalValue defaultValue =
    match optionalValue with
    | Some value -> value
    | None -> defaultValue 

let ifEmptySeqTakeThisElementOtherwiseTakeLast strategyAscent (initTime:float, initDepth:float) = 
    match  ( strategyAscent |> Seq.isEmpty) with 
    | true  -> (initTime, initDepth)
    | false -> strategyAscent
               |> Seq.last

let getLinearAscentLeg (initTime,  initDepth) linearSlope targetDepth controlTime =
    let linearPart = getSeqOfDepthsForLinearAscentSection  (initTime, initDepth)  linearSlope targetDepth controlTime
    let nextTimeAndDepth = ifEmptySeqTakeThisElementOtherwiseTakeLast linearPart (initTime,  initDepth)
    linearPart , nextTimeAndDepth

let getConstantDepthLeg ( initTime, initDepth ) nTimeStepsToKeepDepth controlTime = 
    let constantLeg = Seq.init nTimeStepsToKeepDepth (fun kIndex -> float(kIndex + 1) * controlTime , initDepth )
    let nextTimeAndDepth = ifEmptySeqTakeThisElementOtherwiseTakeLast  constantLeg  ( initTime, initDepth )
    constantLeg , nextTimeAndDepth

let private getTanhAscentLeg  (initTime:float, initDepth:float ) initSlope  (tay: float ) targetDepth controlTime     =
    //tay parameter belongs to (-0.9, 0.0]: 0.0 being fastest, -0.9 slowest

    let b = tanh tay
    let a = initDepth / (tay + 1.0 )
    let k = initSlope / ( initDepth * ( tay - 1.0 ) )

    let tanhLeg t = 
        a * tanh ( -k * ( t - initTime ) + b ) + a ; 
    
    let times =  Seq.initInfinite ( fun sampleNumber ->  initTime + (float(sampleNumber + 1 ))* controlTime)
    let depths = times |> Seq.map tanhLeg
    
    let tanhLeg = depths 
                         |> Seq.zip times     
                         |> Seq.takeWhile ( fun (_, depth ) -> depth >=  targetDepth + MissionConstraints.depthTolerance)

    let nextTimeAndDepth = ifEmptySeqTakeThisElementOtherwiseTakeLast  tanhLeg  ( initTime, initDepth )
    tanhLeg , nextTimeAndDepth

let createAscentGeneralTrajectory controlTime (initTime, initDepth, targetDepth' : Option<float> ) (generalAscentParams: GeneralAscentParams) =
    let {LinearSlope = linearSlope 
         BreakFraction = breakFraction
         WaitingTime = holdingTime'
         Tay = tay' 
         TanhInitDerivative = tanhInitDerivative' } = generalAscentParams
    
    let targetDepth             = setOptionArgToDefaultIfNone targetDepth' 0.0
    let holdingTime             = setOptionArgToDefaultIfNone holdingTime' 0
    let tay                     = setOptionArgToDefaultIfNone tay' 0.0
    let tanhInitDerivative      = setOptionArgToDefaultIfNone tanhInitDerivative' linearSlope

    // First leg: linear part
    let targetLinearPart = (initDepth - targetDepth) * breakFraction + targetDepth
    let linearAscent, lastNodeForLinearAscent = getLinearAscentLeg  (initTime, initDepth)  linearSlope targetLinearPart controlTime

    // Second leg: constant depth 
    let constantDepth, lastNodeForConstantDepth = getConstantDepthLeg (lastNodeForLinearAscent) holdingTime controlTime

    // Third leg: tanh ascent 
    let tanhDepth , _       = getTanhAscentLeg lastNodeForConstantDepth  tanhInitDerivative tay targetDepth controlTime 

    seq{ yield! linearAscent 
         yield! constantDepth
         yield! tanhDepth}

let createAscentSimpleTrajectory controlTime (bottomTime, maximumDepth) (linearSlope, breakFraction, tay,tanhInitDerivative) = 
    let linearPart = getSeqOfDepthsForLinearAscentSection  (bottomTime, maximumDepth)  linearSlope (breakFraction*maximumDepth) controlTime
    
    let initTime , initDepth = match  ( linearPart |> Seq.isEmpty) with 
                               | true  -> (bottomTime, maximumDepth)
                               | false -> linearPart
                                          |> Seq.last

    let initTanhPart , _  =  getTanhAscentLeg (initTime , initDepth )  tanhInitDerivative tay 0.0 controlTime
                        
    match (initTanhPart |> Seq.isEmpty) with 
    | true  -> linearPart
    | _     -> ( seq{ yield! linearPart ; yield! initTanhPart}   ) 

let private   executeThisStrategy   (Environment environm: Environment<LEStatus, float, obj> )  (actualLEStatus:State<LEStatus>)  nextDepth   = 
    (environm actualLEStatus    nextDepth).EnvironmentFeedback.NextState

let   computeUpToSurface leInitState  ascentStrategy environment = 
    ascentStrategy
    |> Seq.scan (executeThisStrategy  environment) leInitState

let simulateStrategyUntilZeroRisk initStateAtSurface  environment =
    let infiniteSequenceOfZeroDepth = Seq.initInfinite ( fun _ -> Control  0.0)
    infiniteSequenceOfZeroDepth
    |> Seq.scan (executeThisStrategy environment) initStateAtSurface
    |> SeqExtension.takeWhileWithLast (fun leStatus -> leStatus2IsEmergedAndNotAccruingRisk leStatus ModelParams.threshold
                                                       |> not ) 
    |> Seq.skip 1  // skip the initial state which is just initStateAtSurface, so if sequences are merged it is not computed twice


let getTotalRisk ascentRisk (upToZeroRiskHistory:seq<State<LEStatus>>)   =
    match (upToZeroRiskHistory |> Seq.isEmpty) with
    | true -> ascentRisk
    | false -> upToZeroRiskHistory 
                |> Seq.last 
                |> leStatus2Risk 

let createAscentHistory (upToSurfaceHistory:seq<State<LEStatus>>) upToZeroRiskHistory =
    match  (upToZeroRiskHistory |> Seq.isEmpty) with
    | true -> seq { upToSurfaceHistory  } 
    | false -> seq {upToSurfaceHistory ; upToZeroRiskHistory} 

let getTimeAndAccruedRiskForThisStrategy environment leInitState (ascentTrajectory:seq<float*float>)  =
    let ascentStrategy = ascentTrajectory |> Seq.map (snd >> Control )
    let upToSurfaceHistory  = computeUpToSurface leInitState ascentStrategy environment
    
    let initStateAtSurface  = upToSurfaceHistory |> Seq.last 
    let upToZeroRiskHistory = simulateStrategyUntilZeroRisk initStateAtSurface  environment
    let initTimeAtSurface = initStateAtSurface |> leStatus2ModelTime
    let ascentTime = initTimeAtSurface - (leInitState |> leStatus2ModelTime)
    let ascentRisk = initStateAtSurface |> leStatus2Risk

    let totalRisk = upToZeroRiskHistory 
                    |> getTotalRisk ascentRisk
                    
    let surfaceRisk = totalRisk  - ascentRisk 

    let ascentHistory = createAscentHistory upToSurfaceHistory  upToZeroRiskHistory 
    
    {AscentTime    = ascentTime
     AscentRisk    = ascentRisk
     SurfaceRisk   = surfaceRisk 
     TotalRisk     =  totalRisk
     InitTimeAtSurface = initTimeAtSurface
     AscentHistory = Some ascentHistory }

// given all parameters solve for the current ascent (using bisection solver)




//let solver = BisectionSolver()
//solver.LowerBound <- 0.0
//solver.UpperBound <- 2.0

//solver.TargetFunction <- System.Func<_,_> (Math.Cos)
//let result = solver.Solve()

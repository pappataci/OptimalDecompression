#r @"C:\Users\glddm\source\repos\DecompressionL20190920A\packages\Extreme.Numerics.7.0.15\lib\net46\Extreme.Numerics.dll"
#r @"C:\Users\glddm\source\repos\DecompressionL20190920A\packages\Microsoft.ML.Probabilistic.0.3.1912.403\lib\netstandard2.0\Microsoft.ML.Probabilistic.dll"
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

open ReinforcementLearning
open InitDescent
open LEModel
open InputDefinition
open System
open Extreme.Mathematics
open Extreme.Mathematics.Optimization
open AscentSimulator
open AscentBuilder

let initStateAndEnvDescent maxSimTime  (integrationTime, controlToIntegration)   maximumDepth  bottomTime  = 
    let strategyOutput, myEnv = getInitConditionAfterDescentPhase (integrationTime, controlToIntegration, integrationTime ) (Some maxSimTime) 1   maximumDepth  bottomTime maximumDepth 
    let (Output initialConditionAfterDescent) = strategyOutput 
    let leState = initialConditionAfterDescent
                  |> Array.head
                  |> ( fun (state , _ , _ , _)  -> state ) 
    leState , myEnv

 // input example

let integrationTime, controlToIntegration = 0.1 , 1
let controlTime = integrationTime * (float controlToIntegration)
let  maxSimTime = 15000.0

// inputs specific to mission 
let maximumDepth = 200.0 // ft
let bottomTime   = 100.0 // minutes
let maxPDCS = 3.3e-2

// small test
let leInitState, myEnv =  initStateAndEnvDescent maxSimTime  (integrationTime, controlToIntegration)   maximumDepth  bottomTime

let getSeqOfDepthsForLinearAscentSection  (initTime:float , initDepth) (slope:float) (breakOut:float) controlTime  = 
   // output is seq of depths 
   let targetDepth = breakOut * initDepth
   let increment = controlTime * slope
   let depths = [|initDepth + increment .. increment .. targetDepth  |]
                |> Seq.ofArray

   let times = ( fun sampleNumber ->  initTime + (float(sampleNumber + 1 ))* controlTime)
               |> Seq.init  (depths|> Seq.length )  

   Seq.zip  times depths 
  
let breakOut = 0.2
let linearSlope = -10.0
let linearPartSeq = getSeqOfDepthsForLinearAscentSection  (0.0, maximumDepth)  linearSlope breakOut controlTime

let getArrayOfDepthsForTanhAscentSection controlTime initSlope  tay   (initTime:float, initDepth:float )  =
    //tay parameter belongs to (-1.0, 0.0]
    
    let b = tanh tay
    let a = initDepth / (tay + 1.0 )
    let k = initSlope / ( initDepth * ( tay - 1.0 ) )

    let tanhLeg t = 
        a * tanh ( -k * ( t - initTime ) + b ) + a ; 
    
    let times =  Seq.initInfinite ( fun sampleNumber ->  initTime + (float(sampleNumber + 1 ))* controlTime)
    let depths = times |> Seq.map tanhLeg
    
    let outputSequence = Seq.zip times depths 
    outputSequence
    

let createAscentTrajectory controlTime (bottomTime, maximumDepth) (linearSlope, breakOut) tay = 
    let linearPart = getSeqOfDepthsForLinearAscentSection  (bottomTime, maximumDepth)  linearSlope breakOut controlTime
    let initTanhPart = linearPart
                       |> Seq.last 
                       |> getArrayOfDepthsForTanhAscentSection controlTime linearSlope tay
                       |> Seq.takeWhile ( fun (_, depth ) -> depth >=  MissionConstraints.depthTolerance)
    match (initTanhPart |> Seq.isEmpty) with 
    | true  -> linearPart
    | _     -> (Seq.concat ( seq{ linearPart ; initTanhPart} ) ) 
    |> Seq.map snd 

let depthExample = createAscentTrajectory controlTime ( bottomTime, maximumDepth ) ( linearSlope, 0.3 ) 0.0

let ascentStrategyExample   = depthExample
                              |> Seq.map Control 

let executeThisStrategy   (Environment environm: Environment<LEStatus, float, obj> )  (actualLEStatus:State<LEStatus>)  nextDepth   = 
    (environm actualLEStatus    nextDepth).EnvironmentFeedback.NextState
    
let computeUpToSurface leInitState  ascentStrategy environment = 
    ascentStrategy
    |> Seq.scan (executeThisStrategy  environment) leInitState

let simulateStrategyUntilZeroRisk initStateAtSurface =
    let infiniteSequenceOfZeroDepth = Seq.initInfinite ( fun _ -> Control  0.0)
    infiniteSequenceOfZeroDepth
    |> Seq.scan (executeThisStrategy myEnv) initStateAtSurface
    |> SeqExtension.takeWhileWithLast (fun leStatus -> leStatus2IsEmergedAndNotAccruingRisk leStatus ModelParams.threshold
                                                       |> not ) 
    |> Seq.skip 1  // skip the initial state which is just initStateAtSurface, so if sequences are merged it is not computed twice

// small testing with the two branches (up to surface and at surface)
let upToSurfaceHistory = computeUpToSurface leInitState ascentStrategyExample myEnv
let initStateAtSurface = upToSurfaceHistory |> Seq.last 
let upToZeroRisk = simulateStrategyUntilZeroRisk initStateAtSurface

type StrategyResults = {AscentTime  : float 
                        AscentRisk  : float 
                        SurfaceRisk : float
                        InitTimeAtSurface : float 
                        AscentHistory : Option<seq<float>>}

let getTimeAndAccruedRiskForThisStrategy leInitState ascentStrategy environment =
    let upToSurfaceHistory  = computeUpToSurface leInitState ascentStrategy environment
    let initStateAtSurface  = upToSurfaceHistory |> Seq.last 
    let upToZeroRiskHistory = simulateStrategyUntilZeroRisk initStateAtSurface 
    let initTimeAtSurface = initStateAtSurface |> leStatus2ModelTime
    let ascentTime = initTimeAtSurface - (leInitState |> leStatus2ModelTime)
    let ascentRisk = initStateAtSurface |> leStatus2Risk
    let surfaceRisk = ( upToZeroRiskHistory 
                            |> Seq.last 
                            |> leStatus2Risk ) - ascentRisk 
    {AscentTime = ascentTime
     AscentRisk = ascentRisk
     SurfaceRisk = surfaceRisk 
     InitTimeAtSurface = initTimeAtSurface
     AscentHistory = None}
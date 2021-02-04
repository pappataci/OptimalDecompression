[<AutoOpen>]
module OneLegStrategy


open ReinforcementLearning
open InitDescent
open LEModel
open InputDefinition
open System
open Extreme.Mathematics
open Extreme.Mathematics.Optimization
open AscentSimulator
open AscentBuilder

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

let private getSeqOfDepthsForLinearAscentSection  (initTime:float , initDepth) (slope:float) (breakOut:float) controlTime  = 
   // output is seq of depths 
   let targetDepth = breakOut * initDepth
   let increment = controlTime * slope
   let depths = [|initDepth + increment .. increment .. targetDepth  |]
                |> Seq.ofArray

   let times = ( fun sampleNumber ->  initTime + (float(sampleNumber + 1 ))* controlTime)
               |> Seq.init  (depths|> Seq.length )  

   Seq.zip  times depths 

let private getArrayOfDepthsForTanhAscentSection controlTime initSlope  tay   (initTime:float, initDepth:float )  =
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

let createAscentTrajectory controlTime (bottomTime, maximumDepth) (linearSlope, breakOut, tay,tanhInitDerivative) = 
    let linearPart = getSeqOfDepthsForLinearAscentSection  (bottomTime, maximumDepth)  linearSlope breakOut controlTime
    let initTanhPart = linearPart
                       |> Seq.last 
                       |> getArrayOfDepthsForTanhAscentSection controlTime tanhInitDerivative tay
                       |> Seq.takeWhile ( fun (_, depth ) -> depth >=  MissionConstraints.depthTolerance)
    match (initTanhPart |> Seq.isEmpty) with 
    | true  -> linearPart
    | _     -> (Seq.concat ( seq{ linearPart ; initTanhPart} ) ) 
    
let private   executeThisStrategy   (Environment environm: Environment<LEStatus, float, obj> )  (actualLEStatus:State<LEStatus>)  nextDepth   = 
    (environm actualLEStatus    nextDepth).EnvironmentFeedback.NextState

let   computeUpToSurface leInitState  ascentStrategy environment = 
    ascentStrategy
    |> Seq.scan (executeThisStrategy  environment) leInitState

let   simulateStrategyUntilZeroRisk initStateAtSurface  environment=
    let infiniteSequenceOfZeroDepth = Seq.initInfinite ( fun _ -> Control  0.0)
    infiniteSequenceOfZeroDepth
    |> Seq.scan (executeThisStrategy environment) initStateAtSurface
    |> SeqExtension.takeWhileWithLast (fun leStatus -> leStatus2IsEmergedAndNotAccruingRisk leStatus ModelParams.threshold
                                                       |> not ) 
    |> Seq.skip 1  // skip the initial state which is just initStateAtSurface, so if sequences are merged it is not computed twice

let getTimeAndAccruedRiskForThisStrategy leInitState (ascentTrajectory:seq<float*float>) environment =
    let ascentStrategy = ascentTrajectory |> Seq.map (snd >> Control )
    let upToSurfaceHistory  = computeUpToSurface leInitState ascentStrategy environment
    let initStateAtSurface  = upToSurfaceHistory |> Seq.last 
    let upToZeroRiskHistory = simulateStrategyUntilZeroRisk initStateAtSurface  environment
    let initTimeAtSurface = initStateAtSurface |> leStatus2ModelTime
    let ascentTime = initTimeAtSurface - (leInitState |> leStatus2ModelTime)
    let ascentRisk = initStateAtSurface |> leStatus2Risk
    let totalRisk = ( upToZeroRiskHistory 
                      |> Seq.last 
                      |> leStatus2Risk ) 
    let surfaceRisk = totalRisk  - ascentRisk 

    let ascentHistory = seq { upToSurfaceHistory ; upToZeroRiskHistory }
    
    {AscentTime    = ascentTime
     AscentRisk    = ascentRisk
     SurfaceRisk   = surfaceRisk 
     TotalRisk     =  totalRisk
     InitTimeAtSurface = initTimeAtSurface
     AscentHistory = Some ascentHistory }
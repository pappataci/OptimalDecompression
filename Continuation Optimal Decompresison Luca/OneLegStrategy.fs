[<AutoOpen>]
module OneLegStrategy

open ReinforcementLearning
open InitDescent
open LEModel
open InputDefinition
open AscentSimulator

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

let private getSeqOfDepthsForLinearAscentSection  (initTime:float , initDepth) (slope:float) (targetDepth:float) controlTime  = 
   // output is seq of depths 
   let increment = controlTime * slope
   let depths = [|initDepth + increment .. increment .. targetDepth  |]
                |> Seq.ofArray

   let times = ( fun sampleNumber ->  initTime + (float(sampleNumber + 1 ))* controlTime)
               |> Seq.init  (depths|> Seq.length )  

   Seq.zip  times depths 

let private getArrayOfDepthsForTanhAscentSection controlTime initSlope  (tay':Option<float>)   (initTime:float, initDepth:float )  =
    //tay parameter belongs to (-1.0, 0.0]
    
    let tay = match tay' with 
              | Some tay -> tay 
              | None -> 0.0

    let b = tanh tay
    let a = initDepth / (tay + 1.0 )
    let k = initSlope / ( initDepth * ( tay - 1.0 ) )

    let tanhLeg t = 
        a * tanh ( -k * ( t - initTime ) + b ) + a ; 
    
    let times =  Seq.initInfinite ( fun sampleNumber ->  initTime + (float(sampleNumber + 1 ))* controlTime)
    let depths = times |> Seq.map tanhLeg
    
    let outputSequence = Seq.zip times depths 
    outputSequence


type GeneralAscentParams = { LinearSlope                    : float 
                             BreakFraction                  : float 
                             HoldingTimeInSamplingTimeUnits : Option<int>
                             Tay                            : Option<float>
                             TanhInitDerivative             : Option<float> }

let setOptionArgToDefaultIfNone  optionalValue defaultValue =
    match optionalValue with
    | Some value -> value
    | None -> defaultValue 

let createAscentGeneralTrajectory controlTime (bottomTime, maximumDepth) (generalAscentParams: GeneralAscentParams) =
    let {LinearSlope = linearSlope 
         BreakFraction = breakFraction
         HoldingTimeInSamplingTimeUnits = holdingTime
         Tay = tay 
         TanhInitDerivative = tanhInitDerivative } = generalAscentParams
    


    0.0

let createAscentSimpleTrajectory controlTime (bottomTime, maximumDepth) (linearSlope, breakOut, tay,tanhInitDerivative) = 
    let linearPart = getSeqOfDepthsForLinearAscentSection  (bottomTime, maximumDepth)  linearSlope (breakOut*maximumDepth) controlTime
    
    let initTime , initDepth = match  ( linearPart |> Seq.isEmpty) with 
                               | true  -> (bottomTime, maximumDepth)
                               | false -> linearPart
                                          |> Seq.last

    let initTanhPart =  (initTime , initDepth )
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
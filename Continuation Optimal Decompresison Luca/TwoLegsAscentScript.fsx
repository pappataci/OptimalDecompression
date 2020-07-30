open System

#r @"C:\Users\glddm\source\repos\DecompressionL20190920A\packages\Microsoft.ML.Probabilistic.0.3.1912.403\lib\netstandard2.0\Microsoft.ML.Probabilistic.dll"
#r @"C:\Users\glddm\source\repos\DecompressionL20190920A\packages\FSharp.Collections.ParallelSeq.1.1.3\lib\net45\FSharp.Collections.ParallelSeq.dll"

#load "Learner.fs"
#load "ReinforcementLearning.fs"
#load "Gas.fs"
#load "PredefinedDescent.fs"
#load "LEModel.fs"
#load "OptimalAscentLearning.fs"
#load "IOUtilities.fs"
#load "InputDefinition.fs"
#load "EnvironmentToPython.fs"
#load "AsyncHelpers.fs"
#load "SeqExtension.fs"
#load "AscentSimulator.fs"

//open ReinforcementLearning
open InitDescent
open LEModel
open ToPython
open AscentSimulator

type InitialDepthParams = { MaxDepth                 : float 
                            BottomTime               : float
                            TargetDepth              : float  }  // node right after bottom is reached   

//MissionConstraints.ascentRateLimit

type TwoLegAscentParams = { ConstantDepth              : float
                            TimeStepsAtConstantDepth   : int  } // minimum value is 1 

type ImmersionNode = { CurrentState  : LEState 
                       AccruedRisk   : float  
                       DescentBound  : float }

type ImmersionAnalytics  = { LastNodeAtBottom                   : ImmersionNode
                             TargetNode                         : ImmersionNode
                             InitFinalAscentNode                : ImmersionNode
                             AtSurfaceNode                      : ImmersionNode
                             FinalNode                          : ImmersionNode }
                          
let getAscentToSurfaceSeq initialDepth ascentRateLimit = 
    [ initialDepth .. ascentRateLimit .. 0.0 ] @ [0.0]
    |> List.toSeq
    |> Seq.skip 1  // was already there 
               
let twolegParamsToAscentStrategy (twoLegParams: TwoLegAscentParams) (maxAscentRate:float) = // ascentRate should be NEGATIVE 
    let constantDepthSeq = Seq.init twoLegParams.TimeStepsAtConstantDepth (fun _ -> twoLegParams.ConstantDepth) 
    let ascentToSurface  = getAscentToSurfaceSeq twoLegParams.ConstantDepth  ( max maxAscentRate   MissionConstraints.ascentRateLimit) 
    let atDepthSequence  = Seq.initInfinite  ( fun _ -> 0.0) 
    seq{ yield! constantDepthSeq
         yield! ascentToSurface 
         yield! atDepthSequence} , constantDepthSeq |> Seq.length , ascentToSurface |> Seq.length

let strategyOutput2ImmersionNode strategyNode  = 
    let (State state ) , _ , _ , descentBound = strategyNode
    { CurrentState = state.LEPhysics 
      AccruedRisk  = state.Risk.AccruedRisk
      DescentBound = descentBound}

let resetImmersionNodeRisk (immersionNode: ImmersionNode )  = 
    {immersionNode with AccruedRisk = 0.0}

let getInitAndTargetNodeFromDescent ( Output strategyToTargetOut : StrategyOutput) stepsFromMaxToTarget   =
    let lastNodeInfoAtMaxDepth = strategyToTargetOut.[0]
    let targetOut = strategyToTargetOut |> Array.last
    let lastNodeAtBottom = lastNodeInfoAtMaxDepth 
                           |> strategyOutput2ImmersionNode 
                             
    let targetNode         = targetOut 
                             |> strategyOutput2ImmersionNode
                             |> resetImmersionNodeRisk
    [|lastNodeAtBottom ; targetNode |]
     
let solveThis2LegAscent stepsFromMaxToTarget initialDepthParams twoLegAscentParams (maxAscentRate : option<float> ) =  //: ImmersionAnalytics = 
    let toTargetHistory , _ = getInitCondAfterDescentWithDefaultTimes stepsFromMaxToTarget  initialDepthParams.MaxDepth  initialDepthParams.BottomTime initialDepthParams.TargetDepth
    let initNodeAndTargetNode  = getInitAndTargetNodeFromDescent toTargetHistory stepsFromMaxToTarget 

    let getMaxAscentRate = function 
        | Some ascentRate -> ascentRate 
        | None            -> MissionConstraints.ascentRateLimit

    twolegParamsToAscentStrategy  twoLegAscentParams (getMaxAscentRate maxAscentRate)



let stepsFromMaxToTarget , initialDepthParams , twoLegAscentParams , maxAscentRate  = 4  ,  {MaxDepth = 120.0 ; BottomTime = 50.0 ; TargetDepth = 50.0 } , { ConstantDepth  = 60.0   ;  TimeStepsAtConstantDepth  = 5    } , None

let ascent  = solveThis2LegAscent stepsFromMaxToTarget initialDepthParams twoLegAscentParams maxAscentRate 

//constantDepthSeq |> Seq.length , ascentToSurface |> Seq.length
let ascStrategySeq, lenConstantDepth  , lenAscentSurf = ascent
//let (Output out ) = toTargHist

ascStrategySeq |> SeqExtension.takeWhileWithLast (fun x -> abs(x) > 1.e-2)  |> Seq.toArray


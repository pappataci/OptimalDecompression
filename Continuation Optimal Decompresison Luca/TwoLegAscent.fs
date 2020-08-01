[<AutoOpen>]
module TwoLegAscent

open InitDescent
open LEModel
open AscentSimulator

type InitialDepthParams = { MaxDepth                 : float 
                            BottomTime               : float
                            TargetDepth              : float  }  // node right after bottom is reached   

type TwoLegAscentParams = { ConstantDepth              : float
                            TimeStepsAtConstantDepth   : int  } // minimum value is 1 

type ImmersionNode = { CurrentState  : LEState 
                       AccruedRisk   : float  
                       DescentBound  : float }

type ImmersionAnalytics  = { LastNodeAtBottom                   : ImmersionNode
                             TargetNode                         : ImmersionNode
                             NodeAtConstantDepth                : ImmersionNode
                             InitFinalAscentNode                : ImmersionNode
                             AtSurfaceNode                      : ImmersionNode
                             FinalNode                          : ImmersionNode }

type ResultData  = { TargetDepth:float 
                     P0: float 
                     P1:float 
                     P2:float 
                     TimeStepsAtConstDepth:float 
                     ConstantDepthLev:float
                     AscentRate: float
                     OnSurfaceTime:float
                     FinalRisk:float
                     FinalTime:float}

 
let immersionNode2StateResetTime immersionNode  =  { LEPhysics =  resetTimeOfLEState immersionNode.CurrentState ; 
                                                      Risk  = { AccruedRisk = 0.0 ; IntegratedRisks =  0.0 
                                                                                    |> Array.create  ( immersionNode.CurrentState.TissueTensions |> Array.length )    } } 
                                                    |> State
          
let getAscentToSurfaceSeq initialDepth ascentRateLimit = 
     [ initialDepth .. ascentRateLimit .. 0.0 ] @ [0.0]
     |> List.toSeq
     |> Seq.skip 1  // was already there 
                
let twolegParamsToAscentStrategy (maxAscentRate:float)  (twoLegParams: TwoLegAscentParams) = // ascentRate should be NEGATIVE 
     let constantDepthSeq = Seq.init twoLegParams.TimeStepsAtConstantDepth (fun _ -> twoLegParams.ConstantDepth) 
     let ascentToSurface  = getAscentToSurfaceSeq twoLegParams.ConstantDepth  ( max maxAscentRate   MissionConstraints.ascentRateLimit) 
     let atDepthSequence  = Seq.initInfinite  ( fun _ -> 0.0) 
     seq{ yield! constantDepthSeq
          yield! ascentToSurface 
          yield! atDepthSequence} , [|constantDepthSeq ; ascentToSurface |]
                                    |> Array.map ( fun x -> (x |> Seq.length )  - 1 ) 
                                    |> Array.sum
 
let strategyOutput2ImmersionNodeNState strategyNode  = 
    let (State state ) , _ , _ , descentBound = strategyNode    
    { CurrentState = state.LEPhysics
      AccruedRisk  = state.Risk.AccruedRisk
      DescentBound = descentBound}
 
let resetImmersionNodeRiskAndTime (immersionNode: ImmersionNode )  = 
    {immersionNode with AccruedRisk = 0.0; CurrentState  = resetTimeOfLEState immersionNode.CurrentState}
 
let getInitAndTargetNodeFromDescent ( Output strategyToTargetOut : StrategyOutput) stepsFromMaxToTarget   =
    let lastNodeInfoAtMaxDepth = strategyToTargetOut.[0]
    let targetOut = strategyToTargetOut |> Array.last
    let lastNodeAtBottom = lastNodeInfoAtMaxDepth 
                           |> strategyOutput2ImmersionNodeNState 
 
    let targetNode       = targetOut 
                            |> strategyOutput2ImmersionNodeNState
                               
    [|lastNodeAtBottom ; targetNode |> resetImmersionNodeRiskAndTime |]  
      
let strategyOutput2ImmersionNodes ( Output strategyOutput )  lagToSurface timeStepAtConstantDepth = 
    [| 0 ;  1 ;    timeStepAtConstantDepth ; lagToSurface + 1 ; ( strategyOutput |> Array.length ) - 1 |] 
    |> Array.map (fun x -> strategyOutput.[x]  
                           |> strategyOutput2ImmersionNodeNState  )
    |> ( fun x -> x.[0], x.[1] , x.[2], x.[3] , x.[4] )

let immersionAnalyticsToResult immersionAnalytics twoLegAscentParams actualAscentRate = 
    let targetNodeDepth       =  leState2Depth immersionAnalytics.TargetNode.CurrentState
    let targetNodeTensions    =  leState2TensionValues immersionAnalytics.TargetNode.CurrentState
    let constantDepthDuration =  twoLegAscentParams.TimeStepsAtConstantDepth |> float      
    let constantDepthValue    =  leState2Depth immersionAnalytics.NodeAtConstantDepth.CurrentState  
    let onSurfaceTime         =  leState2Time immersionAnalytics.AtSurfaceNode.CurrentState
    let finalNodeInfos        = [immersionAnalytics.FinalNode.AccruedRisk ; leState2Time immersionAnalytics.FinalNode.CurrentState ] |> List.toArray 
    { TargetDepth = targetNodeDepth
      P0 = targetNodeTensions.[0]
      P1 = targetNodeTensions.[1]
      P2 = targetNodeTensions.[2]
      TimeStepsAtConstDepth = constantDepthDuration
      ConstantDepthLev = constantDepthValue
      AscentRate = actualAscentRate
      OnSurfaceTime = onSurfaceTime
      FinalRisk = finalNodeInfos.[0]
      FinalTime = finalNodeInfos.[1]  }
 
let solveThis2LegAscent stepsFromMaxToTarget initialDepthParams twoLegAscentParams (maxAscentRate : option<float> ) =   
    let toTargetHistory , _ = getInitCondAfterDescentWithDefaultTimes stepsFromMaxToTarget  initialDepthParams.MaxDepth  initialDepthParams.BottomTime initialDepthParams.TargetDepth
    let initNodeAndTargetNode   = getInitAndTargetNodeFromDescent toTargetHistory stepsFromMaxToTarget 
 
    let targetState =  initNodeAndTargetNode 
                        |> Array.last 
                        |> immersionNode2StateResetTime
 
    let getMaxAscentRate = function 
        | Some ascentRate -> ascentRate 
        | None            -> MissionConstraints.ascentRateLimit
    
    let actualAscentRate = getMaxAscentRate maxAscentRate

    let ascentStrategyToSurface , lagToSurface  = twoLegAscentParams
                                                   |>  twolegParamsToAscentStrategy  actualAscentRate
 
    let simulationOutput , _ =  simulateStrategyWithDefaultParamsAndThisInitNode targetState ascentStrategyToSurface
 
    let targetNode, nodeAtConstantDepth, initFinalAscentNode, atSurfaceNode , finalNode = strategyOutput2ImmersionNodes simulationOutput lagToSurface twoLegAscentParams.TimeStepsAtConstantDepth
 
    let immersionAnalytics = { LastNodeAtBottom         = initNodeAndTargetNode.[0] 
                               TargetNode               = targetNode
                               NodeAtConstantDepth      = nodeAtConstantDepth   
                               InitFinalAscentNode      = initFinalAscentNode   
                               AtSurfaceNode            = atSurfaceNode 
                               FinalNode                = finalNode                  }

    let resultVector = immersionAnalyticsToResult immersionAnalytics twoLegAscentParams actualAscentRate
 
    resultVector , immersionAnalytics   , simulationOutput   

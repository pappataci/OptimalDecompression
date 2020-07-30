module AscentSimulator 
open ToPython
open LEModel.LEModel
type SimulationParameters = { MaxPDCS : float 
                              MaxSimTime : float
                              PenaltyForExceedingRisk : float 
                              RewardForDelivering : float 
                              PenaltyForExceedingTime : float 
                              IntegrationTime : float
                              ControlToIntegrationTimeRatio : int 
                              DescentRate : float 
                              MaximumDepth : float 
                              BottomTime : float 
                              LegDiscreteTime : float }

type AscentStrategy = |Ascent of seq<float> 

type StrategyInput = StrategyInput of  SimulationParameters*AscentStrategy

let simulateAscent  env ascentLimiter initState (sequenceOfDepths:seq<float>)  =   
    sequenceOfDepths 
    |> Seq.scan ( fun ( nextState, rew, isTerminal, _ )  depth -> getNextEnvResponseAndBoundForNextAction(env, nextState , depth , ascentLimiter)  ) (  initState, 0.0 , false, 0.0)  
    |> SeqExtension.takeWhileWithLast (fun (_ , _, isTerminalState, _) ->  not isTerminalState)
    |> Seq.toArray

type StrategyOutput = | Output of (State<LEStatus> * float *bool *float) [] 

let simulateStrategyWithInput (optInitState : Option<State<LEStatus>>) (  StrategyInput (  {MaxPDCS = maxPDCS ; MaxSimTime = maximumSimulationTime ; PenaltyForExceedingRisk = penaltyForExceedingRisk ; 
                                RewardForDelivering = rewardForDelivering ; PenaltyForExceedingTime = penaltyForExceedingTime ; IntegrationTime = integrationTime ;
                                ControlToIntegrationTimeRatio = controlToIntegrationTimeRatio; DescentRate = descentRate; MaximumDepth = maximumDepth ;
                                BottomTime = bottomTime ; LegDiscreteTime = legDiscreteTime }   ,  Ascent  ascentStrategy )     )  = 
    
    let env, initState ,  ascentLimiter , _  =  getEnvInitStateAndAscentLimiter  ( maxPDCS    , maximumSimulationTime , 
                                                                           penaltyForExceedingRisk ,  rewardForDelivering , penaltyForExceedingTime , 
                                                                           integrationTime  ,
                                                                           controlToIntegrationTimeRatio,  
                                                                           descentRate , 
                                                                           maximumDepth  , 
                                                                           bottomTime  , 
                                                                           legDiscreteTime   ) 
    let actualInitState = match optInitState with
                          | Some optInitStateValue -> optInitStateValue
                          | None -> initState 
    (ascentStrategy
    |> simulateAscent  env ascentLimiter actualInitState
    |> Output , env )

let simulateStrategy  = 
    simulateStrategyWithInput None

let getConstantRateAscent stepsFromMaxToTarget initDepth  targetDepth = 
    let step = (targetDepth - initDepth ) / (stepsFromMaxToTarget |> float )
    [|initDepth .. step .. targetDepth|] 
    |> Array.skip 1 
    |> Seq.ofArray

let getInitConditionAfterDescentPhase (integrationTime, controlToIntegration, legDiscreteTime ) stepsFromMaxToTarget  maxDepth  bottomTime targetDepth  = 
    let maxPDCS   , maxSimTime, penaltyForRisk, rewardForDelivering, penaltyForTime ,   descentRate = 
        infinity  , infinity  , 10.0          , 10.0               , 5.0            ,     60.0

    let ascentStrategy = 
        targetDepth 
        |> getConstantRateAscent stepsFromMaxToTarget maxDepth 
        |> Ascent  
        
    let simParams =    {  MaxPDCS = maxPDCS          
                          MaxSimTime = maxSimTime 
                          PenaltyForExceedingRisk = penaltyForRisk
                          RewardForDelivering = rewardForDelivering
                          PenaltyForExceedingTime = penaltyForTime
                          IntegrationTime = integrationTime
                          ControlToIntegrationTimeRatio = controlToIntegration
                          DescentRate = descentRate 
                          MaximumDepth = maxDepth 
                          BottomTime = bottomTime 
                          LegDiscreteTime = legDiscreteTime }
     
    (simParams , ascentStrategy ) |> StrategyInput 
                                  |> simulateStrategy

let getInitCondAfterDescentWithDefaultTimes =
    let integrationTime , controlToIntegrationTime , legDiscreteTime = 0.1 , 10, 0.1
    getInitConditionAfterDescentPhase (integrationTime , controlToIntegrationTime , legDiscreteTime ) 

let getInitCondAfterDescentWithDefaultTimesResettingTimeNRisk stepsFromMaxToTarget  maxDepth  bottomTime targetDepth =
    let (Output sequenceOfResponse) , env = getInitCondAfterDescentWithDefaultTimes stepsFromMaxToTarget  maxDepth  bottomTime targetDepth
    let finalState , ascentLimit  =            sequenceOfResponse
                                                |> Array.last 
                                                |> (fun (state, _, _ , limitAscent) -> (state , limitAscent) ) 

    ( finalState 
     |> resetTissueRiskAndTime , ascentLimit , env )
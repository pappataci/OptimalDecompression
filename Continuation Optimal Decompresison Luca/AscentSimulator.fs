module AscentSimulator 
open ToPython

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

type StrategyOutput = | Output of (State<LEModel.LEModel.LEStatus> * float *bool *float) [] 


let simulateStrategy (  StrategyInput (  {MaxPDCS = maxPDCS ; MaxSimTime = maximumSimulationTime ; PenaltyForExceedingRisk = penaltyForExceedingRisk ; 
                       RewardForDelivering = rewardForDelivering ; PenaltyForExceedingTime = penaltyForExceedingTime ; IntegrationTime = integrationTime ;
                       ControlToIntegrationTimeRatio = controlToIntegrationTimeRatio; DescentRate = descentRate; MaximumDepth = maximumDepth ;
                       BottomTime = bottomTime ; LegDiscreteTime = legDiscreteTime }   ,  Ascent  ascentStrategy )   )  = 
    
    let env, initState ,  ascentLimiter , _  =  getEnvInitStateAndAscentLimiter  ( maxPDCS    , maximumSimulationTime , 
                                                                           penaltyForExceedingRisk ,  rewardForDelivering , penaltyForExceedingTime , 
                                                                           integrationTime  ,
                                                                           controlToIntegrationTimeRatio,  
                                                                           descentRate , 
                                                                           maximumDepth  , 
                                                                           bottomTime  , 
                                                                           legDiscreteTime   ) 
    ascentStrategy
    |> simulateAscent  env ascentLimiter initState
    |> Output

let defaultSimulationParameters =  ( {MaxPDCS = 0.32 ; MaxSimTime = 5000.0 ; PenaltyForExceedingRisk  = 1.0 ; RewardForDelivering = 10.0; PenaltyForExceedingTime = 0.5 ;
                                      IntegrationTime = 0.1; ControlToIntegrationTimeRatio = 10; DescentRate = 60.0; MaximumDepth = 20.0 ; BottomTime = 10.0;  LegDiscreteTime = 0.1} , seq{0.0} |> Ascent)  
                                    |> StrategyInput
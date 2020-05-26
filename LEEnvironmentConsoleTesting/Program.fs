// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.
open System
open LEModel
open OptimalAscentLearning
open InputDefinition

let pressAnyKey() = Console.Read() |> ignore

//[<AutoOpen>]
//module ModelParams = 
//    let crossover               = [|     9.9999999999E+09   ;     2.9589519286E-02    ;      9.9999999999E+09    |]
//    let rates                   = [| 1.0 / 1.7727676636E+00 ; 1.0 / 6.0111598753E+01  ;  1.0 / 5.1128788835E+02  |]
//    let thalmanErrorHypothesis  = true                           
//    let gains                   = [| 3.0918150923E-03 ; 1.1503684782E-04 ; 1.0805385353E-03 |]
//    let threshold               = [| 0.0000000000E+00 ; 0.0000000000E+00 ; 6.7068236527E-02 |]
//    let fractionO2  = 0.21

//    let integrationTime = 0.1 // minute

[<EntryPoint>]
let main _ = 
        
    let maximumRiskBound = pDCSToRisk 3.0e-2

    let modelBuilderParams = { TimeParams = { IntegrationTime                  = 0.1  // minute  
                                              ControlToIntegrationTimeRatio    = 10  
                                              MaximumFinalTime                 = 5000.0 }  // minute 
                               LEParamsGeneratorFcn = USN93_EXP.fromConstants2ModelParamsWithThisDeltaT crossover rates threshold gains thalmanErrorHypothesis 
                               StateTransitionGeneratorFcn = modelTransitionFunction 
                               ModelIntegration2ModelActionConverter = targetNodesPartitionFcnDefinition 
                               RewardParameters                      = { MaximumRiskBound  = maximumRiskBound
                                                                         PenaltyForExceedingRisk =  5000.0 }  }
    
    let modelsDefinition = defEnvironmentModels |> ModelDefiner
    let shortTermRewardEstimator = defineShortTermRewardEstimator shortTermRewardOnTimeDifference  penaltyIfMaximumRiskIsExceeded
    let terminalStatePredicate = StatePredicate defineFinalStatePredicate
    let infoLogger = nullLogger 

    let missionParameters = { DescentRate       = 60.0     // ft/min
                              MaximumDepth      = 120.0    // ft 
                              BottomTime        = 30.0     // min
                              LegDiscreteTime   = 0.1      // min 
                              InitialDepth      = 0.0 }    // ft

                            |> System2InitStateParams

    let initialStateCreator =   defInitStateCreatorFcn getInitialStateWithTheseParams 
    let (Environment environment ,  initState ,  Model  integrationModel', _ ) = 
        initializeEnvironment  (modelsDefinition , modelBuilderParams |> Parameters ) 
            shortTermRewardEstimator 
            terminalStatePredicate 
            infoLogger 
            (initialStateCreator , missionParameters ) (ExtraFunctions None)
    
    let seqOfDepths' = [|90.0 .. -30.0 .. 0.0|] 

    let seqOfZeros = Seq.init 750 ( fun _ -> 0.0)

    let seqOfDepths = seq {yield! seqOfDepths' 
                           yield! seqOfZeros}

    let states = seqOfDepths
                 |> Seq.scan (fun actualState depth ->  (environment actualState (Control depth))
                                                        |> (fun x -> x.EnvironmentFeedback.NextState ) )  initState 
                 |> Seq.toArray 
    
    let equivalentDepthStatete state  =  state 
                                        |> leStatus2TissueTension
                                        |> Array.max
                                        |> n2Pressure2Depth thalmanErrorHypothesis dFO2Air
    Console.WriteLine (initState)
    Console.WriteLine("COMPUTED DEPTHS")
    states |> 
    Array.iter ( equivalentDepthStatete >> Console.WriteLine)

    Console.WriteLine("STATES")
    states
    |> Array.iter ( fun s -> Console.WriteLine(s))

    Console.WriteLine("Last State")
    Console.WriteLine(states |> Array.last)
    pressAnyKey()
    0 // return an integer exit code

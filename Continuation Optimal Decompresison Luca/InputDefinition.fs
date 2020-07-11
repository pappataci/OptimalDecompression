[<AutoOpen>]
module InputDefinition.ForPython

open OptimalAscentLearning

let pDCSToRisk pDCS = 
    -log(1.0-pDCS)

[<AutoOpen>]
module ModelParams = 
    let crossover               = [|     9.9999999999E+09   ;     2.9589519286E-02    ;      9.9999999999E+09    |]
    let rates                   = [| 1.0 / 1.7727676636E+00 ; 1.0 / 6.0111598753E+01  ;  1.0 / 5.1128788835E+02  |]
    let thalmanErrorHypothesis  = true                           
    let gains                   = [| 3.0918150923E-03 ; 1.1503684782E-04 ; 1.0805385353E-03 |]
    let threshold               = [| 0.0000000000E+00 ; 0.0000000000E+00 ; 6.7068236527E-02 |]
    let fractionO2  = 0.21
    let maximumRiskBound = pDCSToRisk 3.3e-2
    let penaltyForExceedingRisk = 5000.0
    let maxFinalTime = penaltyForExceedingRisk

[<AutoOpen>]
module EnvironmentSetup = 
    let modelsDefinition = defEnvironmentModels |> ModelDefiner
    
    // default is shortTermRewardOnTimeDifference
    let shortTermRewardEstimator = defineShortTermRewardEstimator shortTermRewardIsAlwaysZero  finalRewardComputation
    let terminalStatePredicate = StatePredicate defineFinalStatePredicate
    let infoLogger = nullLogger 

    let initialStateCreator =   defInitStateCreatorFcn getInitialStateWithTheseParams 
    let ascentLimiterFcn =  (params2InternalComputationFcn  , computeMaxPositiveRateFcn )
                            |> Some
                            |> optionalExtraFunctionCreator
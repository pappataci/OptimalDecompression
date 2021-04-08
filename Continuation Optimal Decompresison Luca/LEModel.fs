namespace LEModel

[<AutoOpen>]
module LEModel  = 

    open InitDescent
    
    type ExternalPressureConditions = { Ambient   : float
                                        Nitrogen  : float }

    type ExternalConditions         = { Pressures : ExternalPressureConditions 
                                        Depth     : float                        }

    type Tissue                     = |Tension   of  float 
    
    let inline (+>) (Tension x:Tissue ) (Tension y:Tissue)  = 
        (x + y) 
        |> Tension 

    type LEState    = { TissueTensions                : Tissue[] 
                        CurrentDepthAndTime           : DepthInTime     }

    type RiskInfo         = { AccruedRisk       : float 
                              IntegratedRisks   : float[] } 

    type SingleLEModelParam   = { Crossover : float 
                                  Rate      : float 
                                  Threshold : float
                                  Gain      : float }

    type LEModelParams        = { LEParams               : SingleLEModelParam[]
                                  ThalmanErrorHypothesis : bool  
                                  IntegrationTime        : float   
                                  FractionO2             : float }
    
    type LEStatus       = { LEPhysics       : LEState 
                            Risk            : RiskInfo      }
  
    let private linearKineticsIncrement modelConsts deltaT (Tension _ )   pressures  =    
        deltaT * modelConsts.Rate  * ( pressures.Nitrogen -  pressures.Ambient - modelConsts.Crossover + dPFVG) 
    
    let private exponentialKineticsIncrement modelConsts deltaT (Tension tissueTension) pressures =
        let tissueIncForcingTerm = modelConsts.Rate * deltaT
        ( tissueIncForcingTerm * ( pressures.Nitrogen - tissueTension) ) / ( 1.0 + tissueIncForcingTerm)

    let private chooseAppropriateModelDependingOnTissueTensionNForcingTerm (Tension tissueTension ) modelConsts 
        ({Ambient = ambientPressure}:ExternalPressureConditions)  = 
            if (tissueTension > ambientPressure + modelConsts.Crossover - dPFVG) then linearKineticsIncrement
            else exponentialKineticsIncrement
    
    let private getLETissueTensionIncrement modelConsts deltaT pressures ( actualTissueTension:Tissue )  =
        let integrationFcn = chooseAppropriateModelDependingOnTissueTensionNForcingTerm actualTissueTension modelConsts pressures
        integrationFcn modelConsts deltaT actualTissueTension pressures
        |> Tension

    let private getInstantaneousRisk modelConsts (Tension updatedTissueTension) pressures = 
        modelConsts.Gain * ( updatedTissueTension - pressures.Ambient - ( modelConsts.Threshold - dPFVG ) ) / pressures.Ambient

    let private getIntegratedRiskForThisDeltaT deltaT pressures modelConsts updatedTissueTension  = 
        pressures
        |> getInstantaneousRisk modelConsts updatedTissueTension
        |> Operators.max 0.0
        |> (*) deltaT

    let private updateTissueTension  deltaT pressures  modelConsts actualTension =
        (getLETissueTensionIncrement modelConsts deltaT pressures actualTension) 
        |> (+>) actualTension

    let depth2AmbientCondition modelConstants depth =
        
        let ambientPressure = depth2AmbientPressure depth
        let nitrogenPressure = ambientPressure 
                               |> externalN2Pressure modelConstants.ThalmanErrorHypothesis modelConstants.FractionO2 
        {Pressures = {Ambient = ambientPressure 
                      Nitrogen = nitrogenPressure} 
         Depth = depth }
    
    let updateDepthAndTime deltaTime nextDepth  actualDepthInTime     =
        let (TemporalValue actualDepthAndTimeValue) = actualDepthInTime
        {Time = actualDepthAndTimeValue.Time + deltaTime ; Value = nextDepth}
        |> TemporalValue

    // We assume the model is always expressed in terms of depth. So that next action is expressing the final depth.
    // For example suppose actual depth is 300 ft and we want to go to 100 ft with multiple 10; then target will be 100
    // and this function will create subtargets of 30 ft (300/10) since every substep is a tenth of the original target.
    // The function also takes care of providing non negative targets (we cannot go above the sea level).
    
    let defineModelOnSlowerDecisionTime (subDividedNextActionIntoSubActions: State<'S> -> Action<'A> -> seq<Action<'A>>) 
        (Model initialModel:Model<'S,'A>) = 
        
        let nextStepFunction (initState: State<'S>) ( elementaryAction: Action<'A>) = 

            subDividedNextActionIntoSubActions initState elementaryAction 
            |> Seq.fold initialModel initState

        nextStepFunction |> Model

    let leStatus2Depth (State actualLEStatus: State<LEStatus>) = 
        let (TemporalValue actualDepthInTime) = actualLEStatus.LEPhysics.CurrentDepthAndTime
        actualDepthInTime.Value

    let getNextDepth  (integrationTime:float) (  actualLEStatus:State<LEStatus>) (depthRate:float) = 
        let actualDepth = actualLEStatus |>  leStatus2Depth
        actualDepth + depthRate * integrationTime

    let modelTransitionFunction (modelConstants:LEModelParams) (actualLEStatus:LEStatus) (nextDepth:float)  =

        let nextStepAmbientConditions = depth2AmbientCondition modelConstants nextDepth
        
        let updatedTissueTensions = 
            actualLEStatus.LEPhysics.TissueTensions 
            |> Array.map2 (updateTissueTension modelConstants.IntegrationTime nextStepAmbientConditions.Pressures) modelConstants.LEParams  

        let getIntegratedRiskForThisTissue = getIntegratedRiskForThisDeltaT modelConstants.IntegrationTime nextStepAmbientConditions.Pressures
        
        let integratedRisks = updatedTissueTensions 
                               |> Array.map2 getIntegratedRiskForThisTissue modelConstants.LEParams
                               
        let updateAccruedRisk = 
            integratedRisks
            |> Seq.sum
            |> (+) actualLEStatus.Risk.AccruedRisk

        {LEPhysics = { TissueTensions        = updatedTissueTensions 
                       CurrentDepthAndTime   = actualLEStatus.LEPhysics.CurrentDepthAndTime
                                               |> updateDepthAndTime modelConstants.IntegrationTime nextDepth}  
         Risk     = { AccruedRisk     = updateAccruedRisk
                      IntegratedRisks =  integratedRisks  }  }
       
    let leStatus2TissueTension (State {LEPhysics = leState} )  = 
        leState.TissueTensions
        |> Array.map (fun (Tension x ) -> x)

    let leStatus2ModelTime ( State { LEPhysics = leState })  = 
        let (TemporalValue temporalValueInfo) =  leState.CurrentDepthAndTime
        temporalValueInfo.Time

    let leStatus2Risk ( State { Risk = leRiskInfo}) = 
        leRiskInfo.AccruedRisk

    let IsAtSurfaceLevel depth =
        Operators.abs(depth) <  MissionConstraints.depthTolerance

    let areAllTissueTensionsAtMostEqualToAmbientN2Press  actualAmbientPressure  (leParamsThresholds: float[] ) (actualTissueTensions:float[])  = 
        actualTissueTensions 
        |> Array.map2 (fun tissueTensionThreshold  tissueTension  ->  tissueTension < actualAmbientPressure +  tissueTensionThreshold - dPFVG  )   leParamsThresholds
        |> Array.reduce (&&)

    let leStatus2IsEmergedAndNotAccruingRisk ( actualState: State<LEStatus> )    (leParamsThresholds: float[] ) = 
        let actualDepth = actualState |>  leStatus2Depth
        let actualAmbientPressure = actualDepth |> depth2AmbientPressure
        let tissueTensionsAreNotRiskSource = actualState 
                                             |> leStatus2TissueTension 
                                             |> areAllTissueTensionsAtMostEqualToAmbientN2Press   actualAmbientPressure leParamsThresholds
        
        let weAreAtSurfaceLevel = actualDepth  |> IsAtSurfaceLevel // ft

        tissueTensionsAreNotRiskSource && weAreAtSurfaceLevel
    
    let getTissueDepth thalmanHyp fractionO2=
        leStatus2TissueTension
        >> Array.max
        >> n2Pressure2Depth thalmanHyp fractionO2

    let resetTissueRiskAndTime ((State  leStatus ) : State<LEStatus>) = 
        let numberOfTissues = leStatus.Risk.IntegratedRisks |> Array.length
        {leStatus with Risk = {AccruedRisk = 0.0 ; IntegratedRisks = Array.zeroCreate  numberOfTissues } ; 
                       LEPhysics = {leStatus.LEPhysics with CurrentDepthAndTime  = resetTimeOfDepthInTime leStatus.LEPhysics.CurrentDepthAndTime } } 
        |> State

    let resetTimeOfCurrentDepthAndTime (TemporalValue currentDepthAndTime   ) = 
        {currentDepthAndTime with Time = 0.0}
        |> TemporalValue

    let resetTimeOfLEState leState  = 
        {leState with  CurrentDepthAndTime = resetTimeOfCurrentDepthAndTime leState.CurrentDepthAndTime }

    let leState2Time (leState:LEState) = 
        leState.CurrentDepthAndTime
        |> getTime 

    let leState2TensionValues( { TissueTensions = tissueTensions} ) = 
        tissueTensions
        |> Array.map (fun (Tension t ) -> t ) 

    let leState2Depth (leState:LEState) = 
        leState.CurrentDepthAndTime
        |> getValue

    let createFictitiouStateFromDepthTime (initTime, initDepth) = 
        let tensions = [|Tension 1.0; Tension 1.0; Tension 1.0|]
        let temporalValue = TemporalValue {Time = initTime ; Value = initDepth}
        let leState = {TissueTensions = tensions  
                       CurrentDepthAndTime = temporalValue } 

        let fictitiousRisk = {   AccruedRisk       =          0.0 
                                 IntegratedRisks   =  [|0.0;0.0;0.0|] }

        {LEPhysics = leState ; Risk = fictitiousRisk}
        |> State
        
module USN93_EXP = 
    open InitDescent

    let crossover               = [|     9.9999999999E+09   ;     2.9589519286E-02    ;      9.9999999999E+09    |]
    let rates                   = [| 1.0 / 1.7727676636E+00 ; 1.0 / 6.0111598753E+01  ;  1.0 / 5.1128788835E+02  |]
    let thalmanErrorHypothesis  = true                           
    let gains                   = [| 3.0918150923E-03 ; 1.1503684782E-04 ; 1.0805385353E-03 |]
    let threshold               = [| 0.0000000000E+00 ; 0.0000000000E+00 ; 6.7068236527E-02 |]
    let fractionO2  = 0.21
    
    let fromConstants2ModelParamsWithThisDeltaT (crossover:float[]) (rates:float[]) 
                                                (threshold:float[]) (gains:float[]) thalmanErrorHypothesis deltaT  = 
        let actualLEParams = [| 0 .. (gains |> Array.length) - 1 |]
                             |> Array.mapi (fun i _ ->  {Crossover = crossover.[i] 
                                                         Rate      = rates.[i]
                                                         Threshold = threshold.[i] 
                                                         Gain      = gains.[i] } ) 

        {LEParams                   = actualLEParams 
         ThalmanErrorHypothesis     = thalmanErrorHypothesis
         IntegrationTime            = deltaT 
         FractionO2                 = fractionO2}

    let getLEOptimalModelParamsSettingDeltaT = fromConstants2ModelParamsWithThisDeltaT crossover rates threshold gains thalmanErrorHypothesis

    let initStateFromInitDepth modelConstants initDepth = 
        let ambientCondition = depth2AmbientCondition modelConstants initDepth
        let tissueState          =  { TissueTensions       =  Array.create rates.Length ( Tension ambientCondition.Pressures.Nitrogen )
                                      CurrentDepthAndTime  =  TemporalValue { Time = 0.0 ; Value = initDepth }                 } 
        let initialRiskInfo      =  { AccruedRisk = 0.0 ; IntegratedRisks =   Array.create rates.Length 0.0   }
        {LEPhysics = tissueState ; Risk = initialRiskInfo }

    let setThalmanHypothesis thalmanHyp (modelParams:LEModelParams)  = 
        {modelParams with ThalmanErrorHypothesis = thalmanHyp} 

    
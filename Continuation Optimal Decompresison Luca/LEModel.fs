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
        {Ambient = ambientPressure}  = 
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
        |> max 0.0
        |> (*) deltaT

    let private updateTissueTension  deltaT pressures  modelConsts actualTension =
        (getLETissueTensionIncrement modelConsts deltaT pressures actualTension) 
        |> (+>) actualTension

    let depth2AmbientCondition modelConstants depth =
        
        let ambientPressure = depthAmbientPressure depth
        let nitrogenPressure = ambientPressure 
                               |> externalN2Pressure modelConstants.ThalmanErrorHypothesis modelConstants.FractionO2 
        {Pressures = {Ambient = ambientPressure 
                      Nitrogen = nitrogenPressure} 
         Depth = depth }
    
    let updateDepthAndTime deltaTime nextDepth  actualDepthInTime     =
        let (TemporalValue actualDepthAndTimeValue) = actualDepthInTime
        {Time = actualDepthAndTimeValue.Time + deltaTime ; Value = nextDepth}
        |> TemporalValue

    let getNextDepth  (integrationTime:float) (actualLEStatus:LEStatus) (depthRate:float) = 

        let (TemporalValue actualDepthInTime) = actualLEStatus.LEPhysics.CurrentDepthAndTime
        let actualDepth = actualDepthInTime.Value
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

    //let modelTransitionFunctionEveryXStep (Model originalModel: Model<'S,'A>) (xstep:int)=
        

module USN93_EXP = 
    open InitDescent
    let crossover               = [|     9.9999999999E+09   ;     2.9589519286E-02    ;      9.9999999999E+09    |]
    let rates                   = [| 1.0 / 1.7727676636E+00 ; 1.0 / 6.0111598753E+01  ;  1.0 / 5.1128788835E+02  |]
    let thalmanErrorHypothesis  = false                           
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
        let tissueState          =  { TissueTensions       =  Array.init rates.Length (fun _ -> 
                                                                            Tension (ambientCondition.Pressures.Nitrogen) )
                                      CurrentDepthAndTime  = TemporalValue { Time = 0.0 ; Value = initDepth }                 } 
        let initialRiskInfo      =  { AccruedRisk = 0.0 ; IntegratedRisks =   Array.create rates.Length 0.0   }
        {LEPhysics = tissueState ; Risk = initialRiskInfo }

    let setThalmanHypothesis thalmanHyp modelParams  = 
        {modelParams with ThalmanErrorHypothesis = thalmanHyp} 


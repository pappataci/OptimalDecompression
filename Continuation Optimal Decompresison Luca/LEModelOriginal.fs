namespace LEModel

[<AutoOpen>]
module GAS =
    
    [<AutoOpen>]
    module Constants = 
        let dFO2Air = 0.2100
        let dPACO2  = 0.0460
        let dPVO2   = 0.0605
        let dPVCO2  = 0.0696
        let dPH2O   = 0.0617
        let dPFVG   = 0.1917
        let dPFVG2  = 0.19210526315789
        let dPTMG   = 0.153947368421053

//     get pressure in atm (depth in ft)
    let depthAmbientPressure depth = 
        1.0 + depth / 33.066
    
    let externalN2Pressure (bThalmannError:bool) fractionO2 ambientPressure  = 
         //deduct dPACO2 if Thalmann Error is set to true
        (ambientPressure - dPH2O - dPACO2 * (System.Convert.ToDouble  bThalmannError) ) * (1.0 - fractionO2)

[<AutoOpen>]
module ModelTypes = 

    type ExternalPressureConditions = { Ambient   : float
                                        Nitrogen  : float }

    type Tissue                     = |Tension   of  float 

    type LETissueComputation        = { Tissue            : Tissue 
                                        Rate              : float
                                        CrossOverPressure : float } 

    type MultiDimensionalGeneralizedTissue<'T> = 'T[]

module USN93_EXP = 

    [<AutoOpen>]
    module ModelConstants = 
        let crossover = [|     9.9999999999E+09   ;     2.9589519286E-02    ;      9.9999999999E+09    |]
        let rates     = [| 1.0 / 1.7727676636E+00 ; 1.0 / 6.0111598753E+01  ;  1.0 / 5.1128788835E+02  |]
        let defaultThalmanErrorIsTrue = true

    let pressureNitrogenFromAmbientPressure = externalN2Pressure defaultThalmanErrorIsTrue
    let pressureNitrogenFromAmbientBreathingAir = pressureNitrogenFromAmbientPressure dFO2Air

     //for now we consider only uncoupled models
    let linearKineticsIncrement           deltaT {Rate = tissueRate ; CrossOverPressure = xOver}
                                          { Ambient = ambPress; Nitrogen = nitrogenPress}  =
        deltaT * tissueRate * (nitrogenPress - ambPress - xOver + dPFVG) 

    let exponentialKineticsIncrement     deltaT  {Tissue = Tension actualTension;  Rate = tissueRate}      
                                         { Nitrogen = nitrogenPress }  =
        let projectToNextTimeStep value =
            -(value * tissueRate * deltaT) /( 1.0 + tissueRate * deltaT)
        [|actualTension ; nitrogenPress |] 
        |> Array.map projectToNextTimeStep 
        |> Array.sum

    let chooseAppropriateModelDependingOnTissueTensionNForcingTerm 
        {Tissue = Tension tissueTension; CrossOverPressure = crossOver} 
        pressureAmbient  =
        match (tissueTension > pressureAmbient + crossOver - dPFVG) with
        | true  -> linearKineticsIncrement
        | false -> exponentialKineticsIncrement

    let getLETissueTensionIncrement deltaT externalPressures actualLETissueComputation = 
        let integrationFcn = chooseAppropriateModelDependingOnTissueTensionNForcingTerm actualLETissueComputation externalPressures.Ambient
        integrationFcn deltaT  actualLETissueComputation externalPressures

    let getNextLETissueTension deltaT externalPressures actualLETissueComputation  = 
        let tissueTensionIncrement = getLETissueTensionIncrement deltaT externalPressures actualLETissueComputation
        let (Tension actualTissueTension)  = actualLETissueComputation.Tissue
        actualTissueTension + tissueTensionIncrement
                                   

    [<AutoOpen>]
    module Risk =
        
        [<AutoOpen>]
        module RiskConstants = 
            let gain        =      [| 3.0918150923E-03 ; 1.1503684782E-04 ; 1.0805385353E-03 |]
            let threshold   =      [| 0.0000000000E+00 ; 0.0000000000E+00 ; 6.7068236527E-02 |]
            
        let getInstantaneousRisk (Tension tissueTension ) tissueGain  threshold  { Ambient = pressureAmbient} = 
            tissueGain * ( tissueTension - pressureAmbient - ( threshold - dPFVG) ) / pressureAmbient

        let getDeltaRisk deltaT instantaneousRisk =
            deltaT * ( max instantaneousRisk 0.0 ) 
    
        let risk2Probability risk =
            1.0 - exp(-risk)

    [<AutoOpen>]  // TO DO: to be fixed
    module MultiDimensionalTissues =
        
        type LEComputationVec = { TissueState :  MultiDimensionalGeneralizedTissue<LETissueComputation> }
        type TissueVec =        { Tensions    :  MultiDimensionalGeneralizedTissue<Tissue>              }

     

        //type LEState = { TissueState      : TissueVector 
        //                 ForcingTerms     : ExternalPressureConditions 
        //                 AccruedRisk      : float }

        //let state2Probability state = 
        //    risk2Probability state.AccruedRisk
            
        //let getNextState deltaT {TissueState = currentTissueState ; } 
            
        //    (nextAmbientConditions:ExternalPressureConditions )  : LEState =
            
        //    let incrementsToTissueTensions = 
        //        crossover
        //        |> Array.zip  currentTissueState                                                 // create LE tissues with XO
        //        |> Array.map  ( getLETissueTensionAtTPlusDeltaT deltaT  nextAmbientConditions )
       
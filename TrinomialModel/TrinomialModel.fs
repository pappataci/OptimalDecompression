[<AutoOpen>]
module TrinomialModel

    [<AutoOpen>]
    module Profile = 

        type DepthTimeInfo = { Depth : double
                               Time  : double }

        type ExternalPressureConditions = { Ambient   : float
                                            Nitrogen  : float }

        type Trajectory  = |Trajectory of seq<DepthTimeInfo>

    [<AutoOpen>]
    module ModelPhysics = 
        type Tissue                     = |Tension   of  float 

        let inline (+>) (Tension x:Tissue ) (Tension y:Tissue)  = 
            (x + y) 
            |> Tension 

    [<AutoOpen>]
    module Mission = 
        
        type Node =  {    EnvInfo : DepthTimeInfo
                          Tensions : Tissue[]
                          ExternalPressures : ExternalPressureConditions
                          InstantaneousRisk : double[]
                          AccruedRisk : double[]
                          TotalRisk : double} 

    [<AutoOpen>]
    module ModelDefinition = 

        let modelParams = {CrossOver  = [|     1000.0   ;    0.236795821    ;      1000.0   |] 
                           Rates      = [| 1.0 / 1.7727676636E+00 ; 1.0 / 6.0111598753E+01  ;  1.0 / 5.1128788835E+02  |] 
                           Gains      = [| 3.0918150923E-03 ; 1.1503684782E-04 ; 1.0805385353E-03 |]
                           Thresholds = [| 0.0000000000E+00 ; 0.0000000000E+00 ; 6.7068236527E-02 |]}
        
        let  trinomialScaleFactor  = 0.134096478 
        let maxIntegrationTime = 0.1 // min
        
        type Model<'S, 'A>   = | Model of ('S -> 'A -> 'S)

        let private linearKineticsIncrement iTissue deltaT (Tension _ )   pressures  =    
            deltaT * modelParams.Rates.[iTissue]  * ( pressures.Nitrogen -  pressures.Ambient - modelParams.CrossOver.[iTissue] + dPFVG) 
        
        let private exponentialKineticsIncrement iTissue deltaT (Tension tissueTension) pressures =
            let tissueIncForcingTerm = modelParams.Rates.[iTissue] * deltaT
            ( tissueIncForcingTerm * ( pressures.Nitrogen - tissueTension) ) / ( 1.0 + tissueIncForcingTerm)

        let private chooseAppropriateModelDependingOnTissueTensionNForcingTerm iTissue (Tension tissueTension )  
            ({Ambient = ambientPressure}:ExternalPressureConditions)  = 
                if (tissueTension > ambientPressure + modelParams.CrossOver.[iTissue]  - dPFVG) then linearKineticsIncrement
                else exponentialKineticsIncrement

        let private getLETissueTensionIncrement iTissue deltaT pressures ( actualTissueTension:Tissue )  =
            let integrationFcn = chooseAppropriateModelDependingOnTissueTensionNForcingTerm iTissue actualTissueTension pressures
            integrationFcn iTissue deltaT actualTissueTension pressures
            |> Tension

        let private updateTissueTension  deltaT pressures  iTissue actualTension =
            (getLETissueTensionIncrement iTissue deltaT pressures actualTension) 
            |> (+>) actualTension
       
        let depth2AmbientCondition depth =
                 
            let ambientPressure = depth2AmbientPressure depth
            let nitrogenPressure = ambientPressure 
                                   |> externalN2Pressure
            {Ambient = ambientPressure 
             Nitrogen = nitrogenPressure} 

        let inBetweenNodesTimeDiscretization {Depth = initDepth; Time = initTime} {Depth = targetDepth ; Time = finalTime} = 
            let timeLength = finalTime - initTime;
            let numberOfSteps =  ceil( timeLength / maxIntegrationTime )  
            let actualDeltaT = timeLength / numberOfSteps
            let depthIncrement = (targetDepth - initDepth ) /numberOfSteps
            Seq.init (int numberOfSteps) (fun actualCount -> 
                                                let time = initTime  + (actualCount + 1 |>double) * actualDeltaT
                                                let depth = initDepth + (actualCount + 1 |> double) * depthIncrement
                                                {Depth = depth 
                                                 Time = time} ) 
            |> Trajectory
             
            
        //let modelTransitionFunction (actualLEStatus:LEStatus) (nextDepth:float)  =

        //    let nextStepAmbientConditions = depth2AmbientCondition modelConstants nextDepth
            
        //    let updatedTissueTensions = 
        //        actualLEStatus.LEPhysics.TissueTensions 
        //        |> Array.map2 (updateTissueTension modelConstants.IntegrationTime nextStepAmbientConditions.Pressures) modelConstants.LEParams  

        //    let getIntegratedRiskForThisTissue = getIntegratedRiskForThisDeltaT modelConstants.IntegrationTime nextStepAmbientConditions.Pressures
            
        //    let integratedRisks = updatedTissueTensions 
        //                           |> Array.map2 getIntegratedRiskForThisTissue modelConstants.LEParams
                                   
        //    let updateAccruedRisk = 
        //        integratedRisks
        //        |> Seq.sum
        //        |> (+) actualLEStatus.Risk.AccruedRisk

        //    0.0
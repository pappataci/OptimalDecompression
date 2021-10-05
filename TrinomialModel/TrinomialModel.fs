[<AutoOpen>]
module TrinomialModel
    
    [<AutoOpen>]
    module Profile = 

        type DepthTime =     { Depth : double
                               Time  : double }

        
        type Trajectory  = |Trajectory of seq<DepthTime>

    [<AutoOpen>]
    module ModelPhysics = 
        type TissueTension                     = |Tension   of  float 

        let inline (+>) (Tension x:TissueTension ) (Tension y:TissueTension)  = 
            (x + y) 
            |> Tension 

    [<AutoOpen>]
    module Mission = 
        
        type Node =  {    EnvInfo : DepthTime
                          MaxDepth : double
                          AscentTime: double
                          TissueTensions : TissueTension[]
                          ExternalPressures : ExternalPressureConditions
                          InstantaneousRisk : double[]
                          IntegratedRisk : double[]
                          IntegratedWeightedRisk : double[]
                          AccruedWeightedRisk : double[]
                          TotalRisk : double} 

        type NodeEvolution = | NodeEvolution of seq<Node>

    [<AutoOpen>]
    module ModelDefinition = 

        let modelParams = {CrossOver  = [|     1000.0   ;    0.236795821    ;      1000.0   |] 
                           Rates      = [| 1.0 / 1.7727676636E+00 ; 1.0 / 6.0111598753E+01  ;  1.0 / 5.1128788835E+02  |] 
                           Gains      = [| 3.0918150923E-03 ; 1.1503684782E-04 ; 1.0805385353E-03 |]
                           //Gains = [|1.0 ; 1.0 ; 1.0|]
                           Thresholds = [| 0.0000000000E+00 ; 0.0000000000E+00 ; 6.7068236527E-02 |]}
        
        let  trinomialScaleFactor  = 0.134096478 
        let maxIntegrationTime = 0.01  // min // this was 0.01 changed for debugging
        
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

        let private getLETissueTensionIncrement iTissue deltaT pressures ( actualTissueTension:TissueTension )  =
            let integrationFcn = chooseAppropriateModelDependingOnTissueTensionNForcingTerm iTissue actualTissueTension pressures
            integrationFcn iTissue deltaT actualTissueTension pressures
            |> Tension

        let private updateTissueTension  deltaT pressures iTissue  actualTension =
            (getLETissueTensionIncrement iTissue deltaT pressures actualTension) 
            |> (+>) actualTension
       
        let depth2AmbientCondition depth =
                 
            let ambientPressure = depth2AmbientPressure depth
            let nitrogenPressure = ambientPressure 
                                   |> externalN2Pressure
            {Ambient = ambientPressure 
             Nitrogen = nitrogenPressure} 

        let getTissueTensionsAtDepth externalPressureCondition =
            Tension externalPressureCondition.Nitrogen
            |> Array.create modelParams.Rates.Length 
             
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

        // instanntaneous risks are not weighted with gains, yet
        let private getInstantaneousRisk pressures iTissue (Tension updatedTissueTension) = 
            
            ( updatedTissueTension - pressures.Ambient - ( modelParams.Thresholds.[iTissue]  - dPFVG ) ) / pressures.Ambient
            |> Operators.max 0.0

        // this is the Markov transition (state -> action -> state)  for ONE delta t
        let oneStepInTimeTransitionFunction (actualNode:Node) (action:DepthTime) : Node = 
            
            let nextAmbientConditions = depth2AmbientCondition action.Depth
            let actualTissueTensions = actualNode.TissueTensions
            let deltaT = action.Time - actualNode.EnvInfo.Time
            
            let nextTissueTensions = Array.mapi (updateTissueTension deltaT nextAmbientConditions) actualTissueTensions
            let instantaneousRisks = Array.mapi (getInstantaneousRisk  nextAmbientConditions)  nextTissueTensions
            let integratedRisks = instantaneousRisks 
                                  |> Array.map  ( (*) deltaT ) 

            let integratedWeightedRisks = Array.map2 (*) integratedRisks modelParams.Gains
            let tolerance = 1.0e-7
            let maxDepth , hasBeenUpdated = 
                if action.Depth + tolerance>= actualNode.MaxDepth then
                    action.Depth , true
                else 
                    actualNode.MaxDepth , false
            
            let resetAscentTimeIfNewMaxDepth (initNode:Node) hasMaxDepthBeenUpdated (ascentDuration:double) =   
                if hasMaxDepthBeenUpdated then  
                    0.0
                else
                    initNode.AscentTime + ascentDuration

            //Logger.LoggerSettings.addToLogger(seq{ "initDepth, nextDepth, nextNodeTime, maxDepth, updated" ; 
            //                                  actualNode.EnvInfo.Depth.ToString() + " " + action.Depth.ToString() + " " + action.Time.ToString() + " " + maxDepth.ToString() + " "
            //                                  + hasBeenUpdated.ToString()  })

            let actualAscentTime = resetAscentTimeIfNewMaxDepth actualNode hasBeenUpdated deltaT
            let updatedAcrruedRisk  =  Array.map2 (+) integratedWeightedRisks actualNode.AccruedWeightedRisk
            { EnvInfo = action
              MaxDepth = maxDepth
              AscentTime = actualAscentTime
              TissueTensions = nextTissueTensions
              ExternalPressures  = nextAmbientConditions
              InstantaneousRisk = instantaneousRisks
              IntegratedRisk = integratedRisks
              IntegratedWeightedRisk = integratedWeightedRisks
              AccruedWeightedRisk = updatedAcrruedRisk
              TotalRisk = updatedAcrruedRisk |> Array.sum} 

        // this is the Markov function (state -> initNode -> state) for one discrete action
        let oneActionStepTransition (initNode: Node) (action:DepthTime) : Node  =  
            let (Trajectory discretizedTraj)  = action   
                                                    |> inBetweenNodesTimeDiscretization initNode.EnvInfo
            
            let internalSeqOfNodes = discretizedTraj
                                     |> Seq.scan oneStepInTimeTransitionFunction initNode
            internalSeqOfNodes
            |>Seq.last

        let isAccrueingRisk tissueTensionThreshold (Tension tissueTension) = 
            let surfaceAmbientPressure = 1.0
            tissueTension > surfaceAmbientPressure +  tissueTensionThreshold - dPFVG

        let accrueingRiskAtSurface  (actualTissueTensions: TissueTension[])  = 
            actualTissueTensions 
            |> Array.map2 isAccrueingRisk  modelParams.Thresholds
            |> Array.reduce (||)

        let runModelUntilZeroRisk (initNodeAtSurface: Node) = 
            let initTime = initNodeAtSurface.EnvInfo.Time
            let infiniteSeqOfDepthAndTime = 
                Seq.initInfinite (fun count -> {Depth = 0.0 ; Time = initTime + (float (count + 1 ) ) * maxIntegrationTime})
            infiniteSeqOfDepthAndTime
            |> Seq.scan oneStepInTimeTransitionFunction initNodeAtSurface
            |> SeqExtension.takeWhileWithLast ( fun x ->  x.TissueTensions
                                                          |> accrueingRiskAtSurface ) 
            |> Seq.last 
            |> (fun node -> {node with AscentTime = initNodeAtSurface.AscentTime})

        let getSimulationMetric(simSolution : seq<Node>) = 
            simSolution
            |> Seq.last
            |> (fun lastNode -> lastNode.AscentTime , lastNode.TotalRisk)
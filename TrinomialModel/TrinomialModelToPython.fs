namespace TrinomialModToPython

module ToPython =
    let internalActionStepTransition (initNode: Node, nextTime: double , nextDepth : double) =
        let nextDepthTime = {Time = nextTime; Depth = nextDepth}
        oneActionStepTransition initNode nextDepthTime
    
    let createInitNodeWithThesePressAtDepth( press0, press1, press2 , depth) = 
        let envInfo = {Time = 0.0; Depth = depth}

        let tensions = [|press0; press1;press2|]
                        |> Array.map Tension
        let externalPressures = depth2EnvPressures depth
        let (zeroVec:double[])  = Array.zeroCreate  ( ModelDefinition.modelParams.Rates |> Array.length )  
        
        {EnvInfo = envInfo  
         TissueTensions = tensions
         MaxDepth = depth
         AscentTime = 0.0
         ExternalPressures = externalPressures
         InstantaneousRisk = zeroVec
         IntegratedRisk = zeroVec
         IntegratedWeightedRisk = zeroVec 
         AccruedWeightedRisk = zeroVec 
         TotalRisk  = 0.0}
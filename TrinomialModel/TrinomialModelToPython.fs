namespace TrinomialModToPython

module ToPython =

    let  ascentRate = ascentRate
    let descentRate = descentRate

    let getTables() = 
        table9FileName
        |> getTableOfInitialConditions

    let isAtSurface (depth:double) =
        abs(depth) < 1.0E-7 // tolerance for being at surface
    
    let stepFunction (initNode: Node, nextTime: double , nextDepth : double) =
        let nextDepthTime = {Time = nextTime; Depth = nextDepth}
        let nextNode =  oneActionStepTransition initNode nextDepthTime

        if nextDepth |> isAtSurface  then
            runModelUntilZeroRisk nextNode
        else
            nextNode
          
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

    let nodeToStateVec (node) = 
        let tissueTensions = node.TissueTensions
                             |> Array.map (fun (Tension t ) -> t  )
        
        Array.append tissueTensions [|node.ExternalPressures.Nitrogen ; node.TotalRisk|]
        
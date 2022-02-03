namespace TrinomialModToPython

module ToPython =

    let ascentRate = ascentRate
    let descentRate = descentRate

    let validateTablesData maybeTableInitCond maybeTableStrats =
        
        match (maybeTableInitCond , maybeTableStrats) with
        | (Some tableInitCond, Some tableStrategies) ->  tableInitCond , tableStrategies
        | _ -> table9FileName
               |> getTableInitialConditionsAndTableStrategies

    let getTables() =        
        let maybeTableInitConditions = tryReadTableMissionsMetricsFromFile tableInitConditionsFile
        let maybeTableStrategies = tryReadTableStrategiesFromFile tableStrategiesFile
        validateTablesData maybeTableInitConditions maybeTableStrategies

    let getMapOfDenseInitConditions() = 
        let readMap = mapOfInitialConditionsFile
                      |> readObjFromFile<Map<double, TableMissionMetrics[]>>
                      
        match readMap with
        | Some m -> fun depth -> m.[depth]
        | None -> fun _ -> printfn "Empty map"
                           null

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
    
    // just used for debugging and testing from Python
    let createNode(dpth, tm, t0,t1,t2,  totRisk) = 
        let envInfo = {Depth = dpth; Time = tm}
        let tensions = [|t0;t1;t2|]
                       |> Array.map Tension
         
        let dummyZeroes : float[]= Array.zeroCreate (modelParams.Gains|>Seq.length )

        {EnvInfo = envInfo
         MaxDepth = dpth
         AscentTime = 0.0
         TissueTensions = tensions
         ExternalPressures = depth2AmbientCondition dpth 
         InstantaneousRisk = dummyZeroes
         IntegratedRisk = dummyZeroes
         IntegratedWeightedRisk = dummyZeroes
         AccruedWeightedRisk = dummyZeroes
         TotalRisk = totRisk}
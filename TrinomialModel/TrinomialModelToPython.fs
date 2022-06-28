namespace TrinomialModToPython
open Newtonsoft.Json

module ToPython =

    let ascentRate = ascentRate
    let descentRate = descentRate
    let modelParams = modelParams

    let crudeStateValueEstimate (n:Node) = 
        (tissueTensionsToIntegratedWeightedRisks n.TissueTensions n.ExternalPressures 1.0)
        |>  Array.sum

    let validateTablesData maybeTableInitCond maybeTableStrats =
        
        match (maybeTableInitCond , maybeTableStrats) with
        | (Some tableInitCond, Some tableStrategies) ->  tableInitCond , tableStrategies
        | _ -> table9FileName
               |> getTableInitialConditionsAndTableStrategies

    let getTablesWithFileNames initCondFileName strategiesFileName = 
        let maybeTableInitConditions = tryReadTableMissionsMetricsFromFile initCondFileName
        let maybeTableStrategies = tryReadTableStrategiesFromFile strategiesFileName
        validateTablesData maybeTableInitConditions maybeTableStrategies

    let getTables() = 
        getTablesWithFileNames tableInitConditionsFile tableStrategiesFile
        
    let getTablesNoExc() =
        getTablesWithFileNames tableInitCondNoExpFile tableStrategiesNoExpFile

    let getMapOfInitConditionsFromFile fileName =
        let readMap = fileName
                      |> readObjFromFile<Map<double, TableMissionMetrics[]>>
        
        match readMap with
        | Some m -> fun depth -> m.[depth]
        | None -> fun _ -> printfn "Empty map"
                           null
                           
    let getMapOfDenseInitConditions() =  getMapOfInitConditionsFromFile mapOfInitialConditionsFile

    let getMapOfDenseInitCondNoExp() = getMapOfInitConditionsFromFile mapOfInitCondNoExpFile
    
    let generalStepFunction runModelUntilZeroRisk (initNode: Node, nextTime: double , nextDepth : double) =
        let nextDepthTime = {Time = nextTime; Depth = nextDepth}
        let nextNode =  oneActionStepTransition initNode nextDepthTime
        if nextDepth |> isAtSurface  then
            runModelUntilZeroRisk nextNode
        else
            nextNode
    
    let stepFunction (initNode: Node, nextTime: double , nextDepth : double)  =
        generalStepFunction runModelUntilZeroRisk (initNode, nextTime , nextDepth ) 
        

    let getValueIfSomeOtherwiseDefault (f:Option<'T>) (defaultValue:'T) =
        match f with
        | Some x -> x
        | None -> defaultValue

    let isAtConstantDepth (aNode:Node) nextDepth = 
        abs(aNode.EnvInfo.Depth - nextDepth) < 1.0e-2

    let createStepFunctionWithSurrogates zeroSurfaceRiskModel constantDepthSurrogateModel = 
        
        let atSurfaceModel = getValueIfSomeOtherwiseDefault zeroSurfaceRiskModel runModelUntilZeroRisk
        let computationFunction(aNode:Node, nextTime:double, nextDepth: double) = 
            match (aNode, nextTime, nextDepth) with                                                                                 
            | initNode, _, nextDepth when (isAtConstantDepth initNode nextDepth )  ->   constantDepthSurrogateModel aNode
            | _  -> generalStepFunction atSurfaceModel (aNode, nextTime, nextDepth)
        computationFunction
    
    let zeroDepthSurrogateFromDisk = surfacePressureFileName
                                     |> createRunningUntilZeroRiskSurrogateFromDisk

    let innerStepFunctionWSurrogate  =   match zeroDepthSurrogateFromDisk with
                                                 | Some x -> generalStepFunction x 
                                                 | None -> generalStepFunction runModelUntilZeroRisk

    let stepFunctionWSurrogate(initNode:Node, nextTime:double, nextDepth:double) = 
        innerStepFunctionWSurrogate(initNode, nextTime, nextDepth)
                              
    let createInitNodeWithThesePressAtDepth( press0, press1, press2 , depth) = 
        let envInfo = {Time = 0.0; Depth = depth}
        let tensions = [|press0; press1;press2|]
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
        
        Array.append tissueTensions [|node.ExternalPressures.Nitrogen ; node.TotalRisk|]

    let nodeToStateResidualRisk (node:Node, riskBound) = 
        {node with TotalRisk = riskBound - node.TotalRisk}
        |> nodeToStateVec
    
    let generalStepFcnResidual stepFcn (initNode, nextTime, nextDepth, riskBound) =
        let nextNode = stepFcn(initNode, nextTime, nextDepth)
        let stateWResidualRisk = nodeToStateResidualRisk(nextNode, riskBound)
        nextNode, stateWResidualRisk

    let stepFcnResidual(initNode, nextTime, nextDepth, riskBound) =
        generalStepFcnResidual stepFunction (initNode, nextTime, nextDepth, riskBound) 

    let stepFcnSurrogateResidual(initNode, nextTime, nextDepth, riskBound) =
        generalStepFcnResidual stepFunctionWSurrogate (initNode, nextTime, nextDepth, riskBound) 

    // just used for debugging and testing from Python
    let createNode(dpth, tm, t0,t1,t2,  totRisk) = 
        let envInfo = {Depth = dpth; Time = tm}
        let tensions = [|t0;t1;t2|]
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

    let setRisk(tmm:TableMissionMetrics, updatedRisk) = 
        {tmm with TotalRisk = updatedRisk }

    let string2MissionMetrics(content:string)= 
        content
        |> JsonConvert.DeserializeObject<TableMissionMetrics>

    let createTableMissionMetrics(maxDepth, bottomTime, totalRisk) =
        let initNode =getInitAscentNodeCondition maxDepth bottomTime
        let totalAscentTime = nan
        {MissionInfo = {MaximumDepth = maxDepth
                        BottomTime = bottomTime
                        TotalAscentTime = totalAscentTime}
         TotalRisk = totalRisk
         InitAscentNode = initNode}

    let createTableMissionMetricsNoHistory(t0,t1,t2, depth, optionTotalRisk:option<float>) =
        
        let totalRisk = match optionTotalRisk with
                        | Some  x -> x 
                        | None -> 1.0 // default value for running the mission until the surface

        let maxDepth,  bottomTime, totAscentTime = -1.0, -1.0, 0.0
        
        let missionInfo = {MaximumDepth = maxDepth
                           BottomTime = bottomTime
                           TotalAscentTime = totAscentTime}
        let tm = 0.0
        let initAscentNode = createNode(depth, tm, t0, t1, t2, totalRisk)
        
        {MissionInfo = missionInfo
         TotalRisk = totalRisk
         InitAscentNode = initAscentNode}
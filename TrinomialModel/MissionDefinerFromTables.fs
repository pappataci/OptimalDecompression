[<AutoOpen>]
module MissionDefinerFromTables

open ModelRunner

type TableMissionMetrics = { MissionInfo: MissionInfo
                             TotalRisk: double
                             InitAscentNode: Node }     

let resetTimeForNode (aNode:Node) = 
    {aNode with EnvInfo ={Depth = aNode.EnvInfo.Depth ;
                          Time = 0.0}}


let partitionSequenceInAscentDescent (initSequence:seq<DepthTime> , missionInfo:MissionInfo)  = 
    let timeTol = 1.0E-4
    initSequence
    |> Seq.toArray
    |> Array.partition ( fun x -> x.Time < missionInfo.BottomTime - timeTol)
    
let createDepthsGrid (initDepthTime:DepthTime,finalDepthTime:DepthTime) : float[] = 
   [|initDepthTime.Depth ..  -deltaDepthForRefinedNodeMapping .. finalDepthTime.Depth|]
   |> Array.map (fun depth -> System.Math.Round(depth, 3))

let getArrayWithoutNLastElement numElem (anArray:'T[]) = 
   anArray.[0 .. (anArray.Length - 1 - numElem) ]

let getArrayWithoutLastElement  = 
   getArrayWithoutNLastElement 1 

let getAscentTimes (ascentArray:DepthTime[]) = 
   ascentArray
   |> getArrayWithoutLastElement
   |> Array.map (fun x -> x.Time)

let createTimeSequences (initTime:float) numberOfElements  = 
    let timeIncrement = deltaDepthForRefinedNodeMapping / ( abs ascentRate  ) 
    (fun idx -> initTime + (float idx) * timeIncrement)
    |> Array.init numberOfElements 
 
let getRefinedAscentSequenceWithDeltaDepth  (ascentArray:DepthTime[])  = 
    let depthTimePairs = ascentArray |> Array.pairwise
    let refinedDepths = depthTimePairs |> Array.map createDepthsGrid
    let initIntervalTimes = getAscentTimes ascentArray
    let times =  initIntervalTimes 
                 |> Array.map2 (fun refinedDepthSection initTime ->   createTimeSequences  initTime (refinedDepthSection |> Array.length) )   refinedDepths 

    let ascentWithoutLastNode  = refinedDepths
                                |> Array.map2 (fun timeArray depthArray -> Array.zip timeArray depthArray) times 
                                |> Array.filter (fun x -> x.Length > 1 ) // get rid of repetitions
                                |> Array.concat
                                |> Array.filter (fun (_, depth) -> depth > minDepthForStop - 1.0e-5)
                                |> Array.map (fun (time,depth) -> {Time = time; Depth = depth})
    Array.concat [ascentWithoutLastNode ; [| (ascentArray |> Array.last) |]]
    
let createRedefinedSequence  (initSequence:seq<DepthTime> , missionInfo:MissionInfo) : seq<DepthTime> = 
    let descentArray, ascentArray = partitionSequenceInAscentDescent (initSequence , missionInfo)
    let refinedAscentSequence = getRefinedAscentSequenceWithDeltaDepth  ascentArray 
    Array.concat [descentArray ; refinedAscentSequence]
    |> Array.toSeq

let getInitialConditionNodeWithoutResettingTime (solutionOfSeqOfNodes:seq<Node>)  (missionInfo: MissionInfo) = 
    let isNodeAtBottomTime aNode = abs( aNode.EnvInfo.Time - missionInfo.BottomTime) < 1.0e-10
    solutionOfSeqOfNodes  
    |> Seq.find isNodeAtBottomTime

let getInitialConditionNode (solutionOfSeqOfNodes:seq<Node>)  (missionInfo: MissionInfo) =     
    missionInfo
    |>getInitialConditionNodeWithoutResettingTime  solutionOfSeqOfNodes
    |> resetTimeForNode

let getTensionToRiskAtSurface (solutionOfSeqOfNodes:seq<Node>) =
    let seqLength = solutionOfSeqOfNodes |> Seq.length
    let previousToLastNode  = solutionOfSeqOfNodes |> Seq.item (seqLength - 2 )
    let lastNode = solutionOfSeqOfNodes |> Seq.last
    let tensionsAtSurface = previousToLastNode.TissueTensions
    let toGoRisk = lastNode.TotalRisk - previousToLastNode.TotalRisk
    tensionsAtSurface, toGoRisk

let getTensionToIndividualRisksAtSurface (solutionOfSeqOfNodes:seq<Node>) = 
    let seqLength = solutionOfSeqOfNodes |> Seq.length
    let previousToLastNode  = solutionOfSeqOfNodes |> Seq.item (seqLength - 2 )
    let lastNode = solutionOfSeqOfNodes |> Seq.last
    let tensionsAtSurface = previousToLastNode.TissueTensions
    let riskIncrement = Array.map2 (-) lastNode.AccruedWeightedRisk previousToLastNode.AccruedWeightedRisk
    tensionsAtSurface, riskIncrement

let getTableMetrics (initAscentNode:Node) (lastNode:Node)  (missionInfo: MissionInfo) : TableMissionMetrics =
    {MissionInfo = missionInfo
     TotalRisk = lastNode.TotalRisk
     InitAscentNode = initAscentNode }

let getInitialConditionsFromSolution (modelSolution:seq<Node>)  (tableParams: MissionInfo) = 
    let initAscentNode = getInitialConditionNode modelSolution tableParams
    let lastNode = modelSolution 
                    |> Seq.last
    tableParams
    |> getTableMetrics initAscentNode lastNode

let fromSolutionModelToInitAscentNode (tableParams:MissionInfo) finalRisk (nodeSolution:Node)   :TableMissionMetrics =
    
    let countAscentTimeFromCurrentNode (node:Node) = 
        tableParams.TotalAscentTime +  tableParams.BottomTime - node.EnvInfo.Time

    let getResidualRiskFromCurrentNode (node:Node) = 
        finalRisk - node.TotalRisk

    let defineInitialNode(node:Node) = 
         
        let zeroVec = modelParams.Gains.Length
                      |> Array.zeroCreate
                      
        {node with EnvInfo = {node.EnvInfo with Time = 0.0}
                   TotalRisk = 0.0
                   AccruedWeightedRisk = zeroVec
                   AscentTime = 0.0
                   InstantaneousRisk = zeroVec
                   IntegratedRisk = zeroVec 
                   IntegratedWeightedRisk = zeroVec }

    {MissionInfo = {tableParams with TotalAscentTime = countAscentTimeFromCurrentNode nodeSolution } 
     TotalRisk = getResidualRiskFromCurrentNode nodeSolution
     InitAscentNode = defineInitialNode nodeSolution}  

let getArrayInitCondFromDenseSolution (tableParams:MissionInfo) (modelSolution:seq<Node>)  = 
    let initAscentNode = getInitialConditionNode modelSolution tableParams
    let timeTol = 1.0e-5
    let finalRisk = modelSolution
                    |> Seq.last
                    |> (fun node -> node.TotalRisk)

    let initAscentMapper = fromSolutionModelToInitAscentNode tableParams finalRisk


    modelSolution
    |> Seq.filter (fun aNode -> aNode.EnvInfo.Time >= initAscentNode.EnvInfo.Time - timeTol ) 
    |> Seq.map initAscentMapper
    |> Seq.toArray
    |> getArrayWithoutNLastElement 2 
    |> Seq.ofArray

let depthTimetoDepth (depthTime:DepthTime) = 
    depthTime.Depth

let depthToAction (strategy: seq<float> )  =
    
    let internalSeq = strategy   
                    |> Seq.pairwise
                    |> Seq.map (fun (previousDepth, actualDepth) -> match abs(previousDepth - actualDepth) < 1.0e-3 with
                                                                    | true -> 1.0
                                                                    | _ -> 0.0 )
    seq { yield 0.0
          yield! internalSeq}
    |> Seq.toArray

let generalizedInitialConditions initialConditionGetter tableFileName =
    let seqDepthAndTimeFromTables , missionInfos =  tableFileName
                                                    |> getDataContent
                                                    |> Array.map data2SequenceOfDepthAndTime
                                                    |> Array.unzip

    let solutions = seqDepthAndTimeFromTables |> Array.Parallel.map  runModelOnProfileUsingFirstDepthAsInitNode
    let tableMissionMetrics, tableStrategies = Array.map2 initialConditionGetter solutions missionInfos , seqDepthAndTimeFromTables
    tableMissionMetrics , tableStrategies

let getTableOfInitialConditions = generalizedInitialConditions getInitialConditionsFromSolution

let getAllInitConditionsFromSeq = 
    createRedefinedSequence
    >> runModelOnProfileUsingFirstDepthAsInitNode

let getFinalRiskFromSolution = Seq.last
                               >> (fun (node:Node) -> node.TotalRisk)

let getAscentInitCondition  initAscentNode (ascentNodeToInitConditionMpapper:Node -> TableMissionMetrics)  =
    let timeTol = 1.0e-5
    Seq.filter (fun (aNode:Node) -> aNode.EnvInfo.Time >= initAscentNode.EnvInfo.Time - timeTol ) 
     >> Seq.map ascentNodeToInitConditionMpapper
    >> Seq.toArray
    >> getArrayWithoutNLastElement 2 

let seqNMissionInfoToSeqMissionInfo (seqMissionInfo:seq<DepthTime>*MissionInfo) : TableMissionMetrics[] =
    let _, missionInfo = seqMissionInfo
    let modelSolution = seqMissionInfo
                         |> getAllInitConditionsFromSeq
    let initAscentNode = getInitialConditionNodeWithoutResettingTime modelSolution missionInfo
    let finalRisk = modelSolution
                    |> getFinalRiskFromSolution
    let initAscentMapper = fromSolutionModelToInitAscentNode missionInfo finalRisk
    modelSolution
    |> getAscentInitCondition initAscentNode initAscentMapper

let tableFileToInitConditions =  getDataContent
                                 >> Array.map data2SequenceOfDepthAndTime
                                 >> Array.Parallel.map seqNMissionInfoToSeqMissionInfo
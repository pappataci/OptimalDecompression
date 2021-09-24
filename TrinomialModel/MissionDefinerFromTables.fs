[<AutoOpen>]
module MissionDefinerFromTables

open ModelRunner

type TableMissionMetrics = { MissionInfo: MissionInfo
                             TotalRisk: double
                             InitAscentNode: Node }     

let resetTimeForNode (aNode:Node) = 
    {aNode with EnvInfo ={Depth = aNode.EnvInfo.Depth ;
                          Time = 0.0}}

let getInitialConditionNode (solutionOfSeqOfNodes:seq<Node>)  (missionInfo: MissionInfo) = 
    
    let isNodeAtBottomTime aNode = abs( aNode.EnvInfo.Time - missionInfo.BottomTime) < 1.0e-10

    solutionOfSeqOfNodes  
    |> Seq.find isNodeAtBottomTime
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

let getInitialConditionAndTargetForTable (tableSeqODepths:seq<DepthTime> , tableParams: MissionInfo) =
    let modelSolution = runModelOnProfileUsingFirstDepthAsInitNode tableSeqODepths
    getInitialConditionsFromSolution modelSolution tableParams

let getTableOfInitialConditions tableFileName = 
    let seqDepthAndTimeFromTables , missionInfos =  table9FileName
                                                    |> getDataContent
                                                    |> Array.map data2SequenceOfDepthAndTime
                                                    |> Array.unzip

    let solutions = seqDepthAndTimeFromTables |> Array.Parallel.map  runModelOnProfileUsingFirstDepthAsInitNode
    Array.map2 getInitialConditionsFromSolution solutions missionInfos
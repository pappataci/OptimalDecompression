[<AutoOpen>]
module MissionDefinerFromTables

type TableMissionMetrics = { MissionInfo: MissionInfo
                             TotalRisk: double
                             InitAScentNode: Node }     

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
[<AutoOpen>]
module MissionDefinerFromTables

type TableMissionMetrics = {MissionInfo: MissionAscentInfo
                            TotalRisk  : double}            

let resetTimeForNode (aNode:Node) = 
    {aNode with EnvInfo ={Depth = aNode.EnvInfo.Depth ;
                          Time = 0.0}}

let getInitialConditionNode (solutionOfSeqOfNodes:seq<Node>)  (missionInfo: MissionAscentInfo) = 
    solutionOfSeqOfNodes  
    |> Seq.find (fun aNode ->  aNode.EnvInfo.Time = missionInfo.BottomTime)
    |> resetTimeForNode
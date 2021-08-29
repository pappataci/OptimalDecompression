#load "SeqExtension.fs"
#load "Gas.fs"
#load "ELModelCommon.fs"
#load "TrinomialModel.fs"
#load "TableDataInputs.fs"
#load "TableReader.fs"
#load "ProfileIntegrator.fs"
#load "MissionDefinerFromTables.fs"
//open ProfileIntegrator
open ModelRunner

let profilingOutput  = fileName
                                            |> getDataContent
                                            |> Array.map data2SequenceOfDepthAndTime


let getTableMetrics (initAscentNode:Node) (lastNode:Node)  (missionInfo: MissionInfo) : TableMissionMetrics =
    
    {MissionInfo = missionInfo
     TotalRisk = lastNode.TotalRisk
     InitAScentNode = initAscentNode
     }


let getInitialConditionAndTargetForTable (tableSeqODepths:seq<DepthTime> , tableParams: MissionInfo) =
    
    let modelSolution = runModelOnProfile tableSeqODepths
    let initAscentNode = getInitialConditionNode modelSolution tableParams
    let lastNode = modelSolution 
                   |> Seq.last 
    tableParams
    |> getTableMetrics initAscentNode lastNode
    

//let getTensionDistributionForThisTissue 


let solutions = profilingOutput |> Array.Parallel.map  ( fun( x,   _ )  -> runModelOnProfile x ) 

let tableInitialConditions = profilingOutput |> Array.Parallel.map getInitialConditionAndTargetForTable

let tensionToRiskTable = solutions |> Array.Parallel.map getTensionToRiskAtSurface




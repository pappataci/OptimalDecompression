// Learn more about F# at https://fsharp.org
// See the 'F# Tutorial' project for more help.
#load "SeqExtension.fs"
#load "Gas.fs"
#load "ELModelCommon.fs"
#load "TrinomialModel.fs"
#load "TableDataInputs.fs"
#load "TableReader.fs"
#load "ProfileIntegrator.fs"
#load "MissionDefinerFromTables.fs"

let profilingOutput  = fileName
                                            |> getDataContent
                                            |> Array.map data2SequenceOfDepthAndTime
                                            |> Array.unzip
let  computedNodeSeq , ascentParams = profilingOutput

let testProfile =  computedNodeSeq.[0]




let profileOut = runModelOnProfile testProfile
                 //|> Seq.last 
profileOut  |> Seq.item 2

let problematicProfile = computedNodeSeq.[376]

let getTableMetrics (lastNode:Node)  (missionInfo: MissionAscentInfo) : TableMissionMetrics =
    {MissionInfo = missionInfo
     TotalRisk = lastNode.TotalRisk}

getInitialConditionNode profileOut ascentParams.[0]

let testProblematicOut = runModelOnProfile problematicProfile  
//module  Params = 
//    let mutable A = 1.0


//module Mammolo = 
//    let f x = Params.A * x 

//module ChangeParams  =
//    let changeParams x = 
//        Params.A <- x 

// OK THis is interesting for changing the parameters

let pSeriousDCS node = 1.0 - exp(-trinomialScaleFactor * node.TotalRisk)
let pMildDCS node = (1.0 - exp(-node.TotalRisk)) * (1.0 - pSeriousDCS node)
let pNoDCSEvent node = exp( -(trinomialScaleFactor + 1.0) * node.TotalRisk)

//  

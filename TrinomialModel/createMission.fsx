#r @"C:\Users\glddm\.nuget\packages\newtonsoft.json\13.0.1\lib\net45\Newtonsoft.Json.dll"
#r @"C:\Users\glddm\.nuget\packages\fsharp.data\3.3.3\lib\net45\FSharp.Data.dll"
#r @"C:\Users\glddm\.nuget\packages\fsharp.stats\0.4.3\lib\netstandard2.0\FSharp.Stats.dll"

#load "SeqExtension.fs"
#load "Gas.fs"
#load "ELModelCommon.fs"
#load "TrinomialModel.fs"
#load "TableDataInputs.fs"
#load "TableReader.fs"
#load "ProfileIntegrator.fs"
#load "MissionDefinerFromTables.fs"
//#load "MissionSerializer.fs"
//#load "SurrogateModelCreation.fs"
//#load "TableToDiscreteActionsSeq.fs"
//#load "TrinomialModelToPython.fs"

//open TrinomialModToPython.ToPython
open FSharp.Stats.Interpolation
open ModelRunner


let maxDepthBottomTimeToSeqDepthTime (maxDepth: double) (bottomTime:double) : seq<DepthTime> = 
    let start = defineDepthAndTime (0.0, 0.0)
    let maxDepthStart = defineDepthAndTime ( maxDepth, maxDepth/descentRate )
    let maxDepthEnd = defineDepthAndTime(maxDepth, bottomTime)
    seq{yield start
        yield maxDepthStart
        yield maxDepthEnd}

let getInitAscentNodeCondition maxDepth bottomTime : Node =
    bottomTime
    |> maxDepthBottomTimeToSeqDepthTime maxDepth
    |> runModelOnProfileUsingFirstDepthAsInitNode
    |> Seq.last


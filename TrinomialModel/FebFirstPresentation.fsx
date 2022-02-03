#r @"C:\Users\glddm\.nuget\packages\fsharp.data\3.3.3\lib\net45\FSharp.Data.dll"

#r @"C:\Users\glddm\.nuget\packages\newtonsoft.json\13.0.1\lib\net45\Newtonsoft.Json.dll"
#load "SeqExtension.fs"
#load "Gas.fs"
#load "ELModelCommon.fs"
#load "TrinomialModel.fs"
#load "TableDataInputs.fs"
#load "TableReader.fs"
#load "ProfileIntegrator.fs"
#load "MissionDefinerFromTables.fs"
#load "MissionSerializer.fs"


// generate the data

let profilExample , _    = table9FileName
                        |> getDataContent
                        |> Array.head
                        |> data2SequenceOfDepthAndTime
                        //|> seqNMissionInfoToSeqMissionInfo


open FSharp.Data
open ModelRunner

type PressureTable = CsvProvider<Schema = "float, float, float,float,float", HasHeaders=false>

// create the descentData
let maxDepth = 90.0
let deltaT = 0.2
let bottomTime = 300.0

let incrementTime (initTime:double) (idx:int) =
    initTime + (double idx) * deltaT

let descentFract  = Seq.initInfinite (fun x -> let time = incrementTime 0.0 x
                                               {Depth =  time * descentRate ; Time = time } ) 
                       |> Seq.takeWhile (fun {Depth = d} -> d <= maxDepth)

let lastDepth  = descentFract 
                 |> Seq.last
                 |> (fun x-> x.Depth)

let descentFractionLast = match abs(lastDepth - maxDepth) > 1.0e-10 with
                          | true -> seq{{Depth = maxDepth   ; Time = abs(maxDepth / descentRate ) }  }
                          | false -> Seq.empty

let descentFraction = Seq.concat [descentFract ; descentFractionLast]


let getLastTime seqDepthTime = 
    seqDepthTime
    |> Seq.last
    |> ( fun {Time = t} -> t)


let bottomFraction' = Seq.initInfinite ( fun x -> let time =  incrementTime (getLastTime  descentFraction) x 
                                                  {Time = time; Depth = maxDepth } )
                     |> Seq.takeWhile (fun {Time = t } -> t <= bottomTime)
                     |> Seq.skip 1 // this belongs to previous sequence

let bottomFraction = Seq.concat [ bottomFraction' ; seq{ {Depth = maxDepth ; Time = bottomTime} } ]




let ascentDepths = [ for depth in maxDepth  ..  ascentRate*deltaT ..  -1.0e-15-> depth ] |> Seq.skip 1
let depthsCount = ascentDepths |> Seq.length 
let times = Seq.init depthsCount (fun idx -> bottomTime  + ( double idx  + 1.0 ) * deltaT)

let ascentFraction = Seq.map2 (fun t d -> {Time = t; Depth =d } ) times ascentDepths

let missionProfile = Seq.concat [descentFraction; bottomFraction; ascentFraction]

let modelSolution = runModelOnProfileUsingFirstDepthAsInitNode missionProfile


//"Time (double), Depth (double), P0 (double), P1 (double), P2 (double)"
// get relevant data from solution
let nodeToOutputData (n:Node) = 
                let tensions = n.TissueTensions
                               |> Array.map (fun (Tension t ) -> t )
                (n.EnvInfo.Time, n.EnvInfo.Depth, tensions.[0], tensions.[1], tensions.[2])

let outputData = modelSolution
                 |> Seq.map  (nodeToOutputData >>  PressureTable.Row ) 
                 |> ( fun x -> new PressureTable(x))
                 |> ( fun pressureTable -> pressureTable.Save (@"C:\Users\glddm\Desktop\exampleAscent.csv" ) )


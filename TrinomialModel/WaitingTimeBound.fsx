#r @"C:\Users\glddm\.nuget\packages\newtonsoft.json\13.0.1\lib\net45\Newtonsoft.Json.dll"
#r @"C:\Users\glddm\.nuget\packages\fsharp.data\3.3.3\lib\net45\FSharp.Data.dll"

#load "SeqExtension.fs"
#load "Gas.fs"
#load "ELModelCommon.fs"
#load "TrinomialModel.fs"
#load "TableDataInputs.fs"
#load "TableReader.fs"
#load "ProfileIntegrator.fs"
#load "MissionDefinerFromTables.fs"
#load "MissionSerializer.fs"
#load "TableToDiscreteActionsSeq.fs"
#load "TrinomialModelToPython.fs"

open TrinomialModToPython.ToPython

//let tables , ascentStrategies = getTables()

let completeMission, missionInfos =  table9FileName
                                      |> getDataContent
                                      |> Array.map data2SequenceOfDepthAndTime
                                      |> Array.unzip


let getAscent  (mission: seq<DepthTime> ) =
     mission 
     |> Seq.skip 3 


let ascents = 
    completeMission
    |> Array.map getAscent

let testIndex = 100

let getTimesNWaitingTime (ascent:seq<DepthTime>) = 
    ascent
    |> Seq.pairwise
    |> Seq.map (fun (current , next)  ->  match abs(current.Depth - next.Depth ) < 1.0e-8 with
                                          | true -> (current.Depth, next.Time - current.Time)
                                          | false -> (current.Depth, -1.0 )   )
    |> Seq.filter (fun (_, time) -> time >0.0)

let depthWithWaitingTimes = ascents 
                             |> Seq.map getTimesNWaitingTime
                             |> Seq.concat

let grouppedByDepth = depthWithWaitingTimes
                      |> Seq.groupBy ( fun (d,wt) -> d )

let depthAndWaits = grouppedByDepth
                         |> Seq.map ( fun (depth, values)  ->  (depth, values |> Seq.map snd) )
                         |> Seq.map (fun (d, wSeq) -> (d, wSeq |> Seq.max) )
                         |> Seq.toArray 



let depthAndWaits2 = depthAndWaits|> Seq.map ( fun (d, t) -> (round(d) , t))|> Seq.toArray

let dNWaits = depthAndWaits2 
                |> Array.groupBy ( fun (d,wt ) -> d ) 
                |> Array.map  ( fun (depth, values)  ->  (depth, values |> Seq.map snd) )
                |> Seq.map (fun (d, wSeq) -> (d, wSeq |> Seq.max) )

open FSharp.Data
type DepthWaiting = CsvProvider<Sample = "Depth, WaitingTime" , Schema = "Depth(float), WaitingTime(float)", HasHeaders=true>

let csvWriter = dNWaits
                |> Seq.map DepthWaiting.Row
                |> (fun n-> new DepthWaiting(n))

csvWriter.Save(@"C:\Users\glddm\Desktop\waitingTimes.csv")
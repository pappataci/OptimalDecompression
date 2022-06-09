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
#load "MissionSerializer.fs"
#load "SurrogateModelCreation.fs"
#load "TableToDiscreteActionsSeq.fs"
#load "TrinomialModelToPython.fs"

open TrinomialModToPython.ToPython
open FSharp.Stats.Interpolation
open ModelRunner

let  depth, bottomTime = 50.0, 120.0

let getSpecificMission x =   x.MissionInfo.MaximumDepth = depth
                             && x.MissionInfo.BottomTime = bottomTime

let tableOfInterest = getMapOfDenseInitCondNoExp() depth
                      |>Array.filter  getSpecificMission
                      

let missionMet, strategies = getTablesNoExc()

let missionIndex = missionMet 
                   |> Array.indexed
                   |> Array.filter ( fun (x,y) -> getSpecificMission y)
                   |> Array.head 
                   |> fst

let missionOfInterest = missionMet.[missionIndex]

let tablesStrategy = strategies.[missionIndex]

let initNode = missionOfInterest.InitAscentNode
let riskBOund = missionOfInterest.TotalRisk

let toInput (node:Node) time depth = (node, time, depth, riskBOund)
let getNextOut node time depth = toInput node time depth
                                 |> stepFcnSurrogateResidual


let scaledStrat = tablesStrategy
                  |> Seq.filter (fun x -> x.Time > bottomTime)
                  |> Seq.map (fun x -> {x with Time = x.Time - bottomTime} )
                  |> Seq.toArray 



             
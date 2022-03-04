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

open ModelRunner

//let out = getMapOfDenseInitConditions

//let tableOfInitCond = table9FileName
//                     |> tableFileToInitConditions // this is not dense (just the tables)

let t =  getTables()

let initConditions = getMapOfDenseInitConditions()


//let testProfileIdx = 310

let testDepth = 50.0

let profilesOfInterest = (initConditions testDepth ) |> Seq.sortByDescending (fun x -> x.InitAscentNode.TissueTensions.[0]) 

let initConditionMax = profilesOfInterest |> Seq.head
let initConditionMin = profilesOfInterest |> Seq.last


let waitForMinutesAndAscent minutesToWait = 
    seq{  yield  {Depth = testDepth 
                  Time = minutesToWait}
          yield {Time = minutesToWait - testDepth / ascentRate
                 Depth = 0.0}  }

let getAscentDistr tableMissionMetrics = [|0.0 ..1.0 .. 100.0|]
                                           |> Array.Parallel.map (waitForMinutesAndAscent 
                                                                 >> (runModelOnProfile tableMissionMetrics.InitAscentNode)  
                                                                 >> Seq.last)

let maxNodes = getAscentDistr  initConditionMax
let minNOdes = getAscentDistr initConditionMin

let integratedWeightedRiskMax = maxNodes |> Array.map (fun x-> x.AccruedWeightedRisk.[0]/ x.TotalRisk *100.0)
let integratedWeightedRiskMin = minNOdes |> Array.map (fun x-> x.AccruedWeightedRisk.[0]/ x.TotalRisk *100.0)

integratedWeightedRiskMax |> Seq.max
integratedWeightedRiskMax|> Seq.min

printf "crude approximation is not applicable. Idea parked"
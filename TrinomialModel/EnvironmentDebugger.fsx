
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

// load tables, without exceptional dives
let tables , strategy  = getTablesNoExc()

let tableEx = 95 // example
let actualTable = tables.[tableEx]

let strategyEx = strategy.[tableEx]


let initialNode = actualTable.InitAscentNode

let modelStrategy = [(0.0,30.0);(0.33333333,20.0);(49.59755707, 20.  );(50.26422373,  0.   ) ] 
                    |> List.map (fun (time, depth) -> {Time = time; Depth = depth})

let modelOut = runModelOnProfile  initialNode modelStrategy

modelOut |> Seq.toArray

// trying with surrogate
let riskBound = 0.09645298282
let node' , _ = stepFcnSurrogateResidual(initialNode, 0.0, 30.0, riskBound)
let node'' , _  =  stepFcnSurrogateResidual( node' , 0.33333333,20.0, riskBound)
let node''' , _  =  stepFcnSurrogateResidual( node'' , 49.59755707,  20.,  riskBound)
let finalNode , _ = stepFcnSurrogateResidual(  node''' , 50.26422373, 0.0,  riskBound)
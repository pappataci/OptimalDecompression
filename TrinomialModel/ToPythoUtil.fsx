#r @"C:\Users\glddm\.nuget\packages\newtonsoft.json\13.0.1\lib\net45\Newtonsoft.Json.dll"
#load "SeqExtension.fs"
#load "Gas.fs"
#load "ELModelCommon.fs"
#load "TrinomialModel.fs"
#load "TableDataInputs.fs"
#load "TableReader.fs"
#load "ProfileIntegrator.fs"
#load "MissionDefinerFromTables.fs"
#load "SurfaceTableCreator.fs"
#load "Diagnostics.fs"
#load "TableToDiscreteActionsSeq.fs"
#load "MissionSerializer.fs"
#load "TrinomialModelToPython.fs"
#load "MissionSerializer.fs"

open TrinomialModToPython.ToPython

//let initConditions, tableStrategies = getTables()

open System.IO 
open Newtonsoft.Json

//JsonConvert.SerializeObject

//let test = JsonConvert.SerializeObject initConditions

let tableMissionFile = @"C:\Users\glddm\Desktop\initCondition.data"
//System.IO.File.WriteAllText(outputFile, test)

let tableInitConditions, tableStrategies = getTables()
//tableInitConditionsFile , tableStrategiesFile

//let (Some tableObj) = deserializeObjectFromFile<TableMissionMetrics[]> outputFile
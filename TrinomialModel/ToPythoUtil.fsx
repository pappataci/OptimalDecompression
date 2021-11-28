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
#load "TrinomialModelToPython.fs"

open TrinomialModToPython.ToPython

//let initConditions, tableStrategies = getTables()

open Newtonsoft.Json

//JsonConvert.SerializeObject

//let test = JsonConvert.SerializeObject initConditions

let outputFile = @"C:\Users\glddm\Desktop\initCondition.data"
//System.IO.File.WriteAllText(outputFile, test)

let serializedData = System.IO.File.ReadAllText outputFile
let testData = JsonConvert.DeserializeObject<TableMissionMetrics[]>  serializedData

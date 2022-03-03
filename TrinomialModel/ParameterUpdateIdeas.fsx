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
#load "TableToDiscreteActionsSeq.fs"
#load "TrinomialModelToPython.fs"

open TrinomialModToPython.ToPython

//let testProblematicOut = runModelOnProfile problematicProfile  
//module  Params = 
//    let mutable A = 1.0


//module Mammolo = 
//    let f x = Params.A * x 

//module ChangeParams  =
//    let changeParams x = 
//        Params.A <- x 

// OK THis is interesting for changing the parameter
// %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%% //


//let newTableMissionMetrics, strategies = getTableInitialConditionsAndTableStrategies table9FileName
let tables, strats = getTables()

//dumpObjectToFile newTableMissionMetrics ( dataSrcFolder + @"\tableInitConditionsNew.json") 
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

// get all initial conditions in TableMissionMetrics[][] 
//let tableOfInitCond = table9FileName
//                     |> tableFileToInitConditions

//// collocate all initialCOnditions in one array
//let allInitialConditions = tableOfInitCond
//                            |> Array.concat

//let grouppedByDepth = allInitialConditions
//                     |> Array.groupBy (fun initCondition -> initCondition.InitAscentNode.EnvInfo.Depth)
//                     |> Array.sortBy ( fun (d,_ ) -> d )
                     

//let mapOfInitConditions = grouppedByDepth
//                            |> Map
                             

//dumpObjectToFile mapOfInitConditions @"C:\Users\glddm\Documents\Duke\Research\OptimalAscent\Table9_9\initConditionsMap.json"


// uncomment to read the data from disk
//let (Some mapOfTables) = readObjFromFile<Map<double, TableMissionMetrics[]>>   @"C:\Users\glddm\Documents\Duke\Research\OptimalAscent\Table9_9\initConditionsMap.json"

//let getKeys (aMap:Map<'K, 'V>)  = 
//    aMap
//    |> Map.toSeq
//    |> Seq.map fst

//let depths = mapOfTables |> getKeys

let myMap = getMapOfDenseInitConditions()
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

// get all initial conditions in TableMissionMetrics[][] 
let tableOfInitCond = table9FileName
                     |> tableFileToInitConditions

// collocate all initialCOnditions in one array
let allInitialConditions = tableOfInitCond
                            |> Array.concat

let grouppedByDepth = allInitialConditions
                     |> Array.groupBy (fun initCondition -> initCondition.InitAscentNode.EnvInfo.Depth)
                     |> Array.sortBy ( fun (d,_ ) -> d )
                     

let mapOfInitConditions = grouppedByDepth
                            |> Map
                             
let mapsOfDepths = mapOfInitConditions
                   |> Map.toSeq
                   |> Seq.map fst
                   //|> Seq.map fst


dumpObjectToFile mapOfInitConditions @"C:\Users\glddm\Documents\Duke\patroclo.json"
//let example = test.TryFind 25.0

//test.
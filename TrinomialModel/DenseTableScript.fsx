#r @"C:\Users\glddm\.nuget\packages\newtonsoft.json\13.0.1\lib\net45\Newtonsoft.Json.dll"
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
#load "TableToDiscreteActionsSeq.fs" 
#load "SurrogateModelCreation.fs"
#load "TrinomialModelToPython.fs"

open TrinomialModToPython.ToPython


//let predicate (_, missionInfo) = 
//    missionInfo.MaximumDepth <= 190.0 && missionInfo.TotalAscentTime <= 90.0 


//CREATION OF DENSE FILTERED DICTIONARY
//let mapOfInitConditions = table9FileName 
//                         |> filteredTableFileToInitConditions predicate
//                         |> tableMissionMetricsToDictByDepth


// uncomment to save the data to disk
//let fileName = @"C:\Users\glddm\Documents\Duke\Research\OptimalAscent\Table9_9\initConditionsMapNoExcept.json"
//dumpObjectToFile mapOfInitConditions fileName


// uncomment to read the data from disk
//let (Some mapOfTables) = readObjFromFile<Map<double, TableMissionMetrics[]>>   @"C:\Users\glddm\Documents\Duke\Research\OptimalAscent\Table9_9\initConditionsMap2.json"

//let getKeys (aMap:Map<'K, 'V>)  = 
//    aMap
//    |> Map.toSeq
//    |> Seq.map fst

//let depths = mapOfTables |> getKeys

let myCompleteMap = getMapOfDenseInitConditions()

let myMap = getMapOfDenseInitCondNoExp()



let tableMissionMetrics, ascentStrategy = getTables()

// find example mission
let maxDepth , bottomTime = 100.0, 50.0

let findIndex bt md = Array.indexed
                      >> Array.filter (fun (_, tmm) -> (tmm.MissionInfo.BottomTime = bt) && 
                                                       (tmm.MissionInfo.MaximumDepth = md)  )
                      >> Array.head
                      >> fst


let findIndexForGiveBtMD =  findIndex bottomTime maxDepth
let index = findIndexForGiveBtMD tableMissionMetrics

let initialConditions, depthProfiles = getTableOfInitialConditions table9FileName


let initNode = initialConditions.[index].InitAscentNode
let completeStrategy = depthProfiles.[index]
                        |> Seq.toArray

open ModelRunner

let solution = runModelOnProfileUsingFirstDepthAsInitNode  completeStrategy

solution |> Seq.toArray

//let solution = runModelOnProfile 

// small script to find mission index for the given mission 

let actualDepth = 55.0

let denseInitConditions= getMapOfDenseInitConditions()
let initCondition = denseInitConditions(actualDepth)

let indexOnDense = findIndexForGiveBtMD initCondition

let waitingTimes = [104.5, 145.0, 233.5, 1054.0]

let twentyMap  = myMap(20.0)


let getTensionsValue (x:float[]) = x


let refPressures = [|1.43635301 ;2.30398171; 1.05098322|]
twentyMap
|> Array.minBy ( fun tm -> let tensions = tm.InitAscentNode.TissueTensions
                                            |>  getTensionsValue
                           Array.map2 (fun x y -> (x-y)**2.0) tensions refPressures)

let aMap = myMap(300.0)
           |> Seq.maxBy (fun x -> let ant = x.InitAscentNode.TissueTensions
                                  ant |> Seq.map (fun ( x ) -> x ) |> Seq.max )


let allData = [|20.0 .. 5.0 .. 300.0|]
                |> Seq.map myMap
                |> Seq.concat
                |> Seq.toArray

let minRisk = allData
              |> Seq.minBy (fun x -> x.TotalRisk)

let maxRisk = allData
             |> Seq.maxBy ( fun x -> x.TotalRisk)



// conversion old to new format 

//type TableMissionMetrics2 = 
// TableMissionMetrics
let tables = getTables()
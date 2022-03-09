[<AutoOpen>]
module MissionSerializer

open Newtonsoft.Json
open System.IO

type SurfacePressureData = { PressureGrid: float[][]
                             RiskEstimate: float[][]}


let dumpObjectToFile inputObject outputFile =
    inputObject
    |>JsonConvert.SerializeObject 
    |>(fun content -> File.WriteAllText(outputFile, content) )


let readObjFromFile<'T> inputFile =  // it assumes the deserialization works
        
        if File.Exists inputFile
            then inputFile
                  |> File.ReadAllText
                  |>  JsonConvert.DeserializeObject<'T>
                  |> Some       
        else
            printfn "file not found"
            None

let tryReadTableMissionsMetricsFromFile = readObjFromFile<TableMissionMetrics[]>
let tryReadTableStrategiesFromFile = readObjFromFile<float[][]>


let getPressureGridFromDisk  = 
    readObjFromFile<SurfacePressureData> 
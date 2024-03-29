﻿[<AutoOpen>]
module TableDataInputs
open System


let ascentRate = -30.0 // FPM
let descentRate = 75.0 // FPM
let defaultAscentStep = -10.0 // decrement in depth in the ascending part of the table

let deltaDepthForRefinedNodeMapping = 5.0
let minDepthForStop = 20.0

let docFolder = Environment.SpecialFolder.MyDocuments 
                |> Environment.GetFolderPath


let dataSrcFolder =  docFolder + @"\Duke\Research\OptimalAscent\Table9_9"

let table9FileName = "table9_9_air.txt"

// used for serialization/deserializaiton
let tableInitConditionsFile = dataSrcFolder + @"\tableInitConditionsNew.json"
let tableInitCondNoExpFile = dataSrcFolder + @"tableInitCondNoExc.json"


let tableStrategiesFile = dataSrcFolder + @"\tableStrategis.json"
let tableStrategiesNoExpFile = dataSrcFolder + @"tableStrateNoExc.jsob"

let mapOfInitialConditionsFile = dataSrcFolder + @"\initConditionsMap2.json"  // @"\initConditionsMap.json"

let mapOfInitCondNoExpFile = dataSrcFolder + @"\initConditionsMapNoExcept.json"

let surfacePressureFileName = ( dataSrcFolder + @"\surfacePressureData.txt" )

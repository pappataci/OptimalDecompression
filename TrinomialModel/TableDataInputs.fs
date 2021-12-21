[<AutoOpen>]
module TableDataInputs

let ascentRate = -30.0 // FPM
let descentRate = 75.0 // FPM
let defaultAscentStep = -10.0 // decrement in depth in the ascending part of the table

let dataSrcFolder = @"C:\Users\glddm\Desktop\Table9_9"
let table9FileName = "table9_9_air.txt"

// used for serialization/deserializaiton
let tableInitConditionsFile = dataSrcFolder + @"\tableInitConditions.json"
let tableStrategiesFile = dataSrcFolder + @"\tableStrategis.json"
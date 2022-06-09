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


// Script for testing surface surrogate functionality

open TrinomialModToPython.ToPython
open ModelRunner

let zeroDepthSurrogate  = surfacePressureFileName
                           |> createRunningUntilZeroRiskSurrogateFromDisk

let tables, _  = getTables()

let ascentNodeExample = tables.[0].InitAscentNode

let nextTime, nextDepth = abs(ascentNodeExample.EnvInfo.Depth/ ascentRate) , 0.0

// this works for surface model
let nodeOut, nextState = stepFcnSurrogateResidual(ascentNodeExample, nextTime, nextDepth, 0.01)


let nodeWithDiscreteSolution = seq{{Time = nextTime; Depth = nextDepth}}
                               |> runModelOnProfile  ascentNodeExample  
                               |> Seq.last
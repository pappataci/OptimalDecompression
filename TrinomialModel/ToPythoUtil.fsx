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

//let tableInitConditions, tableStrategies = getTables()


let initialConditions, depthProfiles = getTableOfInitialConditions table9FileName

let ascentProfiles = Array.map2 getAscentProfileFromSingleDepthProfile initialConditions  depthProfiles

//toVectorOfActionsGen (constantDepthFcn , ascentFcn) strategy



let timeScale  = 100.0

let actionScaled = fun _ -> timeScale

let constantDepthFcn { Time = previousTime ; Depth = depth}  { Time = nextTime} : seq<float> = 
    
    let timeDifference = nextTime - previousTime
    let scalingFactor = actionScaled depth
    let completeParts = floor(timeDifference/scalingFactor) |> round |> int 
    let remainder = (timeDifference % scalingFactor  ) / scalingFactor

    let remainderSequence =  match remainder > 0.0 with
                             | true -> seq{-remainder    }
                             | false -> Seq.empty


    seq{yield! Seq.init completeParts (fun _ -> -1.0)
        yield! remainderSequence}




let ascentFcn {Depth = initDepth } {Depth = targetDepth} = 
    seq {1.0 - targetDepth / initDepth}


let index = 325

ascentProfiles.[index] // test ascent profiles
|> Seq.toArray
|> printfn "%A"

let testResult = toVectorOfActionsGen (constantDepthFcn, ascentFcn) ascentProfiles.[index]

let forPastingToPython (x:float[]) =
    let r = fun (x:float)  -> System.Math.Round(x, 10)
    x
    |> Seq.map   (fun aNumber ->  aNumber |> r |> string  )  
    |> String.concat ", "

testResult
|>forPastingToPython

let actionsToDisk:seq<DepthTime> -> string= toVectorOfActionsGen (constantDepthFcn, ascentFcn)
                                              >> forPastingToPython


let actionsStringForFile = ascentProfiles
                           |> Array.Parallel.map actionsToDisk

System.IO.File.WriteAllLines(@"C:\Users\glddm\Desktop\Table9_9\actions.txt", actionsStringForFile)
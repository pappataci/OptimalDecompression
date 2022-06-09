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

open TrinomialModToPython.ToPython
open FSharp.Stats.Interpolation
open ModelRunner

let depth = 20.0

let initNode = createSurfInitNodeForTissueWithValueAtDepth 20.0 2 2.0

let createInfiniteSequenceAtConstantDepth depthLevel = 
    Seq.initInfinite (fun idx ->  {Time =  maxIntegrationTime * (double) idx ;  Depth = depthLevel} )


let integrateAtConstantDepthUntilZeroRisk  initNode =
    //let constantDepthStrategy = createInfiniteSequenceAtConstantDepth initNode.EnvInfo.Depth
    //runModelOnProfile
    initNode.EnvInfo.Depth
    |> createInfiniteSequenceAtConstantDepth
    |> Seq.scan  oneActionStepTransition initNode
    |>  Seq.takeWhile (fun node ->  acrrueingRiskAtDepth node.EnvInfo.Depth  node.TissueTensions)

// function for creating maps: tested and functional
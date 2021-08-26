// Learn more about F# at https://fsharp.org
// See the 'F# Tutorial' project for more help.
#load "SeqExtension.fs"
#load "Gas.fs"
#load "ELModelCommon.fs"
#load "TrinomialModel.fs"
#load "TableDataInputs.fs"
#load "TableReader.fs"


//let b x = x * 2.0

let defNodeWithTensionAtDepthAndTime initDepthTime = // needed more generic function with initRisk and pressures

    let {Depth = initDepth; Time = initTime} = initDepthTime
    let externalPressures = depth2AmbientCondition initDepth
    let zeroVector = Array.zeroCreate modelParams.Gains.Length
    {EnvInfo = initDepthTime
     Tensions = getTissueTensionsAtDepth externalPressures
     ExternalPressures = depth2AmbientCondition initDepth
     InstantaneousRisk = zeroVector
     AccruedWeightedRisk = zeroVector
     IntegratedRisk = zeroVector
     IntegratedWeightedRisk = zeroVector
     TotalRisk = 0.0}


let initNodeAtSurface = defNodeWithTensionAtDepthAndTime {Depth = 0.0; Time = 0.0}

let thirtyNode = oneActionStepTransition initNodeAtSurface {Time = 0.4; Depth =  30.0}

let three71Node = oneActionStepTransition thirtyNode {Time = 371.0; Depth =  30.0}

let three72Node = oneActionStepTransition three71Node {Time = 372.0; Depth =  0.0}

let myFinalNode = runModelUntilZeroRisk three72Node

let finalNode = oneActionStepTransition three72Node {Time = 1812.0; Depth =  0.0}

let computedNodeSeq , checkedDiveLength = fileName
                                            |> getDataContent
                                            |> Array.map data2SequenceOfDepthAndTime
                                            |> Array.unzip

//module  Params = 
//    let mutable A = 1.0


//module Mammolo = 
//    let f x = Params.A * x 

//module ChangeParams  =
//    let changeParams x = 
//        Params.A <- x 

// OK THis is interesting for changing the parameters

let pSeriousDCS node = 1.0 - exp(-trinomialScaleFactor * node.TotalRisk)
let pMildDCS node = (1.0 - exp(-node.TotalRisk)) * (1.0 - pSeriousDCS node)
let pNoDCSEvent node = exp( -(trinomialScaleFactor + 1.0) * node.TotalRisk)
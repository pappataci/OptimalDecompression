// Learn more about F# at https://fsharp.org
// See the 'F# Tutorial' project for more help.
#load "Gas.fs"
#load "ELModelCommon.fs"
#load "TrinomialModel.fs"


let defineInitNodeAtDepth initDepthTime = // RISK TO BE ADDED
    
    let {Depth = initDepth; Time = initTime} = initDepthTime
    let externalPressures = depth2AmbientCondition initDepth
    let zeroVector = Array.zeroCreate modelParams.Gains.Length
    {EnvInfo = initDepthTime
     Tensions = getTissueTensionsAtDepth externalPressures
     ExternalPressures = depth2AmbientCondition initDepth
     InstantaneousRisk = zeroVector
     AccruedRisk = zeroVector
     TotalRisk = 0.0}

let initNodAtSurface = defineInitNodeAtDepth {Depth = 0.0; Time = 0.0}

let thirtyNode = getEvolutionDynamicsOnTrajectory initNodAtSurface {Time = 0.4; Depth =  30.0}

let three71Node = getEvolutionDynamicsOnTrajectory thirtyNode {Time = 371.0; Depth =  30.0}

let three72Node = getEvolutionDynamicsOnTrajectory three71Node {Time = 372.0; Depth =  0.0}

let finalNode = getEvolutionDynamicsOnTrajectory three72Node {Time = 1812.0; Depth =  0.0}

//module  Params = 
//    let mutable A = 1.0


//module Mammolo = 
//    let f x = Params.A * x 

//module ChangeParams  =
//    let changeParams x = 
//        Params.A <- x 

// OK THis is interesting for changing the parameters
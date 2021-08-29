//let defineInitNodeAtDepth initDepthTime = // RISK TO BE ADDED
//let a = 1.0

//let b x = x * 2.0

let defNodeWithTensionAtDepthAndTime initDepthTime = // RISK TO BE ADDED
    
    let {Depth = initDepth; Time = initTime} = initDepthTime
    let externalPressures = depth2AmbientCondition initDepth
    let zeroVector = Array.zeroCreate modelParams.Gains.Length
    {EnvInfo = initDepthTime
     TissueTensions = getTissueTensionsAtDepth externalPressures
     ExternalPressures = depth2AmbientCondition initDepth
     InstantaneousRisk = zeroVector
     AccruedWeightedRisk = zeroVector
     IntegratedRisk = zeroVector
     IntegratedWeightedRisk = zeroVector
     TotalRisk = 0.0}


//    let {Depth = initDepth; Time = initTime} = initDepthTime
//    let externalPressures = depth2AmbientCondition initDepth
//    let zeroVector = Array.zeroCreate modelParams.Gains.Length
//    {EnvInfo = initDepthTime
//     Tensions = getTissueTensionsAtDepth externalPressures
//     ExternalPressures = depth2AmbientCondition initDepth
//     InstantaneousRisk = zeroVector
//     AccruedRisk = zeroVector
//     TotalRisk = 0.0}

[<EntryPoint>]
let main _ =
    
    let initNodAtSurface = defNodeWithTensionAtDepthAndTime {Depth = 0.0; Time = 0.0}
    
    let thirtyNode = oneActionStepTransition initNodAtSurface {Time = 0.4; Depth =  30.0}
    
    let three71Node = oneActionStepTransition thirtyNode {Time = 371.0; Depth =  30.0}
    
    let three72Node = oneActionStepTransition three71Node {Time = 372.0; Depth =  0.0}


    0 // return an integer exit code

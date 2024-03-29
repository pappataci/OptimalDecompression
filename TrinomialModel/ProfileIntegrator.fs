﻿namespace ModelRunner

[<AutoOpen>]
module ProfileIntegrator =
    
    let runModelUntilSurface initialNode sequenceDepthTime  =
        sequenceDepthTime
        |> Seq.scan  oneActionStepTransition initialNode

    let runModelStartingFromInitWithSeqNodes (surfaceRiskEstimator:Node->Node) initialNode sequenceDepthTime = 
        let internalNodes = runModelUntilSurface initialNode sequenceDepthTime
        let lastNode = internalNodes |> Seq.last
        
        let finalNode = match isAtSurface lastNode.EnvInfo.Depth with
                        | true -> seq{surfaceRiskEstimator lastNode}
                        | false -> Seq.empty

        //let finalNode = surfaceRiskEstimator surfaceNode
        seq{yield! internalNodes
            yield! finalNode }

    let (runModelOnProfile: Node -> seq<DepthTime> -> seq<Node> )  = runModelStartingFromInitWithSeqNodes runModelUntilZeroRisk

    let runModelOnProfileUsingFirstDepthAsInitNode (sequenceDepthTime: seq<DepthTime>) =  // this won't work if sequenceDepthTime is the empty sequence
        
        let initialNode = sequenceDepthTime 
                       |> Seq.head
                       |> defNodeWithTensionAtDepthAndTime

        let remainingStrategy = sequenceDepthTime 
                                |> Seq.skip 1 
        
        let (remainingSolution:seq<Node>) = match remainingStrategy |> Seq.isEmpty with 
                                            | true -> Seq.empty
                                            | false -> remainingStrategy
                                                        |> runModelOnProfile initialNode
                                                        |> Seq.skip 1 // initialNode will be yielded anyway

        seq{ yield initialNode 
             yield! remainingSolution}
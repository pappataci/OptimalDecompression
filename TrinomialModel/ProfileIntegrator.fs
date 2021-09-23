namespace ModelRunner

[<AutoOpen>]
module ProfileIntegrator =

    
    let runModelUntilSurface initialNode sequenceDepthTime  =
        sequenceDepthTime
        |> Seq.scan  oneActionStepTransition initialNode

    let runModelStartingFromInitWithSeqNodes (surfaceRiskEstimator:Node->Node) initialNode sequenceDepthTime = 
        let internalNodes = runModelUntilSurface initialNode sequenceDepthTime
        let surfaceNode = internalNodes |> Seq.last 
        let finalNode = surfaceRiskEstimator surfaceNode
        seq{yield! internalNodes
            yield finalNode}

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

    //let runModelOnInternalNodes  sequenceDepthTime  =
    //    let initialNode =  sequenceDepthTime
    //                       |>Seq.head 
    //                       |> defNodeWithTensionAtDepthAndTime
    //    sequenceDepthTime
    //    |> Seq.scan  oneActionStepTransition initialNode

    //let private runModelOnProfileGen (surfaceRiskEstimator:Node->Node) seqDepthTime = 
    //    let internalNodes = runModelOnInternalNodes  seqDepthTime
    //    let surfaceNode = internalNodes |> Seq.last 
    //    let finalNode = surfaceRiskEstimator surfaceNode
    //    seq{yield! internalNodes
    //        yield finalNode}

    //let runModelOnProfile' :seq<DepthTime> -> seq<Node>  = runModelOnProfileGen runModelUntilZeroRisk


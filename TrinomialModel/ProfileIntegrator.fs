namespace ModelRunner

[<AutoOpen>]
module ProfileIntegrator =
    open TableReader 
    let runModelOnInternalNodes  sequenceOfNodes  =
        let initialNode =  sequenceOfNodes
                           |>Seq.head 
                           |> defNodeWithTensionAtDepthAndTime
        sequenceOfNodes
        |> Seq.scan  oneActionStepTransition initialNode
        |> Seq.skip 1

    let runModelOnProfileGen (surfaceRiskEstimator:Node->Node) seqOfNodes = 
        let internalNodes = runModelOnInternalNodes  seqOfNodes
        let surfaceNode = internalNodes |> Seq.last 
        let finalNode = surfaceRiskEstimator surfaceNode
        seq{yield! internalNodes
            yield finalNode}

    let runModelOnProfile :seq<DepthTime> -> seq<Node>  = runModelOnProfileGen runModelUntilZeroRisk
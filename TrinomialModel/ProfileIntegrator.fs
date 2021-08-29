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

    let runModelOnProfile  seqOfNodes = 
        let internalNodes = runModelOnInternalNodes  seqOfNodes
        let surfaceNode = internalNodes |> Seq.last 
        let finalNode = runModelUntilZeroRisk surfaceNode
        seq{yield! internalNodes
            yield finalNode}
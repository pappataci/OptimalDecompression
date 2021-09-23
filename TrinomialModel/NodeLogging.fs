namespace Logging

[<AutoOpen>]
module NodeLogging = 
    open Logger    

    let vec2String (vec: seq<'T>) = 
        vec
        |> Seq.map (fun x -> x.ToString()  + " ")
        |> Seq.reduce (+)


    let nodeToString (aNode:Node) = 
        "depth " + aNode.EnvInfo.Depth.ToString() + " " + 
        "time " + aNode.EnvInfo.Time.ToString() + " " + 
        "ascTime " + aNode.AscentTime.ToString()+ " " + 
        "accrWRisk " + vec2String aNode.AccruedWeightedRisk +  
        "ambientPress " + aNode.ExternalPressures.Ambient.ToString() + " " + 
        "N2 Press " + aNode.ExternalPressures.Nitrogen.ToString() + " " + 
        "InstRisk " + vec2String  aNode.InstantaneousRisk + 
        "IntegratedRisk " + vec2String  aNode.IntegratedRisk +
        "MaxDepth " + aNode.MaxDepth.ToString()  + " " + 
        "intWeightedRisk " + vec2String aNode.IntegratedWeightedRisk +
        "totalRisk " + aNode.TotalRisk.ToString() + " " +
        "tissue tensions " + vec2String aNode.TissueTensions 


    let solutionToString (solution:seq<Node>) = 
        solution
        |> Seq.map nodeToString

    let solutionToLog( solution : seq<Node> ) =
        solution
        |> solutionToString
        |> addToLogger
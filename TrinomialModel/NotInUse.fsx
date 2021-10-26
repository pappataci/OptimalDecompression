


//let problematicProfiles = tableInitialConditions 
//                            |> Array.indexed
//                            |> Array.filter (fun (i, x) -> x.IsNone)
//                            |> Array.map fst


//let pSeriousDCS node = 1.0 - exp(-trinomialScaleFactor * node.TotalRisk)
//let pMildDCS node = (1.0 - exp(-node.TotalRisk)) * (1.0 - pSeriousDCS node)
//let pNoDCSEvent node = exp( -(trinomialScaleFactor + 1.0) * node.TotalRisk)


//let curveGen = linPowerCurveGenerator decisionTime initialNode curveParams

//let strategyCurve = curveGen |> curveStrategyToString
//let outputStrategyFileName = @"C:\Users\glddm\Desktop\New folder\text.txt"
//strategyCurve
//|>  writeStringSeqToDisk   outputStrategyFileName

type DepthTime =  { Depth: double
                    Time: double }

let toVectorOfActions (strategy: seq<float> )  =
    
    let internalSeq = strategy   
                    |> Seq.pairwise
                    |> Seq.map (fun (previousDepth, actualDepth) -> match abs(previousDepth - actualDepth) < 1.0e-3 with
                                                                    | true -> 1.0
                                                                    | _ -> 0.0 )
    seq { yield 0.0
          yield! internalSeq}

let depths = [|0.0|]
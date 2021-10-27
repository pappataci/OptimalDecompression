


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



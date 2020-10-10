

let addSteadyStateValue increment (seqTimeDepth:seq<float*float>)  steadyStateValue = 
    let lastElementAtSteadyState = (seqTimeDepth 
                                    |> Seq.last 
                                    |> fst 
                                    |> (+) increment , steadyStateValue ) 
    Seq.append seqTimeDepth  (Seq.singleton  lastElementAtSteadyState  )

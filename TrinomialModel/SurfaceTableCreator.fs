
[<AutoOpen>]
module SurfaceTableCreator

let initPressures (tensions:TissueTension[][]) =
    seq { for x in 0 .. ( (tensions|>Array.length) - 1 )  do 
                                                            let actualTension = tensions.[x] |> Array.map (fun (Tension t) -> t )
                                                            yield actualTension.[0], actualTension.[1], actualTension.[2] }
    |> Seq.toArray
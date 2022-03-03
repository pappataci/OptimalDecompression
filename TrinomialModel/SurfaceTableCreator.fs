
[<AutoOpen>]
module SurfaceTableCreator

let initPressures (tensions:float[][]) =
    seq { for x in 0 .. ( (tensions|>Array.length) - 1 )  do 
                                                            let actualTension = tensions.[x]  
                                                            yield actualTension.[0], actualTension.[1], actualTension.[2] }
    |> Seq.toArray
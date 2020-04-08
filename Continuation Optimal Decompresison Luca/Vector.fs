module VectorOp

let diff (vector:float[]) = 
    vector
    |> Array.pairwise
    |> Array.Parallel.map (fun (x,y) ->  y - x ) 
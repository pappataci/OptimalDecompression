[<AutoOpen>]
module AsyncHelpers

let private createAsyncListOfFcns (fcn: 'P -> 'Q) (arrayOfInputs:'P[])  = 
    let singleComputation singleInput = fun() ->  fcn singleInput // explicitely declared for clarity
    arrayOfInputs
    |> Seq.map ( fun anInput ->  async{ return (singleComputation anInput)() })

let applyInParallelAndAsync (fcn:'P->'Q) (arrayOfInputs:'P[]) = 
    createAsyncListOfFcns fcn arrayOfInputs
    |> Async.Parallel
    |> Async.RunSynchronously
namespace Utilities
open System.Diagnostics

[<AutoOpen>]
module Diagnostics = 

    let timeThis (fcnToBeComputed:'I -> 'O) (inputToFcn: 'I)  =
        let watch = new Stopwatch()
        watch.Start()
        let result = fcnToBeComputed inputToFcn
        result , ( watch.ElapsedMilliseconds |> float |> (*) 1.0e-3) 

    let percentComparison (actual:double) (benchmark) =
        (actual - benchmark)/benchmark * 100.0
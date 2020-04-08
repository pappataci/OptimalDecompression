// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.
open System
open OptimalAscentLearning

let pressAnyKey() = Console.Read() |> ignore

[<EntryPoint>]
let main _ = 
    
    let (State a) = InitStateDefinition.initState
    Console.WriteLine(a)
    pressAnyKey()
    0 // return an integer exit code

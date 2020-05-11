// Learn more about F# at https://fsharp.org
// See the 'F# Tutorial' project for more help.

open Dependency
open System

[<EntryPoint>]
let main argv =
    Console.WriteLine(Calculate.outputExample)
    Console.Read()
    printfn "%A" argv
    0 // return an integer exit code

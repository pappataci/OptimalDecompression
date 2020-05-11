// Learn more about F# at https://fsharp.org
// See the 'F# Tutorial' project for more help.

open Mammolo
open System

[<EntryPoint>]
let main argv =
    Console.WriteLine(Calculate.outputExample)
    Console.Read() |> ignore
    printfn "%A" argv
    0 // return an integer exit code

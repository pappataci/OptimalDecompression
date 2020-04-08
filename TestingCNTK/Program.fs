// Learn more about F# at https://fsharp.org
// See the 'F# Tutorial' project for more help.
open CNTK
open System

[<EntryPoint>]
let main _ =
    let allDevices = DeviceDescriptor.AllDevices()
    printfn "%A" allDevices
    Console.Read() |> ignore
    0 // return an integer exit code

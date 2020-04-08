//TORCH.NET Example

open Python.Runtime
open System
open System
open Torch
open Numpy.Models
open Python.Runtime
open System.Linq
open System.Diagnostics

[<EntryPoint>]
let main argv =
    //Environment.SetEnvironmentVariable( "PYTHONHOME"  , @"C:\ProgramData\Anaconda3") 
    //Environment.SetEnvironmentVariable( "PYTHON"  , @"C:\ProgramData\Anaconda3\Lib") 
    //Utilities.IO.addToPath  @"C:\ProgramData\Anaconda3"
    //Utilities.IO.addToPath  @"C:\ProgramData\Anaconda3\python37.dll"

    //let folderName =   @"C:\ProgramData\Anaconda3"
    //let addToStringandPutSemicolomn addedString (originalString:string)   = originalString   + addedString + ";"
    //let pathVar = "PYTHON"
    //pathVar
    //|> Environment.GetEnvironmentVariable
    //|> addToStringandPutSemicolomn folderName
    //|> ( fun x -> Environment.SetEnvironmentVariable( pathVar  , x) )


    try
        Torch.torch.device("cpu") 
        |> ignore
        Console.Write("Hello")
    with
        | _  as ex -> printfn "%A" ex.Message
    Console.Read() |> ignore
    0 // return an integer exit code

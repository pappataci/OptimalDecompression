open System
open System.IO

printfn "Initialising..."
Environment.CurrentDirectory <- __SOURCE_DIRECTORY__
// invisibly run a command (paket.exe in this case)
let init paket =
    let psi = new System.Diagnostics.ProcessStartInfo(paket)
    psi.Arguments <- "init"
    psi.UseShellExecute <- false
    let p = System.Diagnostics.Process.Start(psi)
    p.WaitForExit()
    p.ExitCode

if not (File.Exists "paket.exe") then
    printfn "installing paket"
    let url = "http://fsprojects.github.io/Paket/stable"
    use wc = new Net.WebClient()
    let tmp = Path.GetTempFileName()
    let stable = wc.DownloadString(url)
    wc.DownloadFile(stable, tmp)
    File.Move(tmp,Path.GetFileName stable)
    printfn "paket installed"
    System.Threading.Thread.Sleep(100)
    printfn "initialising paket"
    init "paket.exe" |> ignore
    System.Threading.Thread.Sleep(200)
    printfn "paket initialised"
else
    printfn "paket already exists"

/// install.dependencies.fsx

open System.IO
printfn "Installing dependencies"
System.Environment.CurrentDirectory <- __SOURCE_DIRECTORY__
#r "paket.exe"

open Paket
let dependencies = Paket.Dependencies.Locate(__SOURCE_DIRECTORY__)

printfn "%s" dependencies.DependenciesFile

if not (File.Exists "packages/Newtonsoft.Json/lib/net40/Newtonsoft.Json.dll") then
    printfn "installing nuget depenencies"
    // either use the dependencies.Install to add dependencies in the paket.dependencies file
    //dependencies.Install true |> ignore
    // or install them by name
    // I remove the existing versions
    dependencies.Remove "FSharp.Data"
    dependencies.Remove "Newtonsoft.Json 8.0.3"
    // then add them (because I'm pedantic about the way the dependencies file looks)
    dependencies.Add "FSharp.Data"
    dependencies.Add "Newtonsoft.Json 8.0.3"
    printfn "nuget depenencies installed"
else
    printfn "nuget depenencies already exist"

printfn "Dependencies installed"

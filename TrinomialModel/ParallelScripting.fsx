#r @"C:\Users\glddm\.nuget\packages\newtonsoft.json\13.0.1\lib\net45\Newtonsoft.Json.dll"
#r @"C:\Users\glddm\.nuget\packages\fsharp.data\3.3.3\lib\net45\FSharp.Data.dll"
#r @"C:\Users\glddm\.nuget\packages\fsharp.stats\0.4.3\lib\netstandard2.0\FSharp.Stats.dll"


#load "SeqExtension.fs"
#load "Gas.fs"
#load "ELModelCommon.fs"
#load "TrinomialModel.fs"
#load "TableDataInputs.fs"
#load "TableReader.fs"
#load "ProfileIntegrator.fs"
#load "MissionDefinerFromTables.fs"
#load "MissionSerializer.fs"
#load "SurrogateModelCreation.fs"
#load "TableToDiscreteActionsSeq.fs"
#load "TrinomialModelToPython.fs"

open TrinomialModToPython.ToPython

open System.Diagnostics
open System


let runProc filename args startDir : seq<string> * seq<string> = 
    let timer = Stopwatch.StartNew()
    let procStartInfo = 
        ProcessStartInfo(
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            FileName = filename,
            Arguments = args
        )
    match startDir with | Some d -> procStartInfo.WorkingDirectory <- d | _ -> ()

    let outputs = System.Collections.Generic.List<string>()
    let errors = System.Collections.Generic.List<string>()
    let outputHandler f (_sender:obj) (args:DataReceivedEventArgs) = f args.Data
    use p = new Process(StartInfo = procStartInfo)
    p.OutputDataReceived.AddHandler(DataReceivedEventHandler (outputHandler outputs.Add))
    p.ErrorDataReceived.AddHandler(DataReceivedEventHandler (outputHandler errors.Add))
    let started = 
        try
            p.Start()
        with | ex ->
            ex.Data.Add("filename", filename)
            reraise()
    if not started then
        failwithf "Failed to start process %s" filename
    printfn "Started %s with pid %i" p.ProcessName p.Id
    p.BeginOutputReadLine()
    p.BeginErrorReadLine()
    p.WaitForExit()
    timer.Stop()
    printfn "Finished %s after %A milliseconds" filename timer.ElapsedMilliseconds
    let cleanOut l = l |> Seq.filter (fun o -> String.IsNullOrEmpty o |> not)
    cleanOut outputs,cleanOut errors

//let test _ =
//    Process.Start(@"C:\Users\glddm\Documents\Duke\Research\OptimalAscent\repos\PythonRLOptimalAscent\venv\Scripts\python.exe" , @"C:\Users\glddm\Desktop\test.py" ) 

//Array.Parallel.map test [|0;1|]

let filename = @"C:\Users\glddm\Documents\Duke\Research\OptimalAscent\repos\PythonRLOptimalAscent\venv\Scripts\python.exe"
let scriptName = "testParallelFromDotNet.py"
let startDir = Some  @"C:\Users\glddm\Documents\Duke\Research\OptimalAscent\repos\PythonRLOptimalAscent"

//runProc filename args  ( Some  startDir) 

let tables , _ = getTables()

open Newtonsoft.Json

//let descriptors = [|0 .. 5|]
//                 |> Array.map (fun idx -> tables.[idx] |> JsonConvert.SerializeObject )


let zeroVec = Array.create 3 0.0

let testObj =   {    EnvInfo  = {Depth  = 20.0; Time = 0.0}
                     MaxDepth  = 1.0
                     AscentTime = 0.0
                     TissueTensions = [|1.0 .. 3.0|]
                     ExternalPressures  = {Ambient = 1.55; Nitrogen = 0.71}   
                     IntegratedRisk = zeroVec
                     InstantaneousRisk = zeroVec
                     IntegratedWeightedRisk = zeroVec 
                     AccruedWeightedRisk = zeroVec 
                     TotalRisk = 0.0
                     } 

let missionInfo = { MaximumDepth = 30.0
                    BottomTime = 100.0
                    TotalAscentTime = 0.0}

let tmm : TableMissionMetrics = { MissionInfo = missionInfo
                                  TotalRisk = 0.0 
                                  InitAscentNode = testObj }     
let serializedObjs = tmm |> JsonConvert.SerializeObject

//let missionEx =  serializedObjs.[1]
//let trivialEx = JsonConvert.SerializeObject testObj 


let executePython arg = runProc filename (scriptName + " " +  arg ) startDir
executePython  serializedObjs

//// how to use currying for fcns to be passed to Python
//// IMPORTANT: in Python code, call it using Invoke
//let createStepFunctionExample(surfMod,decompMod) = 
//    match surfMod with
//    | Some x -> fun (x:double,y) -> x + y 
//    | None -> fun (x,y) -> x * y 

//let stepFunctionCreatorDummy(surfaceModel, decompressionStopsModel) =
//    let actualFunction = createStepFunctionExample (surfaceModel ,decompressionStopsModel)
//    new  System.Func<double*double,double> (actualFunction)


//descriptors
//|> Array.Parallel.map (fun arg -> runProc filename (args + " " + string(arg)) startDir )

//serializedObjs
//|> Array.Parallel.map executePython 



//executePython  trivialEx

//let riskBound = 0.1

//let riskyTables = tables
//                  |> Array.indexed
//                  |> Array.filter (fun (_, content)-> content.TotalRisk >= riskBound)
//                  |> Array.sortBy (fun (_, t) -> t.MissionInfo.MaximumDepth)

//let pSeriousDCS totalRisk = 1.0 - exp(-trinomialScaleFactor * totalRisk)
//let pMildDCS totalRisk = (1.0 - exp(-totalRisk)) * (1.0 - pSeriousDCS totalRisk)
//let pNoDCSEvent totalRisk = exp( -(trinomialScaleFactor + 1.0) * totalRisk)
//let pDCSEvent totalRisk = 1.0 - pNoDCSEvent totalRisk 

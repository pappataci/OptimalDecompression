#load "Learner.fs"
#load "ReinforcementLearning.fs"
#load "Gas.fs"
#load "PredefinedDescent.fs"
#load "LEModel.fs"
#load "OptimalAscentLearning.fs"
#load "IOUtilities.fs"
#r @"C:\Users\glddm\source\repos\DecompressionL20190920A\packages\FSharp.Data.3.3.3\lib\net45\FSharp.Data.dll"

 
open ReinforcementLearning
open Gas
open InitDescent
open LEModel
open OptimalAscentLearning
open FSharp.Data

let initDepth = 0.0
let descentParameters = {DescentRate = 60.0 ; MaximumDepth = 120.0; BottomTime = 30.0}

let discretizationTimeForLegs = 0.1 

let seqOfStates, seqOfNodes = ModelDefinition.model
                            |> getInitialStateWithTheseParams descentParameters
                               discretizationTimeForLegs initDepth

seqOfNodes |> Seq.last
seqOfStates |>Seq.last 

let externalPressures = 
    seqOfNodes 
    |>  Seq.map (fun (TemporalValue x) -> 
                        let ambPressure = x.Value  
                                          |> depthAmbientPressure 
                        let externalN2Pressure = externalN2Pressure true 0.21 ambPressure 
                        {|Ambient = ambPressure ; N2 =  externalN2Pressure|}  )

let larsExternalPressFile = CsvProvider<  @"C:\Users\glddm\Desktop\ExternalPressures.csv" >.GetSample()

let (larsN2 , larsAmbient)  = larsExternalPressFile.Rows
                              |> Seq.map (fun x-> (x.N2 , x.Ambient) )
                              |> Seq.toArray
                              |> Array.unzip


//let (|Odd|Even|) (num , aParam) = 
//    if ((num+aParam) % 2 = 0 ) then 
//        Even
//    else  Odd

//let testActivePattern aNum aParam= 
//    match (aNum,  aParam) with
//    | Odd -> printfn "Odd"
//    | Even -> printfn "Even"
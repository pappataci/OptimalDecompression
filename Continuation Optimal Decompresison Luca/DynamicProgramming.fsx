#r @"C:\Users\glddm\source\repos\DecompressionL20190920A\packages\Extreme.Numerics.7.0.15\lib\net46\Extreme.Numerics.dll"
#r @"C:\Users\glddm\source\repos\DecompressionL20190920A\packages\Microsoft.ML.Probabilistic.0.3.1912.403\lib\netstandard2.0\Microsoft.ML.Probabilistic.dll"
#r @"C:\Users\glddm\source\repos\DecompressionL20190920A\packages\FSharp.Data.3.3.3\lib\net45\FSharp.Data.dll"
#load "ReinforcementLearning.fs"
#load "PredefinedDescent.fs"
#load "Gas.fs"
#load "LEModel.fs"
#load "OptimalAscentLearning.fs"
#load "InputDefinition.fs"
#load "EnvironmentToPython.fs"
#load "SeqExtension.fs"
#load "AscentSimulator.fs"
#load "AscentBuilder.fs"
#load "OneLegStrategy.fs"
#load "Result2CSV.fs"

open FSharp.Data

open InitDescent
open LEModel
//open InputDefinition
//open System
//open Extreme.Mathematics
//open Extreme.Mathematics.Optimization
//open AscentSimulator
//open AscentBuilder

open OneLegStrategy

type PressureAtSurfaceToRisk = CsvProvider<"P1, P2, P3, ResRisk" , Schema= "float, float, float, float" , HasHeaders = true> 

let createTestingEnvironment (integrationTime, controlToIntegration)  = 
// these are fictitious values just to get out the environment: since the initial state is replaced 
    let  maxSimTime = 15000.0
    let maximumDepth = 120.0 // ft
    let bottomTime   = 4.0 // minutes
    let _, myEnv =  initStateAndEnvDescent maxSimTime  (integrationTime, controlToIntegration)   maximumDepth  bottomTime
    myEnv

// creating pressure to residual risk map for surface level
    // create inputs for pressures
    // run the model in parallel for all inputs
    // write the results to disk for processing with Matlab

// single inputs definition:
let p1 =[|0.8 ..0.2 .. 5.0|]
let p2 = [|0.8 .. 0.05 .. 2.25|]
let p3 = [|0.8 .. 0.025 .. 1.15|]

// create all inputs 3D (all combination):

let createMesh3D x1 x2 x3  = seq { for x1Value in x1 do 
                                       for x2Value in x2 do 
                                           for x3Value in x3 -> (x1Value, x2Value,   x3Value) } |> Seq.toArray 

let pressuresGrid = createMesh3D p1 p2 p3 

// create at surface cost (residual risk; time could be easily incorporated as well, but it is irrelevant)

let integrationTime, controlToIntegration = 0.1 , 1


// from the input we have to create a fictitious State<LEStatus>
let createstateNodeWithThesePressures (p1,p2,p3) = 
   let actualDepth = 0.0
   let ambientPressure = depth2AmbientPressure  actualDepth
   let tissueState          =  { TissueTensions       =     [|Tension p1; Tension p2; Tension p3|]  
                                 CurrentDepthAndTime  =  TemporalValue { Time = 0.0 ; Value =actualDepth  }                 } 
   let initialRiskInfo      =  { AccruedRisk = 0.0 ; IntegratedRisks =   [|0.0; 0.0 ; 0.0 |]   }
   {LEPhysics = tissueState ; Risk = initialRiskInfo }
   |> State 


let strategySimResults2Risk (nodeOfStates:seq<State<LEStatus>>) = 
    match (nodeOfStates |> Seq.isEmpty ) with 
    | true -> 0.0
    | false -> nodeOfStates
               |> Seq.last 
               |> leStatus2Risk


let getAccruedRiskForThesePressures environment tissueTensions   = 
    tissueTensions
    |> createstateNodeWithThesePressures
    |> (fun x ->   simulateStrategyUntilZeroRisk x  environment  )
    |> strategySimResults2Risk


let risk2PDCS risk = 
    (1.0 - exp(-risk))

let getPDCS environment tissueTensions = 
    tissueTensions
    |> getAccruedRiskForThesePressures environment 
    |> risk2PDCS

// create testing environment 
let environment = createTestingEnvironment (integrationTime, controlToIntegration) 

// compute residual risks for given tensions
let residualRiskMap = pressuresGrid
                      |> Array.mapi (fun i x -> printfn "%A" i   
                                                getAccruedRiskForThesePressures environment x )

// create array for output
let output = Seq.map2 (fun (p1,p2,p3) risk -> (p1,p2,p3, risk)) pressuresGrid residualRiskMap


let writeArrayToDisk fileName (finalSubFolder:option<string>)  results  = 
    
    let finalSubFolder = match finalSubFolder with
                         | None -> @"Documents\Duke\Research\OptimalAscent\NetResults\"
                         | Some v -> (v + @"\")
      
    let subFolder =  @"C:\Users\glddm\" + finalSubFolder 

    let table = new PressureAtSurfaceToRisk(Seq.map (fun x -> PressureAtSurfaceToRisk.Row(x)  ) results ) 

    table.Save (subFolder + fileName )

//writeArrayToDisk "RiskAtSurfaceMap.txt" None output
let ordered = Array.zip pressuresGrid [|0 .. ( (residualRiskMap |> Array.length) - 1 ) |]
let ordDiff (rp1,rp2,rp3)  = ordered |> Seq.map ( fun ((p1,p2,p3) , id) ->  (rp1-p1)**2.0 + (rp2-p2)**2.0+ (rp3 - p3)**2.0  , id  ) 

let getNearestFcnValue requestedPoint  =  requestedPoint 
                                              |>  ordDiff
                                              |>  Seq.minBy (fun (distance, id ) -> distance )
                                              |> snd 
                                              |> (fun idx -> pressuresGrid.[idx] ,  residualRiskMap.[idx])
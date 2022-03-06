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
#load "SurfaceSurrogateModelCreation.fs"
#load "TableToDiscreteActionsSeq.fs"
#load "TrinomialModelToPython.fs"


// uncomment to recreate the data
//open TrinomialModToPython.ToPython
//let pressureToRiskData =  getTables() |> fst
//                         |> createPressureToRiskData
//// uncomment with previuos to dump data to disk
//dumpObjectToFile pressureToRiskData surfacePressureFileName

open FSharp.Stats.Interpolation

// trying to save to disk and get it back


let zeroDepthSurrogate  = surfacePressureFileName
                           |> createRunningUntilZeroRiskSurrogateFromDisk

//uncomment to check surrogate correctness
//let surrogateVsModelRunner (n:Node) = 
//    n|> runModelUntilZeroRisk, n|> zeroDepthSurrogate

//let percentageRiskError (n1: Node, n2:Node) = 
//  abs ( n1.TotalRisk - n2.TotalRisk ) / n1.TotalRisk * 100.0

//let getErrorEstimateOnRisk = surrogateVsModelRunner
//                             >>  percentageRiskError

//let nodeExample = createSurfInitNodeForTissueWithValue 0 1.81
//nodeExample.TissueTensions.[1] <- 1.4
//nodeExample.TissueTensions.[2] <- 1.3

//nodeExample 
//|> getErrorEstimateOnRisk 
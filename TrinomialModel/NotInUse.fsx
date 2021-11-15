﻿


//let problematicProfiles = tableInitialConditions 
//                            |> Array.indexed
//                            |> Array.filter (fun (i, x) -> x.IsNone)
//                            |> Array.map fst


//let pSeriousDCS node = 1.0 - exp(-trinomialScaleFactor * node.TotalRisk)
//let pMildDCS node = (1.0 - exp(-node.TotalRisk)) * (1.0 - pSeriousDCS node)
//let pNoDCSEvent node = exp( -(trinomialScaleFactor + 1.0) * node.TotalRisk)


//let curveGen = linPowerCurveGenerator decisionTime initialNode curveParams

//let strategyCurve = curveGen |> curveStrategyToString
//let outputStrategyFileName = @"C:\Users\glddm\Desktop\New folder\text.txt"
//strategyCurve
//|>  writeStringSeqToDisk   outputStrategyFileName


#r @"C:\Users\glddm\source\repos\DecompressionL20190920A\packages\Extreme.Numerics.7.0.15\lib\net46\Extreme.Numerics.dll"
#load "Logger.fs"

#load "SeqExtension.fs"
#load "Gas.fs"
#load "ELModelCommon.fs"
#load "TrinomialModel.fs"
#load "TableDataInputs.fs"
#load "TableReader.fs"
#load "ProfileIntegrator.fs"
#load "MissionDefinerFromTables.fs"
#load "SurfaceTableCreator.fs"
#load "Diagnostics.fs"
#load "MinimalTimeSearcher.fs"

open ModelRunner
open MinimalSearcher
open Extreme.Mathematics
open Extreme.Mathematics.LinearAlgebra
open Logger

let tableInitialConditions = getTableOfInitialConditions table9FileName

let tableMissionMetrics, profileStrategy = tableInitialConditions
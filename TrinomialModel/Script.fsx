#r @"C:\Program Files (x86)\ILNumerics\ILNumerics Ultimate VS\bin\ILNumerics.Core.dll"
#r @"C:\Users\glddm\source\repos\DecompressionL20190920A\FuncApprox\bin\Debug\FuncApprox.dll"
#load "SeqExtension.fs"
#load "Gas.fs"
#load "ELModelCommon.fs"
#load "TrinomialModel.fs"
#load "TableDataInputs.fs"
#load "TableReader.fs"
#load "ProfileIntegrator.fs"
#load "MissionDefinerFromTables.fs"
#load "SurfaceTableCreator.fs"
//open ProfileIntegrator
open ModelRunner
open FuncApprox
let profilingOutput  = fileName
                                |> getDataContent
                                |> Array.map data2SequenceOfDepthAndTime


let solutions = profilingOutput |> Array.Parallel.map  ( fun( x,   _ )  -> runModelOnProfile x ) 

let tableInitialConditions = profilingOutput |> Array.Parallel.map getInitialConditionAndTargetForTable

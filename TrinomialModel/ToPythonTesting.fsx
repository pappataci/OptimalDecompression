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
#load "TrinomialModelToPython.fs"

open ModelRunner

open Extreme.Mathematics
open Extreme.Mathematics.LinearAlgebra
open Logger
open TrinomialModToPython

let depthTimetoDepth (depthTime:DepthTime) = 
    depthTime.Depth

let depthToAction (strategy: seq<float> )  =
    
    let internalSeq = strategy   
                    |> Seq.pairwise
                    |> Seq.map (fun (previousDepth, actualDepth) -> match abs(previousDepth - actualDepth) < 1.0e-3 with
                                                                    | true -> 1.0
                                                                    | _ -> 0.0 )
    seq { yield 0.0
          yield! internalSeq}
    |> Seq.toArray


let getTableOfInitialConditions tableFileName = 
    let seqDepthAndTimeFromTables , missionInfos =  tableFileName
                                                    |> getDataContent
                                                    |> Array.map data2SequenceOfDepthAndTime
                                                    |> Array.unzip

    let solutions = seqDepthAndTimeFromTables |> Array.Parallel.map  runModelOnProfileUsingFirstDepthAsInitNode
    Array.map2 getInitialConditionsFromSolution solutions missionInfos , seqDepthAndTimeFromTables


let tableInitialConditions , actualAscent = getTableOfInitialConditions table9FileName

actualAscent.[32]
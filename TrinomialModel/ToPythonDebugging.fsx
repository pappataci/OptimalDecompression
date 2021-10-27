(*
This script is intended to test that the actions created from the tables are 
equivalent to running the model through the tables, as a sequence of <DepthTime>
*)

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
open Logger
open TrinomialModToPython

//let tableStategiesAsActions = tableStrategies
   //                              |> Array.map ( Seq.map (depthTimetoDepth ) )
   //                              |> Array.map depthToAction


let initialConditions, depthProfiles = getTableOfInitialConditions table9FileName

let getAscentProfileFromSingleDepthProfile initialCondition depthProfile = 
    depthProfile

let getAscentProfilesFromDepthProfiles (initialConditions:TableMissionMetrics[]) (depthProfiles:seq<DepthTime> [])  = 
    Array.map2 getAscentProfileFromSingleDepthProfile initialConditions depthProfiles


let ascentProfiles = getAscentProfilesFromDepthProfiles initialConditions depthProfiles

let toVectorOfActions (strategy: seq<float> )  =
    
    let internalSeq = strategy   
                    |> Seq.pairwise
                    |> Seq.map (fun (previousDepth, actualDepth) -> match abs(previousDepth - actualDepth) < 1.0e-3 with
                                                                    | true -> 1.0
                                                                    | _ -> 0.0 )
    seq { yield 0.0
          yield! internalSeq}

let depths = [|0.0|]
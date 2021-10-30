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

let getAscentProfileFromSingleDepthProfile (initialCondition:TableMissionMetrics) (depthProfile:seq<DepthTime>) = 
    let timeTolerance = 1.0e-5
    depthProfile
    |> Seq.filter ( fun x -> x.Time >= initialCondition.MissionInfo.BottomTime - timeTolerance  )
    |> Seq.skip 1 // get rid of first node: this is the initial condition
    

let getAscentProfilesFromDepthProfiles (initialConditions:TableMissionMetrics[]) (depthProfiles:seq<DepthTime> [])  = 
    Array.map2 getAscentProfileFromSingleDepthProfile initialConditions depthProfiles


let ascentProfiles = getAscentProfilesFromDepthProfiles initialConditions depthProfiles



let vecIndex = 40
let initSeq = getAscentProfileFromSingleDepthProfile initialConditions.[vecIndex]  depthProfiles.[vecIndex]


let testInitSeq = initSeq |> Seq.pairwise

let getNumberOfActions (init:double) final =
    let decisionTime = 1.0
    (final - init) / decisionTime
    |> int

let getActionConstantDepth initTime (finalTime:float) =
    let numberOfActions = getNumberOfActions initTime finalTime
    Seq.init numberOfActions (fun _ -> 1.0)

let getInternalSeq strategy = strategy   
                                |> Seq.pairwise
                                |> Seq.map (fun (prev, actual)  ->  let isAlmostEqualTo (x:float) y = abs(x-y) < 1.0e-3
                                                                    if (prev.Depth |> isAlmostEqualTo actual.Depth ) then 
                                                                       getActionConstantDepth  prev.Time actual.Time 
                                                                    else
                                                                       seq{0.0} )
                                |> Seq.concat

let toVectorOfActions (strategy: seq<DepthTime> )  =   
    let internalSeq = getInternalSeq strategy
    seq { yield 0.0 // first action is always ascent to next level
          yield! internalSeq}
    |> Seq.toArray
    
let actions = initSeq 
                |> toVectorOfActions


printfn " INIT CONDITION %A"   initialConditions.[vecIndex].InitAscentNode
printfn " TABLE ASCENT %A"   ( depthProfiles.[vecIndex] |> Seq.toArray ) 
printfn " ACTIONS %A" actions
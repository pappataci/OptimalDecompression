#r @"C:\Users\glddm\source\repos\DecompressionL20190920A\packages\Extreme.Numerics.7.0.15\lib\net46\Extreme.Numerics.dll"


#load "SeqExtension.fs"
#load "Gas.fs"
#load "ELModelCommon.fs"
#load "TrinomialModel.fs"
#load "TableDataInputs.fs"
#load "TableReader.fs"
#load "ProfileIntegrator.fs"
#load "MissionDefinerFromTables.fs"
#load "SurfaceTableCreator.fs"

open ModelRunner

open Extreme.Mathematics
open Extreme.Mathematics.Optimization
open Extreme.Mathematics.LinearAlgebra
open Extreme.Mathematics.EquationSolvers

open System.IO
//let curveGen = linPowerCurveGenerator decisionTime initialNode curveParams


let curveText (curveGen:seq<DepthTime>) = 
    curveGen
    |> Seq.map (fun x -> x.Time.ToString() + ",  " + x.Depth.ToString())

let writeStringSeqToDisk fileName (stringSeq:seq<string>) = 
    File.WriteAllLines(fileName , stringSeq)

//let curveDescriptions = curveGen |> curveText

//open System.IO
//File.WriteAllLines(@"C:\Users\glddm\Desktop\New folder\text.txt" , curveDescriptions)




//getInitialConditionNode profileOut ascentParams.[0]

//let testProblematicOut = runModelOnProfile problematicProfile  
//module  Params = 
//    let mutable A = 1.0


//module Mammolo = 
//    let f x = Params.A * x 

//module ChangeParams  =
//    let changeParams x = 
//        Params.A <- x 

// OK THis is interesting for changing the parameter



// %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%% //



//let runOptimizationForThisTableEntry (tableEntry:TableMissionMetrics) 
//                                     (InitialGuessFcn initialGuessFcn)
//                                     (TrajGen trajectoryGenerator )  = 
    
//    let initialNode = tableEntry.InitAscentNode
//    let initialGuess = initialGuessFcn initialNode
    


//    0.0
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
#load "MinimalTimeSearcher.fs"
#load "NodeLogging.fs"

open ModelRunner
open MinimalSearcher
open Extreme.Mathematics
open Extreme.Mathematics.Optimization
open Extreme.Mathematics.LinearAlgebra
open Logger
open Logging

let profilingOutput  = table9FileName
                                |> getDataContent
                                |> Array.map data2SequenceOfDepthAndTime

let seqDepthAndTime , missionInfos = profilingOutput |> Array.unzip

let solutions = seqDepthAndTime |> Array.Parallel.map runModelOnProfileUsingFirstDepthAsInitNode

let ascentTimes = solutions
                  |> Array.Parallel.map (fun aSolution -> aSolution |> Seq.last |> (fun aNode -> aNode.AscentTime))
                  |> Array.zip [| 0 .. (solutions.Length - 1 )|]

let missionInfosAscentTimes = missionInfos
                                 |> Array.Parallel.map (fun missionInfo -> missionInfo.TotalAscentTime)
                                 |> Array.zip [| 0 .. (missionInfos.Length - 1 )|]
                                 

let differences = Array.map2 (fun (index , value ) (_, otherValue ) -> (index , abs(value-otherValue))) ascentTimes missionInfosAscentTimes

let indeces = differences
              |> Array.sortByDescending ( fun (index , value)  -> value)


let problIdx = 215

let problSol = solutions.[problIdx]

Logger.LoggerSettings.startLogger()
problSol
|> Logging.NodeLogging.solutionToLog

//let expectedMinusPredicted = Array.map2 (-)  missionInfosAscentTimes ascentTimes |> Array.map abs 
//let caseToBeTested = 105

//let givenMissionInfo = missionInfos.[caseToBeTested]

//let givenSeqDepthAndTime = seqDepthAndTime.[caseToBeTested]

//let givenSolution = givenSeqDepthAndTime |>   runModelOnProfileUsingFirstDepthAsInitNode  


//let initialCondition =  getInitialConditionsFromSolution givenSolution givenMissionInfo






//let initialNode = initialCondition.InitAscentNode


//let testCurve = linPowerCurveGenerator decisionTime initialNode  curveParams


//let testSolution = runModelOnProfile testCurve


// Debugging 

//let depthTimeSeq, missionInfo = profilingOutput.[caseToBeTested]
//depthTimeSeq |> Seq.toArray

//depthTimeSeq
//|> runModelOnProfile
//|> Seq.toArray

//Logger.LoggerSettings.addToLogger(seq{ "depth, updated" ; (maxDepth, hasBeenUpdated ).ToString() } )

//Logger.LoggerSettings.addToLogger(seq{ "initDepth, duration, updated" ; 
//actualNode.EnvInfo.Depth.ToString() + " " + deltaT.ToString() + " " + hasBeenUpdated.ToString()  })
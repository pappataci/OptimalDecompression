open System

#r @"C:\Users\glddm\source\repos\DecompressionL20190920A\packages\Microsoft.ML.Probabilistic.0.3.1912.403\lib\netstandard2.0\Microsoft.ML.Probabilistic.dll"
#r @"C:\Users\glddm\source\repos\DecompressionL20190920A\packages\FSharp.Collections.ParallelSeq.1.1.3\lib\net45\FSharp.Collections.ParallelSeq.dll"

#load "Learner.fs"
#load "ReinforcementLearning.fs"
#load "Gas.fs"
#load "PredefinedDescent.fs"
#load "LEModel.fs"
#load "OptimalAscentLearning.fs"
#load "IOUtilities.fs"
#load "InputDefinition.fs"
#load "EnvironmentToPython.fs"
#load "AsyncHelpers.fs"
#load "SeqExtension.fs"
#load "AscentSimulator.fs"

open ReinforcementLearning
open LEModel
open ToPython
open AscentSimulator

//let maxPDCS = 3.3e-2

let penaltyForExceedingRisk ,  rewardForDelivering , penaltyForExceedingTime= 1.0 , 1.0, 0.5
let integrationTime = 0.1 
let controlToIntegrationTimeRatio = 10
let descentRate = 60.0
let legDiscreteTime =  integrationTime
let maximumSimulationTime = 50001.0

// MISSION PARAMETERS
let maxPDCS =  4.e-2
let maximumDepth = 30.0
let bottomTime = 10.0

// Python equivalent helper function
let env, initState ,  ascentLimiter , nextAscLimit  =  getEnvInitStateAndAscentLimiter  ( maxPDCS    , maximumSimulationTime , 
                                                                           penaltyForExceedingRisk ,  rewardForDelivering , penaltyForExceedingTime , 
                                                                           integrationTime  ,
                                                                           controlToIntegrationTimeRatio,  
                                                                           descentRate , 
                                                                           maximumDepth  , 
                                                                           bottomTime  , 
                                                                           legDiscreteTime   )  

let answer = getNextEnvResponseAndBoundForNextAction(env, initState ,maximumDepth, ascentLimiter)

let (nextState, transitionRew, isTerminalState , ascentRateLimit)  = answer 

let getCompleteSeqAtConstantDepth (constantDepth:float)  =   
    let infinitSeqOfConstantValues = (fun _ -> constantDepth) |> Seq.initInfinite
    let  ascentSeq   =  infinitSeqOfConstantValues 
                        |> Seq.scan ( fun ( nextState, rew, isTerminal, _ )  depth -> getNextEnvResponseAndBoundForNextAction(env, nextState , depth , ascentLimiter)  ) (  initState, 0.0 , false, 0.0)  
                        |> Seq.takeWhile (fun (_ , _, isTerminalState, _) ->  not isTerminalState)
    let aState, aReward, isTerminal, aLimit  = (ascentSeq |> Seq.last )
    let lastNode = seq { getNextEnvResponseAndBoundForNextAction(env, aState , constantDepth, ascentLimiter )  } 
    lastNode 
    |> Seq.append  ascentSeq
    |> Seq.toArray

let seqOfNodes = 5.0
                 |> getCompleteSeqAtConstantDepth  

let stopWatch = System.Diagnostics.Stopwatch.StartNew()
let testParallel = [|5.0; 3.0; 2.0 ; 7.0|]
                   |> Array.map getCompleteSeqAtConstantDepth
let dur' = stopWatch.Elapsed.TotalMilliseconds

let one() = 1.0 |> getCompleteSeqAtConstantDepth
let tw() = 2.0 |> getCompleteSeqAtConstantDepth

let testMe = [ async{return one()  } ; async{return  tw()  } ] 

let a = testMe
        |> Async.Parallel
        |> Async.RunSynchronously



//let stopWatch = System.Diagnostics.Stopwatch.StartNew()
//let out =  applyInParallelAndAsync getCompleteSeqAtConstantDepth   [|5.0; 3.0; 2.0 ; 7.0|]
//let dur = stopWatch.Elapsed.TotalMilliseconds

let lastNode = seqOfNodes |> Seq.last 
seqOfNodes |> Seq.length

let riskToPDCS aRisk = 
    ( 1.0 - exp( -aRisk))

let leToPDCS (State aNode)  =
    aNode.Risk.AccruedRisk
    |> riskToPDCS 

/// THIS IS A TEST
//(fun _ -> 1.0)
//|> Seq.initInfinite
//|> Seq.scan (fun sum value -> sum +  value) 0.0


//perturbState



//let a = 0.0
//        |> Array.create 150 
//        |> Array.scan ( fun (actualState, _)  actualDepth -> getNextEnvResponseAndBoundForNextAction(env, actualState , actualDepth , ascentLimiter) 
//                                                             |> (fun (x,_,_, z) -> (x , z)   ) ) (initState, ascentRateLimit) 

        

//System.IO.File.WriteAllLines( Environment.SpecialFolder.Desktop, randNum)
//let desktopPath = Environment.GetFolderPath Environment.SpecialFolder.Desktop

//System.IO.File.WriteAllLines(desktopPath  + @"\outHist.txt" , randNum)

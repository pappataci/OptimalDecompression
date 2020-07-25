open System

#r @"C:\Users\glddm\source\repos\DecompressionL20190920A\packages\Microsoft.ML.Probabilistic.0.3.1912.403\lib\netstandard2.0\Microsoft.ML.Probabilistic.dll"
#r @"C:\Users\glddm\source\repos\DecompressionL20190920A\packages\FSharp.Collections.ParallelSeq.1.1.3\lib\net45\FSharp.Collections.ParallelSeq.dll"

#r @"C:\Users\glddm\source\repos\DecompressionL20190920A\packages\Streams.0.6.0\lib\netstandard2.0\Streams.dll"

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

let test= async{ do! Async.Sleep(1500) 
                 printfn "executed"  }

let testArray = Array.init 10 (fun _ -> test)
testArray |> Async.Parallel |>Async.RunSynchronously

open Nessos.Streams

let commonSimulationParameters = {MaxPDCS = 0.32 ; MaxSimTime = 20000.0 ; PenaltyForExceedingRisk  = 1.0 ; RewardForDelivering = 10.0; PenaltyForExceedingTime = 0.5 ;
                                  IntegrationTime = 0.1; ControlToIntegrationTimeRatio = 10; DescentRate = 60.0; MaximumDepth = 20.0 ; BottomTime = 10.0;  LegDiscreteTime = 0.1}

let maxInputs =  8.0
let inputsStrategies =  [|0.0 .. maxInputs|] |> Array.map (fun x -> ( commonSimulationParameters , Seq.initInfinite (fun _ -> x )  |> Ascent ) |> StrategyInput )

let nessosResult = inputsStrategies
                    |> ParStream.ofArray
                    |> ParStream.map simulateStrategy
                    |> ParStream.toArray

type Agent<'T> = MailboxProcessor<'T>

type Message = | Message of obj

let echoAgent = 
    Agent<Message>.Start(
        fun inbox -> 
            let rec loop() = 
                async{
                    let! ( Message content) = inbox.Receive()
                    printfn "%O" content 
                    return! loop()
                }
            loop()
                )

type SimulationMessage = StrategyInput  * AsyncReplyChannel<StrategyOutput>

let agent  = Agent<SimulationMessage>.Start(fun inbox -> 
                                                let rec loop() = async{
                                                    let! msg, replyChannel = inbox.Receive()
                                                    msg
                                                    |> simulateStrategy
                                                    |> replyChannel.Reply

                                                    return! loop() } 
                                                loop() )

defaultSimulationParameters
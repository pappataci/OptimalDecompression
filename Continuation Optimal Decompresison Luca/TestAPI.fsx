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

//open ReinforcementLearning
open LEModel
open ToPython
open AscentSimulator

let maxPDCS = infinity // not stringent bound 
let maximumSimulationTime = 10000.0
let penaltyForExceedingRisk = 1.0
let rewardForDelivering = 10.0
let penaltyForExceedingTime = 0.5
let integrationTime = 0.1
let controlToIntegrationTimeRatio = 10
let descentRate = 60.0
let maximumDepth = 30.0
let legDiscreteTime = 0.1

let commonSimulationParameters = {MaxPDCS = maxPDCS ; MaxSimTime = maximumSimulationTime ; PenaltyForExceedingRisk  = penaltyForExceedingRisk ; RewardForDelivering = rewardForDelivering; PenaltyForExceedingTime =penaltyForExceedingTime ;
                                  IntegrationTime = integrationTime; ControlToIntegrationTimeRatio = controlToIntegrationTimeRatio; DescentRate = descentRate; MaximumDepth = maximumDepth ; BottomTime = 15.0;  LegDiscreteTime = legDiscreteTime}

let inputStrategy =  0.0 |>   (fun x -> ( commonSimulationParameters , Seq.initInfinite (fun _ -> x )  |> Ascent ) |> StrategyInput )

let (Output history, _ ) = simulateStrategy inputStrategy

let setBottomTimeMaximumTimeAndMaxDepth maxDepth  bottomTime maxLength  = { commonSimulationParameters with BottomTime = bottomTime ; MaxSimTime = bottomTime + maxLength ; MaximumDepth = maxDepth }



let env, initState ,  ascentLimiter , _  =  getEnvInitStateAndAscentLimiter  ( maxPDCS    , maximumSimulationTime , 
                                                                        penaltyForExceedingRisk ,  rewardForDelivering , penaltyForExceedingTime , 
                                                                        integrationTime  ,
                                                                        controlToIntegrationTimeRatio,  
                                                                        descentRate , 
                                                                        maximumDepth  , 
                                                                        4000.0  , 
                                                                        legDiscreteTime   ) 
printfn "%A" initState
let findMinBottomSaturationTime seqOfTimes pressValue =   seqOfTimes 
                                                        |> SeqExtension.takeWhileWithLast  ( fun  bt ->
                                                                                                let env, initState ,  ascentLimiter , _  =  getEnvInitStateAndAscentLimiter  ( maxPDCS    , maximumSimulationTime , 
                                                                                                                                                penaltyForExceedingRisk ,  rewardForDelivering , 
                                                                                                                                                penaltyForExceedingTime , integrationTime  ,
                                                                                                                                                controlToIntegrationTimeRatio,     descentRate , 
                                                                                                                                                maximumDepth  ,   bt  ,   legDiscreteTime   )  
                                                                                                let tensions = leStatus2TissueTension initState
                                                                                                let minTension =  tensions |> Array.min
                                                                                
                                                                                                minTension < pressValue)  
                                                        |> Seq.toArray |> Seq.last 

let bottomTimes = Array.append  ( Array.append [|10.0 ..10.0 ..100.0 |] [| 100.0 .. 100.0 .. 900.0 |] ) [| 1000.0 .. 500.0 .. 4000.0|]

//let inputStrategyWithThisBottomTime bottomTime  = 0.0 |>   (fun x -> ( setBottomTime bottomTime , Seq.initInfinite (fun _ -> x )  |> Ascent ) |> StrategyInput )


let maxAscentCreator actualDepth indexOfAscent = 
    seq { yield! Seq.init (indexOfAscent  ) (fun _ -> actualDepth )
          yield! Seq.initInfinite (fun _ -> 0.0) 
        } 

//let (Output strategyOutput )  = 4000.0 
//                                |> inputStrategyWithThisBottomTime 
//                                |> simulateStrategy


let getHistoryOfResponse maxDepth bottomTime  indexOfAscent = 
    let incrementWrtToBottomTime = 2000.0
    
    (incrementWrtToBottomTime
     |> setBottomTimeMaximumTimeAndMaxDepth maxDepth bottomTime  , 
     maxAscentCreator maxDepth indexOfAscent // ascentHistory
     |> Ascent  )
    |> StrategyInput
    |> simulateStrategy

let indecesOfAscent = [|0 .. 50 |]

let actualDepth = 30.0

let getStrategyOutputForThisBottomTime bottomTime =
    indecesOfAscent
    |> Array.map (getHistoryOfResponse actualDepth bottomTime)

let pathForSavingData = @"C:\Users\glddm\Desktop\"

let vector2String  aVec   = 
    let xprecision = 3
    let line = sprintf "%.*g"
    aVec
    |> Array.map  ( fun x -> ( line xprecision ) x)
    |> (fun x -> String.Join("," ,  x ) ) 
    //|> Array.fold (+) " " 


let writeDataToFileWithDepthTitle numericMaxDepth ( dataToBeWritten : string []) = 
    let fileName = pathForSavingData + "depth_"  + 
                          ( numericMaxDepth |> string )   + 
                           ".csv"
    using (new System.IO.StreamWriter  (fileName , append =  true ) )  ( fun writer -> Array.map  (fun (x:string) -> writer.Write ( x + " " )   )  dataToBeWritten |> ignore ; writer.WriteLine("") )  

let strategyToString actualDepth bottomTime indexOfAscent (Output response)  =   
    let  initState , _ , _ ,_  = response |> Seq.head 
    let initStateTissue = initState 
                              |> leStatus2TissueTension
                              |> vector2String
    initStateTissue
   
let outputForAllBottomTimes  = 
    bottomTimes
    |> Array.map (fun bottomTime -> indecesOfAscent 
                                             |> Array.map ( fun indexOfAscent  ->           
                                                                  getHistoryOfResponse actualDepth bottomTime indexOfAscent ))
                                                                  //|> strategyToString actualDepth bottomTime indexOfAscent )  )
                                                                  

//let (Output firstDatum) = outputForAllBottomTimes  // this index refers to bottomTimes
                          //|> Seq.item 1  

//let testState , _ , _ , _  = firstDatum |> Array.last

//let a' = outputForAllBottomTimes.[0] 
//            |> Seq.map (fun (Output x ) -> 
//                            let length  = x |> Array.length
//                            let finalRisk , rew,  isFinallyDone = x |> Array.last |> (fun (y, reward, isDone ,_ ) -> (leStatus2Risk y, reward, isDone) )
//                            (length, finalRisk , rew , isFinallyDone) )  |> Seq.toArray

//let getImmersionInitCondition maxDepth bottomTime targetInitAscentDepth = 
    
    //let maxPDCS , maxSimTime, integrationTime, controlToIntegration, descentRate , legDiscreteTime = 
    //    infinity, infinity  , 0.1            ,   10                , 60.0         , 0.1 
    //let  penaltyForExceedingRisk, rewardForDelivering, penaltyForExceedingTime = 10.0 , 10.0, 5.0 

    //let  environment ,  initState , ascentLimiter , nextAscentLimit = getEnvInitStateAndAscentLimiter(maxPDCS ,maxSimTime , penaltyForExceedingRisk, rewardForDelivering, penaltyForExceedingTime, 
    //                                                                    integrationTime, controlToIntegrationTimeRatio, descentRate,  maxDepth, bottomTime , legDiscreteTime )

    //environment,  initState |> resetTissueRiskAndTime , ascentLimiter , nextAscentLimit


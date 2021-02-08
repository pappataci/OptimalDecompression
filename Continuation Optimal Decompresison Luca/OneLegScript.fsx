#r @"C:\Users\glddm\source\repos\DecompressionL20190920A\packages\Extreme.Numerics.7.0.15\lib\net46\Extreme.Numerics.dll"
#r @"C:\Users\glddm\source\repos\DecompressionL20190920A\packages\Microsoft.ML.Probabilistic.0.3.1912.403\lib\netstandard2.0\Microsoft.ML.Probabilistic.dll"
#r @"C:\Users\glddm\source\repos\DecompressionL20190920A\packages\FSharp.Data.3.3.3\lib\net45\FSharp.Data.dll"
#load "ReinforcementLearning.fs"
#load "PredefinedDescent.fs"
#load "Gas.fs"
#load "LEModel.fs"
#load "OptimalAscentLearning.fs"
#load "InputDefinition.fs"
#load "EnvironmentToPython.fs"
#load "SeqExtension.fs"
#load "AscentSimulator.fs"
#load "AscentBuilder.fs"
#load "OneLegStrategy.fs"
#load "Result2CSV.fs"

//open ReinforcementLearning
//open InitDescent
//open LEModel
//open InputDefinition
//open System
//open Extreme.Mathematics
//open Extreme.Mathematics.Optimization
//open AscentSimulator
//open AscentBuilder

open OneLegStrategy

 // input example

let integrationTime, controlToIntegration = 0.1 , 1
let controlTime = integrationTime * (float controlToIntegration)
let  maxSimTime = 15000.0

// inputs specific to mission 
let maximumDepth = 120.0 // ft
let bottomTime   = 30.0 // minutes
let maxPDCS = 3.3e-2

// small test
let leInitState, myEnv =  initStateAndEnvDescent maxSimTime  (integrationTime, controlToIntegration)   maximumDepth  bottomTime
  
let breakOut = 0.0
let linearSlope = -30.0
let tay , tanhInitDerivative = -0.97 , -30.0


//let ascentStrategyExample = createAscentTrajectory controlTime ( bottomTime, maximumDepth ) ( linearSlope, breakOut  , tay , tanhInitDerivative)


// idea is: precreate sequence of strategies and then ordere them according to final time; then see which one does not violate current risk bound

//1) define discrete vectors for free parameters : linearSlope, breakOut, tay; for now tanhInitDerivative is assumed to be equal to linearSlope
//1 bis) create all inputs
//2) create a seq.while function which is executed until current residual risk bound is respected
//3) spit out results to CSV 


//let linearSlopeStep = -5.0 // ft/min 
let breakoutStep , maxBreakOut = 0.1 , 0.99
let minTay , tayStep , maxTay = -0.9 , 0.1 , 0.0

let linearSlopeValues =   [-0.01; -0.02; -0.03;    -0.1; -0.2;  -1.0;  -3.0 ;  -5.01; -10.01; -15.01; -20.01; -25.01; -30.0] 
                          |> Seq.ofList

let breakoutValues = [ 0.0 .. breakoutStep .. maxBreakOut] |> Seq.ofList

let tayValues = [minTay .. tayStep .. maxTay ] @ [maxTay ] |> Seq.ofList

let actualStrategyInputs  = seq { for linearSlope in linearSlopeValues do 
                                        for breakOut in breakoutValues do 
                                            for tay in tayValues -> (linearSlope, breakOut,   tay, linearSlope) } |> Seq.toArray 

let createStrategiesForAllInputs controlTime ( bottomTime, maximumDepth )  actualStrategyInputs =
    actualStrategyInputs
    |>  Array.Parallel.map (createAscentSimpleTrajectory controlTime ( bottomTime, maximumDepth )  )


let getStrategyTime   (aStrategy:seq<float*float>)    =
    
    let initTime = aStrategy                
                   |> Seq.head
                   |> fst

    let lastTime = aStrategy
                   |> Seq.last
                   |> fst

    lastTime - initTime + controlTime 

let allStrategies =  createStrategiesForAllInputs controlTime ( bottomTime, maximumDepth )  actualStrategyInputs

let strategiesLengths = allStrategies
                          |> Array.map Seq.length
  
let order = [| 0 .. ( ( strategiesLengths |> Array.length )  - 1 ) |]

let actualStrategiesWithLengths = Array.zip allStrategies strategiesLengths
                                  |> Array.zip order

let orderedStrategies = actualStrategiesWithLengths |> Array.sortBy ( fun  (_, ( _, length) ) ->  length )
                        
let numInputStrategy = orderedStrategies 
                       |> Seq.map (fun (idNum, (strat , _) ) -> (idNum, strat ) ) 



//getSeqOfDepthsForLinearAscentSection  (initTime:float , initDepth) (slope:float) (breakOut:float) controlTime 

//let computeSurfaceTimeForThisStrategy controlTime ( bottomTime, maximumDepth ) ( linearSlope, breakOut  , tay , tanhInitDerivative) =
    

//let createStrategiesForThisInitCondition controlTime (bottomTim, maximumDepth) (linearSlopes: seq<float> ,   breakoutValues: seq<float> , tayValues: seq<float> ) = 
    
//let stratResults = getTimeAndAccruedRiskForThisStrategy leInitState  ascentStrategyExample myEnv   

// Test For finding optimal strategy

let riskBoundIsViolated pDCS  (strategyResults : StrategyResults)  =
    let initResidualRisk = -log(1.0 - pDCS)
    (strategyResults.TotalRisk ) > initResidualRisk

let optStrat = numInputStrategy
              |> SeqExtension.takeWhileWithLast ( fun (idNum , strategy ) ->   strategy
                                                                              |> getTimeAndAccruedRiskForThisStrategy myEnv leInitState 
                                                                              |> riskBoundIsViolated maxPDCS ) 
   
let result = optStrat |> Seq.last 
//optStrat |> Seq.find ( )
    
//this part is only for testing 

//let offendingInput = actualStrategyInputs |> Seq.item 10 

//let offendingStrategy = allStrategies |> Seq.item 10 

//// check whether this is the offending strategy 
//offendingStrategy
//|> getTimeAndAccruedRiskForThisStrategy myEnv leInitState 

//let ascentStrategy = offendingStrategy |> Seq.map (snd >> Control )
//let upToSurfaceHistory  = computeUpToSurface leInitState ascentStrategy myEnv
//let initStateAtSurface  = upToSurfaceHistory |> Seq.last 
//open LEModel
//let upToZeroRiskHistory = simulateStrategyUntilZeroRisk initStateAtSurface  myEnv
//let initTimeAtSurface = initStateAtSurface |> leStatus2ModelTime
//let ascentTime = initTimeAtSurface - (leInitState |> leStatus2ModelTime)
//let ascentRisk = initStateAtSurface |> leStatus2Risk

//let totalRisk = ( upToZeroRiskHistory 
//                    |> Seq.last 
//                    |> leStatus2Risk ) 

//C:\Users\glddm\Documents\Duke\Research\OptimalAscent\NetResults

let result2OnlyDepth (idNum: int, seqOfTimeDepths:seq<float*float>) =
    seqOfTimeDepths
    |> Seq.map snd

result
|> result2OnlyDepth
|> writeArrayToDisk "Asc120_30_P3_3.txt" (Some @"\Documents\Duke\Research\OptimalAscent\NetResults") 

//result
#r @"C:\Users\glddm\source\repos\DecompressionL20190920A\packages\Extreme.Numerics.7.0.15\lib\net46\Extreme.Numerics.dll"
#r @"C:\Users\glddm\source\repos\DecompressionL20190920A\packages\Microsoft.ML.Probabilistic.0.3.1912.403\lib\netstandard2.0\Microsoft.ML.Probabilistic.dll"
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

open ReinforcementLearning
open InitDescent
open LEModel
open System
open Extreme.Mathematics
open Extreme.Mathematics.Optimization
open AscentSimulator
open AscentBuilder

let initStateAndEnvAfterAscent maxSimTime  (integrationTime, controlToIntegration)   maximumDepth  bottomTime  = 
    let strategyOutput, myEnv = getInitConditionAfterDescentPhase (integrationTime, controlToIntegration, integrationTime ) (Some maxSimTime) 1   maximumDepth  bottomTime maximumDepth 
    let (Output initialConditionAfterDescent) = strategyOutput 
    let leState = initialConditionAfterDescent
                  |> Array.head
                  |> ( fun (state , _ , _ , _)  -> state ) 
    leState , myEnv




 // input example

let integrationTime, controlToIntegration = 0.1 , 1
let controlTime = integrationTime * (float controlToIntegration)
let  maxSimTime = 3000.0

// inputs specific to mission 
let maximumDepth = 96.0 // ft
let bottomTime   = 40.0 // minutes
let maxPDCS = 3.3e-2

// small test
let leInitState, myEnv =  initStateAndEnvAfterAscent maxSimTime  (integrationTime, controlToIntegration)   maximumDepth  bottomTime

let getSeqOfDepthsForLinearAscentSection  (initTime:float , initDepth) (slope:float) (breakEvenCoeff:float) controlTime  = 
   // output is seq of depths 
   let targetDepth = breakEvenCoeff * initDepth
   let increment = controlTime * slope
   let depths = [|initDepth + increment .. increment .. targetDepth  |]
                |> Seq.ofArray

   let times = ( fun sampleNumber ->  initTime + (float(sampleNumber + 1 ))* controlTime)
               |> Seq.init  (depths|> Seq.length )  

   Seq.zip  times depths 
  
let breakEvenCoeff = 0.7
let initSlope = -10.0
let linearPartSeq = getSeqOfDepthsForLinearAscentSection  (0.0, maximumDepth)  initSlope breakEvenCoeff controlTime

let getArrayOfDepthsForTanhAscentSection controlTime initSlope  tay   (initTime:float, initDepth:float )  =
    //tay parameter belongs to (-1.0, 0.0]
    
    let b = tanh tay
    let a = initDepth / (tay + 1.0 )
    let k = initSlope / ( initDepth * ( tay - 1.0 ) )

    let tanhLeg t = 
        a * tanh ( -k * ( t - initTime ) + b ) + a ; 
    
    let times =  Seq.initInfinite ( fun sampleNumber ->  initTime + (float(sampleNumber + 1 ))* controlTime)
    let depths = times |> Seq.map tanhLeg
    
    let outputSequence = Seq.zip times depths 
    outputSequence
    

//let getOneLegAscent 

let tay = 0.0
 

// testing TanhSection
linearPartSeq
|> Seq.last
|> getArrayOfDepthsForTanhAscentSection controlTime initSlope tay
|> SeqExtension.takeWhileWithLast ( fun (_, depth ) -> depth >=  MissionConstraints.depthTolerance)
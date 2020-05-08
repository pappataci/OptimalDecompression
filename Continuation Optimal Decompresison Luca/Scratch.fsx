#load "Learner.fs"
#load "ReinforcementLearning.fs"
#load "Gas.fs"
#load "PredefinedDescent.fs"
#load "LEModel.fs"
#load "OptimalAscentLearning.fs"
#load "IOUtilities.fs"
#r @"C:\Users\glddm\source\repos\DecompressionL20190920A\packages\FSharp.Data.3.3.3\lib\net45\FSharp.Data.dll"

 
open ReinforcementLearning
open Gas
open InitDescent
open LEModel
open OptimalAscentLearning
open FSharp.Data
open System

let initDepth = 0.0
let descentParameters = {DescentRate = 60.0 ; MaximumDepth = 120.0; BottomTime = 30.0}

let discretizationTimeForLegs = 0.1 

////let seqOfStates, seqOfNodes = ModelDefinition.model
////                            |> getInitialStateWithTheseParams descentParameters
////                               discretizationTimeForLegs initDepth

////seqOfNodes |> Seq.last
////seqOfStates |>Seq.last 

//let myAmbient , myN2 = 
//    let (myAmbient' , myN2' ) = 
//        seqOfNodes 
//        |>  Seq.map (fun (TemporalValue x) -> 
//                            let ambPressure = x.Value  
//                                              |> depth2AmbientPressure 
//                            let externalN2Pressure = externalN2Pressure true 0.21 ambPressure 
//                            (ambPressure , externalN2Pressure ) )
//        |> Seq.toArray
//        |> Array.unzip
//    (myAmbient' |> Array.toSeq ,  myN2'  |> Array.toSeq)

//let larsExternalPressFile = CsvProvider<  @"C:\Users\glddm\Desktop\LarsResults.csv" , HasHeaders = false>.GetSample()

//let larsData  = larsExternalPressFile.Rows
//let larsN2 = larsData |>    Seq.map (fun x -> x.Column1 |> float )
//let larsAmbient = larsData |>  Seq.map (fun x -> x.Column2 |> float )   
//let larsTissues = larsData |> Seq.map (fun x -> [| x.Column3 |> float  ; x.Column4 |> float ; x.Column5 |> float |] )

//let seqOfTissueTensions = seqOfStates |> Seq.map leStatus2TissueTension |> Seq.skip 1  // skip initial state to confront with Lars'results

//let absValueDiff  (x:seq<float>) y = Seq.map2 (-) x y 
//                                     |> Seq.map abs

//let ambientDiff = absValueDiff larsAmbient myAmbient  
//                  |> Seq.max

//let n2Diff      = absValueDiff  larsN2 myN2
//                  |> Seq.max

//let printScreen message (variable:float) = 
//    message + " " + variable.ToString("E10")

//ambientDiff 
//|> printScreen "Maximum Error on Ambient "

//n2Diff 
//|> printScreen "Maxim Error on N2 Ambient"


//let myLEModelParams = USN93_EXP.getLEOptimalModelParamsSettingDeltaT 0.1
//let ambientPressure = depth2AmbientPressure 0.0
//let ambientCondition = depth2AmbientCondition myLEModelParams 0.0

//let initState = USN93_EXP.initStateFromInitDepth  myLEModelParams 0.0

//let getNewState actualState nextDepth =  giveNextStateForThisModelNDepthNTimeNode   ModelDefinition.model

//let tissueTensionDiff = Seq.map2 (fun x y ->  x|>
//                                              Array.map2 (-) y 
//                                              |> Array.map ( fun x -> x**2.0) 
//                                              |> Array.sum ) larsTissues seqOfTissueTensions
//                        |> Seq.map2 (fun y x  -> (x/ (y |> Seq.min)  ) * 100.0) larsTissues  
//                        |> Seq.max

//tissueTensionDiff
//|>  printScreen "Max norm error on Tissue Tensions"
  
//depth2N2Pressure  true 0.21  0.0
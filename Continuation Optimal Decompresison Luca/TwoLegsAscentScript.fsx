#r @"C:\Users\glddm\source\repos\DecompressionL20190920A\packages\Microsoft.ML.Probabilistic.0.3.1912.403\lib\netstandard2.0\Microsoft.ML.Probabilistic.dll"
#r @"C:\Users\glddm\source\repos\DecompressionL20190920A\packages\FSharp.Data.3.3.3\lib\net45\FSharp.Data.dll"

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
#load "TwoLegAscent.fs"
#load "Result2CSV.fs"

//open ReinforcementLearning
open InitDescent
open LEModel
open AscentSimulator


// SIMPLIFIED EXPERIMENT

let targetDepth' = 45.0
let seqBottomTimes' = seq{30.0 .. 30.0 .. 300.0}

let initConstDepth = targetDepth' - 5.0

let seqConstDepth' = seq{initConstDepth .. -5.0 .. max (initConstDepth - 25.0)   1.  }
let seqTimeAtConstDepth' = seq{ 1 .. 10 .. 200}
let seqAscentRates = seq{-30.0 .. 5.0 .. -5.0} 

let results , immmersionData , strategyOutput  =  solveThisAscentwithInitEndAscent seqBottomTimes' seqConstDepth' seqTimeAtConstDepth'  seqAscentRates (targetDepth': float )

let anOutput = strategyOutput
               |> Seq.head 

let (Output content ) = anOutput
content |> Array.take 4 

//let stepsFromMaxToTarget ,                         initialDepthParams                    ,              twoLegAscentParams                                 , maxAscentRate  = 
//              4          ,  {MaxDepth = 120.0 ; BottomTime = 50.0 ; TargetDepth = 70.0 } , { ConstantDepth  = 30.0   ;  TimeStepsAtConstantDepth  = 150   } ,    Some -30.0

//let   resultVector , immersionAnalytics   , simulationOutput     = solveThis2LegAscent stepsFromMaxToTarget initialDepthParams twoLegAscentParams maxAscentRate 


//let targetDepth = 60.0

//let seqStepsMaxTarget =   seq{1;2;3;4}
//let seqMaxDepth =   seq{90.0 .. 30.0 .. 210.0}
//let seqBottomTimes = seq{30.0 .. 30.0 .. 120.0}
//let seqConstDepth = seq{60.0 .. -15.0 .. 0.0}
//let seqTimeAtConstDepth = seq{ 1 .. 40 .. 300}
//let seqAscentRates = seq{-30.0 ;    -10.0 ; -5.0  }

//createInputs seqStepsMaxTarget seqMaxDepth seqBottomTimes  seqConstDepth seqTimeAtConstDepth seqAscentRates targetDepth
//|> (fun x -> printfn "%A" (x |> Seq.length))

//let results , immmersionData, strategyOutput = solveThisAscentForThisTargetDepth targetDepth  seqStepsMaxTarget seqMaxDepth seqBottomTimes  seqConstDepth seqTimeAtConstDepth seqAscentRates

//let grouppedResults = results
//                      |> results2Groups
//                      |> Seq.collect getRidOfSubOptimalSolutionsForThisGroup
//let optionTest = assignNoneIfIncreasingTime test |> Seq.toArray




//grouppedResults
//|> writeResultsToDisk "completeResults60.csv" None



//results
//|> myCsvBuildTable
//|> saveToCsv @"C:\Users\glddm\Desktop\TwoLegStudy\TwoLegResults30_MaxTarg.csv"

//let allGroups = groupped 
//                |> Seq.map getRidOfSubOptimalSolutionsForThisGroup

//let test = groupped |> Seq.item 1

//let   data  = groupped 
//              |> Seq.map   ( Array.map ( fun (risk, content ) -> content |> Array.minBy ( fun data -> data.FinalTime ) ) ) 
//              |> ( fun x -> seq{ for y  in x do 
//                                    for z in y  -> z  } )


//data
//|> myCsvBuildTable
//|> saveToCsv @"C:\Users\glddm\Desktop\TwoLegStudy\completeTableLower.csv"
                        //|> ( fun x -> let initTime = x.[0].FinalTime 
                        //              x 
                        //              |> Array.scan (fun (actualMinTime , keepIt , y )  result -> match result.FinalTime < actualMinTime with 
                        //                                                                          | true -> (result.FinalTime, true , y ) 
                        //                                                                          | false -> (actualMinTime , false , y ) ) (initTime , true, x.[0])   ) 
                        ////|> Array.filter (fun (_ , keepit , _ ) -> keepit ) 
                        ////|> Array.unzip3

                            //Array.scan (fun (actualMinTime , keepIt)  result ->  result   ) ( (fun x ) -> )
              
              //|> (fun 
//let getRidOf

//let out = groupped 
//          |> Seq.map ( fun x' -> seq {for x in (x' |> Array.length) do 
//                                         for y in (y |> Seq.length )  ->  }
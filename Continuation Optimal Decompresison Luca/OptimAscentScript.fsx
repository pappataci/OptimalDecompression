#load "Learner.fs"
#load "ReinforcementLearning.fs"
#load "Gas.fs"
#load "PredefinedDescent.fs"
#load "LEModel.fs"
#load "IOUtilities.fs"
#load "Vector.fs" // use to compute difference of vectors (so it is used for debugging)

open Gas
open InitDescent
open LEModel
open IOUtilities

let defaultModelParams =  USN93_EXP.fromConstants2ModelParams 
let zeroDepth = 0.0

let getNextState' = updateLEStatus defaultModelParams
let initState = (USN93_EXP.initStateFromInitDepth defaultModelParams zeroDepth)

let initDepth = 6.0 

let initState' = getNextState' initState initDepth

let getNextState = updateLEStatus ( USN93_EXP.setThalmanHypothesis true  defaultModelParams )

let pressureFile  =  @"C:\Users\glddm\Documents\Duke\Research\OptimalAscent\outputFromOptimalAscent\Ambient_pressure.csv" 

let depths = pressureFile
             |> pressures2DepthsFromFile

let getValue fcnGetter = 
    depths 
    |> Seq.scan getNextState initState'
    |> Seq.map fcnGetter

System.Environment.CurrentDirectory <- @"C:\Users\glddm\Documents\Duke\Research\OptimalAscent"

let tissueToArray (x:Tissue[])=
    x |> Array.map (fun (Tension x) -> x )

let tissueTensionsWithTime = (fun x ->   let tissueTension = x.LEPhysics.TissueTensions |> tissueToArray  
                                         let (TemporalValue y ) = x.LEPhysics.CurrentDepthAndTime 
                                         Array.concat [| [|y.Time|] ;  tissueTension |]    )
                              |>  getValue 
                              |> jaggedArray2Array2D
                      
//tissueTensionsWithTime |> writeMatrixToFileByRow "tensions.csv" ","

let risks = (fun x-> x.Risk.IntegratedRisks   )
            |> getValue
            //|> Seq.toArray
            //|> jaggedArray2Array2D
            //|> writeMatrixToFileByRow "risks.txt" ","

risks
|> Seq.map (fun x -> x |> Array.sum)
|> Seq.sortByDescending (fun x -> x )
|> Seq.sum

let analyticalIntRisks = 
    risks
    |> Seq.scan ( fun state value ->  Array.map2 (+) state value ) 
                (Array.zeroCreate<float> 3 )
    
let accruedRisk  =  (fun x -> x.Risk.AccruedRisk)
                    |> getValue
                    |> Seq.toArray
                     
//let getDepths = ( fun x -> x.LEPhysics.CurrentDepthAndTime |>  (fun (TemporalValue x) -> x.Value  ) )

//depths |> Seq.toArray |> writeArrayToFile "depths.csv"

let larsTensions = fileToMatrix @"C:\Users\glddm\Documents\Duke\Research\OptimalAscent\outputFromOptimalAscent\Tissue_tension.csv" 
let larsArray2dTension  = larsTensions |> Seq.ofArray |> jaggedArray2Array2D

let errorVsTime = 
    
    larsArray2dTension.[*,1..]
    |> Array2D.mapi ( fun i j x  -> x -  tissueTensionsWithTime.[i , j + 1 ]  ) 

errorVsTime |> writeMatrixToFileByRow "error.csv" ","


let differencesInTissues = 
    [|1..3|]
    |> Array.map ( fun index -> VectorOp.diff tissueTensionsWithTime.[*, index])

depths|> Seq.toArray |>  VectorOp.diff

// script to test my computations with Lars' code with the first leg (constant descent plus bottom)

//let testDefine = udpateUpdateLEStatus a 
//let modelTest = USN93_EXP.fromConstants2ModelParams
//                |>  defineModel udpateUpdateLEStatus 
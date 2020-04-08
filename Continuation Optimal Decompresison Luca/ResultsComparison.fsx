#load "Learner.fs"
#load "ReinforcementLearning.fs"
#load "Gas.fs"
#load "PredefinedDescent.fs"
#load "LEModel.fs"
#load "IOUtilities.fs"

open Gas
open InitDescent
open LEModel
open IOUtilities

let larsTensions = fileToMatrix @"C:\Users\glddm\Documents\Duke\Research\OptimalAscent\outputFromOptimalAscent\Tissue_tension.csv" 
let larsArray2dTension  = larsTensions |> Seq.ofArray |> jaggedArray2Array2D


// TO BE REWRITTEN IN TERMS OF DEPTHRATE
//let getValue fcnGetter = 
//    depths 
//    |> Seq.scan getNextState initState'
//    |> Seq.map fcnGetter

//let tissueToArray (x:Tissue[])=
//    x |> Array.map (fun (Tension x) -> x )

//let tissueTensionsWithTime = (fun x ->   let tissueTension = x.LEPhysics.TissueTensions |> tissueToArray  
//                                         let (TemporalValue y ) = x.LEPhysics.CurrentDepthAndTime 
//                                         Array.concat [| [|y.Time|] ;  tissueTension |]    )
//                              |>  getValue 
//                              |> jaggedArray2Array2D

//let errorVsTime = 
    
//    larsArray2dTension.[*,1..]
//    |> Array2D.mapi ( fun i j x  -> x -  tissueTensionsWithTime.[i , j + 1 ]  ) 

//errorVsTime |> writeMatrixToFileByRow "error.csv" ","
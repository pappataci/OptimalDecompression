#load "Learner.fs"
#load "ReinforcementLearning.fs"
#load "Gas.fs"
#load "PredefinedDescent.fs"
#load "LEModel.fs"
 
open ReinforcementLearning
open Gas
open InitDescent
open LEModel

//type LETest = { TissueTension : float[] }

//let multiplyTissueTensionBy  x  multiplier = 
//    x.TissueTension
//    |> Array.map (fun tension -> tension * multiplier)
//    |> (fun x -> {TissueTension = x})
    
//let testComputation2 = getMarchingCountedLazyComputation (fromValueFuncToStateFunc multiplyTissueTensionBy)
//                        {TissueTension = Array.create 3 1.2 } {1.0..infinity} 

//let testSeq = testComputation2
//              |> whileStatePredicateAndMaxIterations 100 ( fun x -> x.TissueTension
//                                                                    |> Array.min
//                                                                    |> (>) 200.0 ) 


let (|Odd|Even|) (num , aParam) = 
    if ((num+aParam) % 2 = 0 ) then 
        Even
    else  Odd

let testActivePattern aNum aParam= 
    match (aNum,  aParam) with
    | Odd -> printfn "Odd"
    | Even -> printfn "Even"

Array.create 1014748364  0.0
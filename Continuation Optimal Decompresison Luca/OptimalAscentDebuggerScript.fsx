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
#load "TwoLegAscent.fs"
#load "Result2CSV.fs"
#load "AscentBuilder.fs"
#load "AscentOptimizer.fs"

open LEModel
open Extreme.Mathematics
open AscentBuilder
open AscentOptimizer

let integrationTime = 0.1
let initTime = 120.0  // mins
let maxDepth = 60.0 // ft
let targetDepth = 0.0
let controlTime = integrationTime * 10.0
let initState = createFictitiouStateFromDepthTime (initTime, maxDepth) 


// DEBUGGING OBJECTIVE FUNCTION (CHECK)
let maxPDCS , maxSimTime = 0.05 , 50000.0
let controlToIntegration = 10 
let maximumDepth = 60.0 
let bottomTime = 120.0
let initAscentStateAndEnv = initStateAndEnvAfterAscent maxSimTime  (integrationTime, controlToIntegration)   maximumDepth  bottomTime
 
let costToGoApproximator = None 

// THIS HAS TO BE ABSTRACTED OUT (with automatic identification of number of params)
//let  objectiveFunction  = defineThreeLegObjectiveFunction initAscentStateAndEnv targetDepth controlTime  maxPDCS costToGoApproximator
let  objectiveFunction : System.Func<Vector<float>, float>  = defineOneStepObjFcn initAscentStateAndEnv targetDepth controlTime  maxPDCS costToGoApproximator


let perturb (x0:Vector<float>)  (dt:float)   i = 
    let t = Vector.Create<float>( x0|> Seq.length )
    t.[i] <- dt
    t

let gr x0 =  
    let dt = 0.01
    let refValue = objectiveFunction.Invoke(x0)
    [|0 ..  ( (x0|> Seq.length) - 1 )  |]
    |> Array.map ( fun idx -> let k = perturb x0 dt idx 
                              let perturbed = k + x0 
                              (objectiveFunction.Invoke(perturbed) - refValue) / dt )
                              |> Vector.Create :> Vector<float>




//let sgd x0 (eta:float) (epsilon:float)  maxIteration =
//    let rec desc x iteration = 
//        let g = gr x   

//        match g.Norm() < epsilon || (iteration > maxIteration )  with
//        | true -> 0.0
//        | false -> 1.0


//        if g.Norm() < epsilon then   x else  printfn "f , g %A , "  ( ( objectiveFunction.Invoke(x0) ) , ( x0|> Seq.toArray ) ); desc  ( x - eta * g )
//    desc x0 iteration + 1 



//let evaluateFcnBetweenMinMax increment fcn initValue (computationBound: float*float -> bool) = 
//    let generator x =  
//        let y = fcn x 
//        match ( computationBound(x,y) ) with 
//        | true  -> ( (x, y)   , x + increment  ) |> Some 
//        | false -> None

//let actual = Vector.Create([|-5.102223817; -19.98396963; -0.02151706647; 5.909616754; 26.0; -11.3107369;-0.3610388302; -3.281399114; 5.0|])

let actual = Vector.Create([|-25.102223817; 3.5 ; 0.0; 2.909616754; 12.0; -1.3107369;-0.3610388302; -3.281399114; 5.0|])


objectiveFunction.Invoke(actual)
//gr(actual) |> Seq.toArray


let sgd x0 (eta:float) (epsilon:float) maxIteration = 
    let optHistory  = (x0 , 0 ) 
                      |> Seq.unfold (fun (actualPoint , iteration)  ->  let gradientValue = (gr actualPoint) 
                                                                        match (gradientValue.Norm() > epsilon &&  iteration < maxIteration)  with 
                                                                        | true ->  let nextPoint = x0 - eta  * gradientValue  
                                                                                   ((nextPoint , iteration + 1), (nextPoint , iteration + 1) ) |> Some 
                                                                        | false ->  printfn "passed falses" ; None  )
    match optHistory |> Seq.isEmpty with 
    | true -> seq{(x0,0)}
    | false -> optHistory

////let out2 = sgd actual 0.01 0.00001 1
////           |> Seq.last
////           |> fst
////           |> Seq.toArray

////objectiveFunction.Invoke(actual)


//actual
//|> ascentWithOneStep initState targetDepth controlTime
//|> Seq.concat 
//|> Seq.map snd 
//|> Seq.toArray 
//|>  writeArrayToDisk "ascentDebug.csv" None 
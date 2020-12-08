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



//let evaluateFcnBetweenMinMax increment fcn initValue (computationBound: float*float -> bool) = 
//    let generator x =  
//        let y = fcn x 
//        match ( computationBound(x,y) ) with 
//        | true  -> ( (x, y)   , x + increment  ) |> Some 
//        | false -> None

//let actual = Vector.Create([|-5.102223817; -19.98396963; -0.02151706647; 5.909616754; 26.0; -11.3107369;-0.3610388302; -3.281399114; 5.0|])

let actual = Vector.Create([|-25.102223817; 3.5 ; 0.0; 2.909616754; 12.0; 
                             -1.3107369;-0.3610388302; -3.281399114 ; 5.0|])

objectiveFunction.Invoke(actual)


let sgd x0 (eta:float) (epsilon:float) maxIteration = 
    let optHistory  = (x0 , 0 ) 
                      |> Seq.unfold (fun (actualPoint , iteration)  ->  printfn "%A" (actualPoint |> Seq.toArray)
                                                                        let gradientValue = (gr actualPoint) 
                                                                        match (gradientValue.Norm() > epsilon &&  iteration < maxIteration)  with 
                                                                        | true ->  let nextPoint = x0 - eta  * gradientValue  
                                                                                   ((nextPoint , iteration + 1), (nextPoint , iteration + 1) ) |> Some 
                                                                        | false ->  None  )
    match optHistory |> Seq.isEmpty with 
    | true -> seq{(x0,0)}
    | false -> optHistory



let  out2= sgd actual 0.001 0.00001 10

//let last = out2
//           |> Seq.last
//           |> fst
//           |> Seq.toArray

////objectiveFunction.Invoke(actual)
let problInitCondition = Vector.Create( [|-25.10299173; -1.082971087; 0.00792394897; -1.924453258; 12.0; -8.051674144; -23.88947656; -13.25137446; 5.0|] )  :> Vector<float> 

problInitCondition
|> ascentWithOneStep initState targetDepth controlTime
|> Seq.concat 
|> Seq.map snd 
|> Seq.toArray 
|>  writeArrayToDisk "ascentDebug.csv" None 

// Try with constraints using library


let gradientForOpt (x:Vector<float> ) (y:Vector<float>) = 
    let y = gr x 
    y




//let nlp = new Extreme.Mathematics.Optimization.NonlinearProgram(objectiveFunction , Func<_,_,_>(gradientForOpt) )


let nlp = new Extreme.Mathematics.Optimization.NonlinearProgram()
nlp.ObjectiveFunction <- objectiveFunction
nlp.ObjectiveGradient <-  System.Func<_,_,_>(gradientForOpt)

nlp.AddLinearConstraint("s1u", [| 1.0; 0.0; 0.0 ; 0.0; 0.0 ; 0.0 ; 0.0 ; 0.0 ; 0.0   |] , Optimization.ConstraintType.LessThanOrEqual, -0.5 ) |> ignore
nlp.AddLinearConstraint("s1l", [| 1.0; 0.0; 0.0 ; 0.0; 0.0 ; 0.0 ; 0.0 ; 0.0 ; 0.0   |] , Optimization.ConstraintType.GreaterThanOrEqual, -30.0 ) |> ignore

nlp.AddLinearConstraint("t1l", [| 0.0; 0.0; 0.0 ; 0.0; 1.0 ; 0.0 ; 0.0 ; 0.0 ; 0.0   |] , Optimization.ConstraintType.GreaterThanOrEqual, 0.0 ) |> ignore

nlp.AddLinearConstraint("s2u", [| 0.0; 0.0; 0.0 ; 0.0; 0.0 ; 1.0 ; 0.0 ; 0.0 ; 0.0   |] , Optimization.ConstraintType.LessThanOrEqual, -0.5 ) |> ignore
nlp.AddLinearConstraint("s2l", [| 0.0; 0.0; 0.0 ; 0.0; 0.0 ; 1.0 ; 0.0 ; 0.0 ; 0.0   |] ,  Optimization.ConstraintType.GreaterThanOrEqual, -30.0 ) |> ignore

nlp.InitialGuess <- actual 

let solution = nlp.Solve()
nlp.ExtremumType <- Optimization.ExtremumType.Minimum

nlp.SolutionReport
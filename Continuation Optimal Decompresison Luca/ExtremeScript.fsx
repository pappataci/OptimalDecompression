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

open ReinforcementLearning
open InitDescent
open LEModel

open System
open Extreme.Mathematics
open Extreme.Mathematics.Optimization

open AscentSimulator
    
let maxPDCS , maxSimTime = 100.0 , 10000.0
let rlDummyParam = 0.0 
let integrationTime , controlToIntegration = 0.1 , 10

let maximumDepth , bottomTime = 60.0 , 120.0

let simParams = { MaxPDCS = maxPDCS ; MaxSimTime = maxSimTime; PenaltyForExceedingRisk = rlDummyParam;  RewardForDelivering = rlDummyParam; PenaltyForExceedingTime = rlDummyParam; 
                  IntegrationTime = integrationTime; ControlToIntegrationTimeRatio = controlToIntegration; DescentRate = MissionConstraints.ascentRateLimit; MaximumDepth = maximumDepth; 
                  BottomTime = bottomTime; LegDiscreteTime = integrationTime} 

let initStateAndEnvAfterAscent  (integrationTime, controlToIntegration)   maximumDepth  bottomTime  = 
    let strategyOutput, myEnv = getInitConditionAfterDescentPhase (integrationTime, controlToIntegration, integrationTime ) (Some maxSimTime) 1   maximumDepth  bottomTime maximumDepth 
    let (Output initialConditionAfterDescent) = strategyOutput 
    let leState = initialConditionAfterDescent
                  |> Array.head
                  |> ( fun (state , _ , _ , _)  -> state ) 
    leState , myEnv

let createThreeLegAscentWithTheseBCs (State initState:State<LEStatus>) (targetDepth:float) (integrationTime:float) (degreesOfFreedom :Vector<float>) : seq<float>  = 
    seq{0.0}

let evaluateCostUpToTarget arrayOfAscentNodes =
    let time = 1.0
    let risk = 0.01
    (time,risk)

let estimateCostToGo lastNode targetDepth simulateSteadyStateAtTargetDepth ( costToGoApproximator: Option<State<LEStatus> * float  -> float* float>)  = 
    match costToGoApproximator with 
    | Some costToGoFcn  -> (lastNode , targetDepth)
                            |> costToGoFcn
    | None ->  Seq.initInfinite ( fun _ -> targetDepth)
               |> (simulateSteadyStateAtTargetDepth lastNode)
               |> evaluateCostUpToTarget 


// costToGoApproximator is fed with actual state and target depth and spits out time and risk (to target depth from current node)
let defineThreeLegObjectiveFunction (initState   , env ) targetDepth (integrationTime:float) (computeCost:float*float -> float)   ( costToGoApproximator: Option<State<LEStatus> * float  -> float * float>)   = 
    
    let sumTuples (x1:float,x2:float) (y1,y2) =
        x1+y1, x2+y2

    let objectiveFunction (degreesOfFreedom:Vector<float>) = 
         let ascentPath  = createThreeLegAscentWithTheseBCs initState targetDepth  integrationTime degreesOfFreedom
         let simulateFromInitStateWithThisAscent = simulateAscent env None 
         let arrayOfAscentNodes = simulateFromInitStateWithThisAscent initState  ascentPath
         let ascentCost = evaluateCostUpToTarget arrayOfAscentNodes 
         let lastNode = arrayOfAscentNodes |> Array.last |> (fun (x,_,_,_) -> x )
         let costToGo  = estimateCostToGo lastNode  targetDepth (simulateFromInitStateWithThisAscent) costToGoApproximator
         let totalCost = sumTuples ascentCost costToGo
         computeCost totalCost

    Func<_,_> objectiveFunction

let timeNRiskCostFcnWithMaxRIsk maxPDCS (time,risk) = 
    let pDCS = 1.0 - exp(-risk)
    let k = 100.0
    time +  ( ( maxPDCS - pDCS ) * k ) 

let getOptimalSolutioForThisMission timeNRiskToCostFcn {MaxPDCS = maxPDCS ; MaxSimTime = maximumSimulationTime ; IntegrationTime = integrationTime ;
                               ControlToIntegrationTimeRatio = controlToIntegration; DescentRate = descentRate; MaximumDepth = maximumDepth ;
                               BottomTime = bottomTime  }  (targetDepth:float)   ( costToGoApproximator: Option<State<LEStatus> * float  -> float* float>)   = 
                               
    let initAscentStateAndEnv = initStateAndEnvAfterAscent  (integrationTime, controlToIntegration)   maximumDepth  bottomTime

    let computeCost = timeNRiskToCostFcn maxPDCS

    let  objectiveFunction  = defineThreeLegObjectiveFunction initAscentStateAndEnv targetDepth integrationTime computeCost costToGoApproximator

    let (gradient: Func<Vector<float>,Vector<float>, Vector<float>> )  = FunctionMath.GetNumericalGradient  objectiveFunction

    let nlp = new NonlinearProgram()
    
    nlp.ObjectiveFunction <- objectiveFunction
    nlp.ObjectiveGradient <- gradient


    // CONSTRAINTS HAVE TO BE ADDED



    let solution = nlp.Solve()
    let optimalValue = nlp.OptimalValue
    let optimalSolutionReport = nlp.SolutionReport 
    
    solution, optimalValue, optimalSolutionReport 
    
     
//getOptimalSolutioForThisMission simParams maximumDepth None

//let testingOut = getOptimalSolutioForThisMission simParams 0.0 None





// EXAMPLE OF HOW TO USE OPTIMIZATION LIBRARY
let numberOfVariables = 2  

let nlp2 = new NonlinearProgram(numberOfVariables)
 

let f_  (x : Vector<float>) =  x.[0] ** 2.0 + 4.0 * x.[1] ** 2.0 - 32.0 * x.[1] + 64.0

let f = Func<_,_> f_


nlp2.ObjectiveFunction <-  fun x ->  x.[0] ** 2.0 + 4.0 * x.[1] ** 2.0 - 32.0 * x.[1] + 64.0


let g = FunctionMath.GetNumericalGradient  f 

nlp2.ObjectiveGradient <- g 

nlp2.AddNonlinearConstraint(
    (fun (x : Vector<float>) -> x.[0] * x.[1] - x.[0] - x.[1] + 1.5), 
    ConstraintType.LessThanOrEqual, 0.0,
    (fun (x : Vector<float>) (y : Vector<float>) -> y.[0] <- x.[1] - 1.0; y.[1] <- x.[0] - 1.0; y)) |> ignore;
            
// Add constraint x0*x1 >= -10
// If the gradient is omitted, it is approximated using divided differences.
nlp2.AddNonlinearConstraint((fun (x : Vector<float>) -> x.[0] * x.[1]), ConstraintType.GreaterThanOrEqual, -10.0)  


//nlp2.AddLinearConstraint("mammolo" , ConstraintType.GreaterThanOrEqual, )
nlp2.InitialGuess <- Vector.Create(-1.0, 1.0)

let x2 = nlp2.Solve();
printfn "Solution: %O" (x2.ToString("F6"))
// The optimal value is returned by the OptimalValue property:
printfn "Optimal value:   %5f" nlp2.OptimalValue
printfn "# iterations: %d" nlp2.SolutionReport.IterationsNeeded

//printf "Press Enter key to exit..."
////Console.ReadLine() |> ignore

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

let threeLegStrategyFcnBuilder (TemporalValue {Time= initTime; Value = initDepth} ) (targetDepth:float) = 
    0.0
    


let maxPDCS , maxSimTime = 100.0 , 10000.0
let rlDummyParam = 0.0 
let integrationTime , controlToIntegration = 0.1 , 10

let maxDepth , bottomTime = 60.0 , 120.0

let simParams = { MaxPDCS = maxPDCS ; MaxSimTime = maxSimTime; PenaltyForExceedingRisk = rlDummyParam;  RewardForDelivering = rlDummyParam; PenaltyForExceedingTime = rlDummyParam; 
                  IntegrationTime = integrationTime; ControlToIntegrationTimeRatio = controlToIntegration; DescentRate = MissionConstraints.ascentRateLimit; MaximumDepth = maxDepth; 
                  BottomTime = bottomTime; LegDiscreteTime = integrationTime} 

let getOptimalSolutioForThisMission {MaxPDCS = maxPDCS ; MaxSimTime = maximumSimulationTime ; PenaltyForExceedingRisk = penaltyForExceedingRisk ; 
                               RewardForDelivering = rewardForDelivering ; PenaltyForExceedingTime = penaltyForExceedingTime ; IntegrationTime = integrationTime ;
                               ControlToIntegrationTimeRatio = controlToIntegrationTimeRatio; DescentRate = descentRate; MaximumDepth = maximumDepth ;
                               BottomTime = bottomTime ; LegDiscreteTime = legDiscreteTime }  (targetDepth:float)  ( costToGoApproximator: Option<LEStatus -> float>) = 
                               
    let strategyOutput, myEnv = getInitConditionAfterDescentPhase (integrationTime, controlToIntegrationTimeRatio, legDiscreteTime ) 1   maximumDepth  bottomTime maximumDepth
    let (Output initialConditionAfterAscent) = strategyOutput 

    initialConditionAfterAscent // this is hust a placeholder --> needed possibly StrategyOutput

let testingOut = getOptimalSolutioForThisMission simParams 0.0 None


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

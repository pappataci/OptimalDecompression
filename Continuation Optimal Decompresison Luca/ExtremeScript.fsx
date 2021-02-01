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

let evaluateCostUpToTarget (arrayOfAscentNodes: (State<LEStatus> * float * bool * float)[] ) =
    let arrayOfStates = arrayOfAscentNodes
                        |> Array.map (fun (  state,_,_,_) -> state)

    let firstState , lastState = arrayOfStates |> Array.head , arrayOfStates |> Array.last 
    
    let getTimeAndAccruedRisk ( leStatus:State<LEStatus>) =
        [|leStatus2ModelTime ; leStatus2Risk|]
        |> Array.map (fun f -> f leStatus)

    let firstNodeTimeAndDepth =  getTimeAndAccruedRisk firstState   |>  Vector.Create
    let lastNodeTimeAndDepth  =  getTimeAndAccruedRisk lastState    |>  Vector.Create
    lastNodeTimeAndDepth - firstNodeTimeAndDepth

// DEBUGGED 
let timeNResidualRiskToCost (timeNResidualRisk : Vector<float>) = 
    // first component is time; second component is residualRisk
    let gain:Vector<float>   = Vector.Create(1.0, // time gain
                                         2.0)  // residual risk gain 
                                         :> Vector<float>
   
    let weigthedCostComponents = timeNResidualRisk.ElementwiseMultiplyInPlace(gain)
    let weigthedSquaredComponents = Vector.ElementwisePow(weigthedCostComponents, 2.0)
    weigthedSquaredComponents.Sum()
    
// cost approximator LEStatus (needs startgin depth and tissue tensions) -> residualRiskAvailable  -> [|time, actualAccruedRisk * 100.0 |] , arrayOfAscentNodes

let estimateCostToGo ( costToGoToEndMissionApprox: Option<State<LEStatus> -> float   -> (Vector<float> * seq<float>) >) lastNode residualRisk    simulateSteadyStateAtTargetDepth   = 
    
    match costToGoToEndMissionApprox with 
    | Some costToGoFcn  -> costToGoFcn lastNode residualRisk 
    | None ->  let surfaceDepthNodes  = Seq.initInfinite ( fun _ -> 0.0)
               let simulationNodesAtTarget  : (State<LEStatus>*float*bool*float)[] =   simulateSteadyStateAtTargetDepth lastNode surfaceDepthNodes  
               // we know that depths are always equal to target depth: so arrayOfDepths is just seqOfTargetDepthNode take number of nodes in simulationsNodes
               let atTargetDepths = surfaceDepthNodes |> Seq.take (simulationNodesAtTarget |> Array.length)
               evaluateCostUpToTarget simulationNodesAtTarget , atTargetDepths

// costToGoApproximator is fed with actual state and target depth and spits out time and risk (to target depth from current node)
let defineThreeLegObjectiveFunction (initState   , env ) targetDepth (controlTime:float)    (maxPDCS:float) costToGoApproximator  = 
   
    let maxRisk = -log(1.0-maxPDCS  )   // maxPDCS is fractional  (e.g. 3.2% is indicated with 0.032) ; same goes for maximum risk 

    let costToGoToEndApproximator = estimateCostToGo costToGoApproximator

    let objectiveFunction (functionParams:Vector<float>) = 
         
          
         let ascentPath  = createThreeLegAscentWithTheseBCs initState targetDepth  controlTime functionParams
         let simulateFromInitStateWithThisAscent = simulateAscent env None 
         let arrayOfAscentNodes = simulateFromInitStateWithThisAscent initState  ascentPath
         let accruedRiskNTimeToTargetDepth = evaluateCostUpToTarget arrayOfAscentNodes 

         let lastNodeState = arrayOfAscentNodes |> Array.last |> (fun (x,_,_,_) -> x )
         let residualRisk = maxRisk - accruedRiskNTimeToTargetDepth.[1]
          
         // this estimates time for cost to go and gives us the optimal sequence of absence from targetDepth to surface 
         let costToGoTermsToSurface , optimalAscentFromTargetDepthToSurface  = costToGoToEndApproximator lastNodeState  residualRisk (simulateFromInitStateWithThisAscent) 
         
         let totalCostComponents = accruedRiskNTimeToTargetDepth + costToGoTermsToSurface

         totalCostComponents |> timeNResidualRiskToCost
         
    Func<_,_> objectiveFunction

let addLinearConstraints ( nlp: NonlinearProgram ) (startDepth:float) (targetDepth:float) =
    
    let minimumDepthDifference = 0.5  // ft  

    // Create Constraints Leg 1 
    nlp.AddLinearConstraint("DepthStartLeg1"   , [| 0.0;  1.0; 0.0 ;  0.0; 0.0 ; 0.0 ; 0.0; 0.0; 0.0 ; 0.0; 0.0; 0.0; 0.0; 0.0 |]  ,  ConstraintType.LessThanOrEqual     , startDepth - minimumDepthDifference ) |> ignore 
    nlp.AddLinearConstraint("DepthTargetLeg1"  , [| 0.0;  1.0; 0.0 ; -1.0; 0.0 ; 0.0 ; 0.0; 0.0; 0.0 ; 0.0; 0.0; 0.0; 0.0; 0.0 |]  ,  ConstraintType.GreaterThanOrEqual ,  minimumDepthDifference  ) |> ignore 
    nlp.AddLinearConstraint("ConstantTimeLeg1" , [| 0.0;  0.0; 0.0 ;  0.0; 1.0 ; 0.0 ; 0.0; 0.0; 0.0 ; 0.0; 0.0; 0.0; 0.0; 0.0 |]  ,  ConstraintType.GreaterThanOrEqual ,                0.0       ) |> ignore // 0 constant time is plausible 
    
    // Create Constraints Leg 2 
    nlp.AddLinearConstraint("DepthStartLeg2"   , [| 0.0;  0.0; 0.0 ;  1.0; 0.0 ; 0.0 ;  -1.0; 0.0;  0.0 ; 0.0; 0.0; 0.0; 0.0; 0.0 |] ,  ConstraintType.GreaterThanOrEqual , minimumDepthDifference ) |> ignore 
    nlp.AddLinearConstraint("DepthTargetLeg2"  , [| 0.0;  0.0; 0.0 ;  0.0;  0.0 ; 0.0 ;  1.0; 0.0; -1.0 ; 0.0; 0.0; 0.0; 0.0; 0.0 |] ,  ConstraintType.GreaterThanOrEqual , minimumDepthDifference ) |> ignore 
    nlp.AddLinearConstraint("ConstantTimeLeg2" , [| 0.0;  0.0; 0.0 ;  0.0;  0.0 ; 0.0 ;  0.0; 0.0;  0.0 ; 1.0; 0.0; 0.0; 0.0; 0.0 |] ,  ConstraintType.GreaterThanOrEqual , 0.0                   ) |> ignore  
    
    // Create Constraints Leg 3 
    nlp.AddLinearConstraint("DepthStartLeg3"   , [| 0.0;  0.0; 0.0 ;  0.0;  0.0 ; 0.0 ;  0.0; 0.0; 1.0 ; 0.0;  -1.0;  0.0;  0.0; 0.0 |] ,  ConstraintType.GreaterThanOrEqual , minimumDepthDifference ) |> ignore 
    nlp.AddLinearConstraint("DepthTargetLeg3"  , [| 0.0;  0.0; 0.0 ;  0.0;  0.0 ; 0.0 ;  0.0; 0.0; 0.0 ;  0.0;  1.0;  0.0; -1.0; 0.0 |] ,  ConstraintType.GreaterThanOrEqual , minimumDepthDifference ) |> ignore 
    nlp.AddLinearConstraint("ConstantTimeLeg3" , [| 0.0;  0.0; 0.0 ;  0.0;  0.0 ; 0.0 ;  0.0; 0.0; 0.0 ;  0.0;  0.0;  0.0;  0.0; 1.0 |] ,  ConstraintType.GreaterThanOrEqual , 0.0 ) |> ignore  
    
let getOptimalSolutioForThisMission  {MaxPDCS = maxPDCS ; MaxSimTime = maxSimTime ; IntegrationTime = integrationTime ;
                               ControlToIntegrationTimeRatio = controlToIntegration; DescentRate = descentRate; MaximumDepth = maximumDepth ;
                               BottomTime = bottomTime  }  (targetDepth:float)   ( costToGoApproximator )   = 
    
    let controlTime = integrationTime * (controlToIntegration |> float)
    let initAscentStateAndEnv = initStateAndEnvAfterAscent maxSimTime  (integrationTime, controlToIntegration)   maximumDepth  bottomTime
     
    let  objectiveFunction  = defineThreeLegObjectiveFunction initAscentStateAndEnv targetDepth controlTime  maxPDCS costToGoApproximator
    //let (gradient: Func<Vector<float>,Vector<float>, Vector<float>> )  = FunctionMath.GetNumericalGradient  objectiveFunction
    

    let nm = new  PowellOptimizer()
    nm.ExtremumType <- ExtremumType.Minimum

    //nlp.ObjectiveGradient <- gradient
    //addLinearConstraints nlp maximumDepth targetDepth 
    nm.ObjectiveFunction <- objectiveFunction
    nm.InitialGuess <- Vector.Create (-20.0, 50.0 ,  0.0,  30.0 , 1.0,  // first leg with constant times 
                                       -20.0, 25.0 , 0.1  , 18.0,  1.5,  // second leg
                                       -8.0 , 12.0 , 0.3  , 2.5  )       // third leg 

    //let solution = nlp.Solve()
    //let optimalValue = nlp.OptimalValue
    //let optimalSolutionReport = nlp.SolutionReport 
    //let solution = powellOpt.FindExtremum()
    let solution = nm.FindExtremum()
    nm.SolutionTest.AbsoluteTolerance <- 1e-10 
    nm, solution


let maxPDCS , maxSimTime = 0.032 , 50000.0
let rlDummyParam = 0.0 
let integrationTime , controlToIntegration = 0.1 , 10
let maximumDepth , bottomTime = 60.0 , 120.0
let targetDepth = 0.0


let simParams = { MaxPDCS = maxPDCS ; MaxSimTime = maxSimTime; PenaltyForExceedingRisk = rlDummyParam;  RewardForDelivering = rlDummyParam; PenaltyForExceedingTime = rlDummyParam; 
              IntegrationTime = integrationTime; ControlToIntegrationTimeRatio = controlToIntegration; DescentRate = MissionConstraints.ascentRateLimit; MaximumDepth = maximumDepth; 
              BottomTime = bottomTime; LegDiscreteTime = integrationTime} 

let testn = getOptimalSolutioForThisMission  simParams   targetDepth    None


let nm, solution = testn

//let initAscentStateAndEnv = initStateAndEnvAfterAscent maxSimTime  (integrationTime, controlToIntegration)   maximumDepth  bottomTime


// DEBUGGING START 

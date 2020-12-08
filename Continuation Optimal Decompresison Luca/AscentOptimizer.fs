[<AutoOpen>]
module AscentOptimizer
open ReinforcementLearning
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

let getLastNodeIfNotReachedSurface arrayOfStates firstNodeAtSurface = 
    match firstNodeAtSurface with 
    | Some node -> node 
    | None -> arrayOfStates |> Array.last  

let evaluateCostOfThisSequenceOfStates (arrayOfAscentNodes: (State<LEStatus> * float * bool * float)[] ) =
    let arrayOfStates = arrayOfAscentNodes
                        |> Array.map (fun (  state,_,_,_) -> state)
    
    let firstState , lastState = arrayOfStates |> Array.head , arrayOfStates |> Array.last 
     
    let getNodeAtSurfaceLevel = leStatus2Depth 
                                     >> IsAtSurfaceLevel
    
    let firstNodeAtSurface' = arrayOfStates |> Array.tryFind getNodeAtSurfaceLevel 
    
    let firstNodeAtSurface = getLastNodeIfNotReachedSurface arrayOfStates firstNodeAtSurface' 

    let accruedRisk = [|firstState ; lastState |]
                      |> Array.map  leStatus2Risk 
                      |> (fun x -> x.[1] - x.[0])

    
    let timeToSurface = [|firstState ; firstNodeAtSurface    |]
                        |> Array.map  leStatus2ModelTime 
                        |> (fun x -> x.[1] - x.[0])

    Vector.Create([|timeToSurface ; accruedRisk|])
    :> Vector<float> 

    //let getTimeAndAccruedRisk ( leStatus:State<LEStatus>) =
    //    [|leStatus2ModelTime ; leStatus2Risk|]
    //    |> Array.map (fun f -> f leStatus)

    //let firstNodeTimeAndRisk =  getTimeAndAccruedRisk firstState   |>  Vector.Create
    //let lastNodeTimeAndRisk  =  getTimeAndAccruedRisk lastState    |>  Vector.Create

    //lastNodeTimeAndRisk - firstNodeTimeAndRisk

// DEBUGGED 
let timeNResidualRiskToCost (timeNResidualRisk : Vector<float>) = 
    // first component is time; second component is residualRisk
    let gain:Vector<float>   = Vector.Create(1.0e-1, // time gain
                                         1.0e5)  // residual risk gain 
                                         :> Vector<float>
   
    let weigthedCostComponents = timeNResidualRisk.ElementwiseMultiplyInPlace(gain)
    let weigthedSquaredComponents = Vector.ElementwisePow(weigthedCostComponents, 2.0)
    let output = weigthedSquaredComponents.Sum()
    output 

let estimateCostToGo ( costToGoToEndMissionApprox: Option<State<LEStatus> -> float   -> (Vector<float> * seq<float>) >) lastNode residualRisk    simulateSteadyStateAtTargetDepth   =  
    
    match costToGoToEndMissionApprox with 
    | Some costToGoFcn  -> costToGoFcn lastNode residualRisk 
    | None ->  let surfaceDepthNodes  = Seq.initInfinite ( fun _ -> 0.0)
               let simulationNodesAtTarget  : (State<LEStatus>*float*bool*float)[] =   simulateSteadyStateAtTargetDepth lastNode surfaceDepthNodes  

               let sequenceOfDepths = simulationNodesAtTarget |> Seq.map (fun (state, _, _, _) -> state |> leStatus2Depth )                
               evaluateCostOfThisSequenceOfStates simulationNodesAtTarget , sequenceOfDepths
               
// costToGoApproximator is fed with actual state and target depth and spits out time and risk (to target depth from current node)
//let defineThreeLegObjectiveFunction (initState   , env ) targetDepth (controlTime:float)    (maxPDCS:float) costToGoApproximator  = 
   
//    let maxRisk = -log(1.0-maxPDCS  )   // maxPDCS is fractional  (e.g. 3.2% is indicated with 0.032) ; same goes for maximum risk 

//    let costToGoToEndApproximator = estimateCostToGo costToGoApproximator

//    let expressRiskInTermsOfResidualRisk   (maxRisk: float ) (totalRawCostComponents:Vector<float>) = 
//        let netCost  = totalRawCostComponents.Clone()
//        netCost.[1] <- maxRisk - totalRawCostComponents.[1] 
//        netCost

//    let objectiveFunction (functionParams:Vector<float>) = 
          
//         //let ascentPath  = createThreeLegAscentWithTheseBCs initState targetDepth  controlTime functionParams
         
//         let ascentPath  = ascentWithOneStep initState targetDepth  controlTime functionParams

//         let simulateFromInitStateWithThisAscent = simulateAscent env None 
//         let arrayOfAscentNodes = simulateFromInitStateWithThisAscent initState  ascentPath
//         printfn "arrayOfState %A" arrayOfAscentNodes
//         let accruedRiskNTimeToTargetDepth = evaluateCostOfThisSequenceOfStates arrayOfAscentNodes 

//         let lastNodeState = arrayOfAscentNodes |> Array.last |> (fun (x,_,_,_) -> x )
//         let residualRisk = maxRisk - accruedRiskNTimeToTargetDepth.[1]
          
//         // this estimates time for cost to go and gives us the optimal sequence of absence from targetDepth to surface 
//         let costToGoTermsToSurface , optimalAscentFromTargetDepthToSurface  = costToGoToEndApproximator lastNodeState  residualRisk (simulateFromInitStateWithThisAscent) 
      
//         let totalCostComponents = accruedRiskNTimeToTargetDepth + costToGoTermsToSurface
//                                   |> expressRiskInTermsOfResidualRisk  maxRisk
           
//         let cost = totalCostComponents |> timeNResidualRiskToCost
//         //printfn "Components Cost %A "  totalCostComponents
//         //printfn "Total Cost %A " cost 
//         cost
         
//    Func<_,_> objectiveFunction

let defineOneStepObjFcn (initState   , env ) targetDepth (controlTime:float)    (maxPDCS:float) costToGoApproximator  = 
     
    let maxRisk = -log(1.0-maxPDCS  )   // maxPDCS is fractional  (e.g. 3.2% is indicated with 0.032) ; same goes for maximum risk 

    let costToGoToEndApproximator = estimateCostToGo costToGoApproximator

    let expressRiskInTermsOfResidualRisk   (maxRisk: float ) (totalRawCostComponents:Vector<float>) = 
        let netCost  = totalRawCostComponents.Clone()
        netCost.[1] <- maxRisk - totalRawCostComponents.[1] 
        netCost

    let objectiveFunction (functionParams:Vector<float>) = 
         
         let ascentPath'  = ascentWithOneStep initState targetDepth  controlTime functionParams
         
         let ascentPath = ascentPath' |> Seq.concat  |> Seq.map snd  |> Seq.skip 1 
         let simulateFromInitStateWithThisAscent = simulateAscent env None 

         //printfn "ASCENT PATH first- last %A" (ascentPath |> Seq.head , ascentPath |> Seq.last )

         let arrayOfAscentNodes = simulateFromInitStateWithThisAscent initState  ascentPath

         //printfn "array of nodesfirst- last %A" (arrayOfAscentNodes |> Seq.head , arrayOfAscentNodes |> Seq.last )
         
         let accruedRiskNTimeToTargetDepth = evaluateCostOfThisSequenceOfStates arrayOfAscentNodes 

         //printfn "ACCRUED RISK %A" (accruedRiskNTimeToTargetDepth)

         let lastNodeState = arrayOfAscentNodes |> Array.last |> (fun (x,_,_,_) -> x )

         //printfn "%A THIS IS THE LAST NODE" lastNodeState

         let residualRisk = maxRisk - accruedRiskNTimeToTargetDepth.[1]
          
         // this estimates time for cost to go and gives us the optimal sequence of absence from targetDepth to surface 
         let costToGoTermsToSurface , optimalAscentFromTargetDepthToSurface  = costToGoToEndApproximator lastNodeState  residualRisk (simulateFromInitStateWithThisAscent) 
      
         let totalCostComponents = accruedRiskNTimeToTargetDepth + costToGoTermsToSurface
                                   |> expressRiskInTermsOfResidualRisk  maxRisk
           
         let cost = totalCostComponents |> timeNResidualRiskToCost
         printfn "Components Cost %A "  totalCostComponents
         //printfn "Total Cost %A " cost 
         cost
         
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


let getOptimalSolutionForThisMission  {MaxPDCS = maxPDCS ; MaxSimTime = maxSimTime ; IntegrationTime = integrationTime ;
                               ControlToIntegrationTimeRatio = controlToIntegration; DescentRate = descentRate; MaximumDepth = maximumDepth ;
                               BottomTime = bottomTime  }  (targetDepth:float) (initialGuess:Vector<float>)    ( costToGoApproximator )  = 
    
    let controlTime = integrationTime * (controlToIntegration |> float) // TO BE CHECKED
    let initAscentStateAndEnv = initStateAndEnvAfterAscent maxSimTime  (integrationTime, controlToIntegration)   maximumDepth  bottomTime
     
    // THIS HAS TO BE ABSTRACTED OUT (with automatic identification of number of params)
    //let  objectiveFunction  = defineThreeLegObjectiveFunction initAscentStateAndEnv targetDepth controlTime  maxPDCS costToGoApproximator
    let  objectiveFunction  = defineOneStepObjFcn initAscentStateAndEnv targetDepth controlTime  maxPDCS costToGoApproximator

    
    let (gradient: Func<Vector<float>,Vector<float>, Vector<float>> )  = FunctionMath.GetNumericalGradient  objectiveFunction
    
    // Start optimization
    
    //powellOpt.ExtremumType <- ExtremumType.Minimum
    //powellOpt.Dimensions <- 14 
    //powellOpt.ObjectiveFunction <- objectiveFunction


    let powellOpt = new  PowellOptimizer()
    powellOpt.ExtremumType <- ExtremumType.Minimum

    //nlp.ObjectiveGradient <- gradient
    //addLinearConstraints nlp maximumDepth targetDepth 
    powellOpt.ObjectiveFunction <- objectiveFunction
    powellOpt.InitialGuess <- initialGuess
    


    //let solution = nlp.Solve()
    //let optimalValue = nlp.OptimalValue
    //let optimalSolutionReport = nlp.SolutionReport 
    //let solution = powellOpt.FindExtremum()
    let solution = powellOpt.FindExtremum()
    powellOpt.SolutionTest.AbsoluteTolerance <- 1e-10 
    powellOpt, solution



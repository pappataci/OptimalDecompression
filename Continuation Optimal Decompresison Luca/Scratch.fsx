

//let addSteadyStateValue increment (seqTimeDepth:seq<float*float>)  steadyStateValue = 
//    let lastElementAtSteadyState = (seqTimeDepth 
//                                    |> Seq.last 
//                                    |> fst 
//                                    |> (+) increment , steadyStateValue ) 
//    Seq.append seqTimeDepth  (Seq.singleton  lastElementAtSteadyState  )

    
    //solution , powellOpt.SolutionReport
    //, optimalValue, optimalSolutionReport 
    


    //PowellOptimizer pw = new PowellOptimizer();
    //  pw.ExtremumType    = ExtremumType.Minimum;
    //  pw.Dimensions      = 2;
      
    //  // Create the initial guess
    //  // The first element is the exponent and the second
    //  // element is the break fraction.  
    //  var initialGuess   = Vector.Create(0.0,0.0);
    //  initialGuess [ 0 ] = 10.0 *  exponent  ;
    //  initialGuess [ 1 ] = 10.0 * breakFraction;
      
    //  // Powell's method does not use derivatives:
    //  pw.InitialGuess = initialGuess;
    //  pw.ObjectiveFunction =  ObjectiveFunction;
    //  pw.FindExtremum ( );


    //let addAtTheBeginningOfVector (originalParams:Vector<float>)  (initDepth:float) = 
    //    let output  = Vector.Create<float> (originalParams.Length + 1  ) 
    //    output.[Range(1,originalParams.Length  )] <- originalParams
    //    output.[0] <- initDepth
    //    output :> Vector<float> 
    
    
    //let  params2Depths:Vector<float> -> seq<float>   =   (getDepthsVectorFromParams depthsIndices)
    //                                                      >> ( Seq.scan mapRealValueToDepthForThisTarget  maxDepth  )

    //let inputNotExceeding bound (x , _:float ) = 
    //    x <= bound + tolerance

// START PREVIOUS DEBUGGING SECTION

//let  params2Depths:Vector<float> -> seq<float>   =   (getDepthsVectorFromParams depthsIndices)
//                                                                     >> ( Seq.scan mapRealValueToDepthForThisTarget  maxDepth  )


//let updatedParamsValues = myParams'' 
//                            |> params2Depths
//                            |> Seq.toArray
//                            |> Array.skip 1 



//let initialGuess =  Vector.Create (-10.0, 0.0 ,  0.0,  0.0 , 1.0,  // first leg with constant times 
//                                   -10.0, 0.0 , 0.1  , 0.0,  1.5,  // second leg
//                                   -5.0 , 0.0 , 0.3  , 2.5  )       // third leg 





//initialGuess
//|> positionDepthsAccordingToConstraints  (depthsIndices ) (maxDepth ) (targetDepth )   
//|> Seq.toArray


//let paramsCompleted = addFinalDepthToParams initialGuess targetDepth
//let paramsCompletedTransformed = positionDepthsAccordingToConstraints depthsIndices maxDepth  targetDepth   paramsCompleted 



//let actualDepthParams = initialGuess
//                                   |> params2Depths
//                                   |> Seq.toArray
//                                   |> Array.skip 1 


//let depthToParamTransform maxValue minValue xToBeTransformed = 
//   let k = 0.1 
//   let argInv = (xToBeTransformed - minValue) / (maxValue - minValue )
//   (1.0/k) * log ( atanh (argInv) ) 



//let bfgs = new Extreme.Mathematics.Optimization.TrustRegionReflectiveOptimizer()

//bfgs.InitialGuess <- x0
//bfgs.Extremum <- Extreme.Mathematics.Optimization.ExtremumType.Minimum

//bfgs.ObjectiveFunction <- objectiveFunction
//bfgs.FastGradientFunction <- gradient 

//objectiveFunction.Invoke(x0)
//|> printfn "%A"
//let a = objectiveFunction x0 x0  

//let opt = bfgs.FindExtremum()

//let test = FunctionMath.CentralDerivative( objectiveFunction , 0.01) 

//let optimizer = new QuasiNewtonOptimizer()
//optimizer.ObjectiveFunction <- objectiveFunction
//optimizer.GradientFunction <- (System.Func<_,_> gr) 
//optimizer.InitialGuess <- x0

//optimizer.ExtremumType <- ExtremumType.Minimum

//let answer = optimizer.FindExtremum()

//optimizer.SolutionReport.Status

//let mapRealValueToDepthForThisTarget = mapRealValueToDepth targetDepth



//let initialGuess =  Vector.Create (-10.0, 0.0 ,  -10.0,  15.0 , 25.0,  // first leg with constant times 
//                                   -5.0, 10.0 , 0.0,    5.0 ) 

//let initialGuess' = Vector.Create (-1.0, 10.0 , 0.0,    5.0 )  // first leg with constant times 



//let simpleAscentPath = ascentWithOneStep initState 0.0 0.1 initialGuess
//                       |> Seq.toArray 

//simpleAscentPath 
//|> Seq.map snd 
//|>  writeArrayToDisk "testSimple.csv" None 


// INIT OPTIMAL_ASCENT_DEBUGGER_SCRIPT
//type SegmentDefiner =    ( Vector<float> -> seq<float*float> ) 


//let atanh = Extreme.Mathematics.Elementary.Atanh
//let concat (x:Vector<'T> )  y  =  LinqExtensions.Concat(x,y)

//let outputNotExceeding maxTarget (_:float , y) = 
//    y  >= maxTarget - tolerance

//let evaluateFcnBetweenMinMax increment fcn initValue (computationBound: float*float -> bool) = 
//    let generator x =  
//        let y = fcn x 
//        match ( computationBound(x,y) ) with 
//        | true  -> ( (x, y)   , x + increment  ) |> Some 
//        | false -> None
    
//    initValue
//    |> Seq.unfold generator

//let straightLineEqGen( myParams:Vector<float> )   =
//    let slope = myParams.[0]
//    let initTime  = myParams.[1]
//    let initDepth = myParams.[2]  
//    let bias = initDepth - slope * initTime
//    (fun x -> slope * x + bias )

//let tanhEqGen ( myParams:Vector<float> )   =
//    let initDerivative = myParams.[0]     
//    let initDepth = myParams.[1]          
//    let alpha = myParams.[2]             
//    let targetDepth = myParams.[3]       
//    let initTime = myParams.[5]  // BC    

//    let deltaF = initDepth - targetDepth
//    let rho = -initDerivative/(alpha*deltaF)
//    let beta =  atanh (rho - 1.0) - alpha * initTime
//    let delta = tanh (alpha * initTime + beta)
//    let A = deltaF / (delta - 1.0)
//    let B = targetDepth - A 
//    (fun x -> let xHat = alpha * x + beta 
//              A * tanh(xHat) + B  ) 

//let addTargetPointToOutput (actualOutput:seq<float*float>)  increment targetDepth = 
//    let actualDepth, actualValue  = actualOutput |> Seq.head 
//    let target = actualDepth + increment , targetDepth 
//    seq { yield! actualOutput 
//          yield  target  }

//let straightLineSectionGen increment ( curveParams:Vector<float> )   = 
    
//   // curveParams ordering: 
//   // 0 r  - Ramp Slope
//   // 1 Ft - Target Depth  
//   // 4 Tr - Time Ramp (Time Start)
//   // 5 Fs - Function Start 
//   let targetDepth = curveParams.[1]
//   let initTime = curveParams.[5]
//   let straightLineFcn = concat (curveParams.GetSlice(0,0)) ( curveParams.GetSlice(5,6) )
//                         |> straightLineEqGen  
//   let output =  evaluateFcnBetweenMinMax increment straightLineFcn initTime (outputNotExceeding targetDepth)
   
//   match ( (output |> Seq.length ) = 1 ) with 
//                   |   true    ->  
//                               addTargetPointToOutput output increment curveParams.[1]
//                   |   false   ->  output 

//let addTargetNodeIfEmpty  (initTime,   increment)  (functionStart, approximateTarget ) (tanhPart:seq<float*float>) =
//    match tanhPart |> Seq.isEmpty with
//    | true -> let out = seq{(initTime , functionStart) ; 
//                            (initTime + increment, approximateTarget ) }
//             // printfn "PASSED"
//           //   printfn "INSIDE ADD %A" out 
//              out 
//    | false ->    //  printfn "PASSED FALSE"
//                    //printfn "INSIDE FALSE  %A" tanhPart
//                    tanhPart 

//let tanhSectionGen  increment ( curveParams:Vector<float> ) = 
    
//   // curveParams ordering: 
//   // 0 r      - Init Function Derivative Value
//   // 1 Fs     - Function Start
//   // 2 alpha  - Tanh Param
//   // 3 Ft     - Function Target
//   // 4 Tr     - Time Tanh (Time Start)
   
//   let percentTolerance  , maxAbsoluteTolerance = 5.0e-3 , 1.0
//   let initTime = curveParams.[5] // CORRECT 
//   let targetDepth = curveParams.[3] // CORRECT 
//   let targetIncrementTolerance =  (max (targetDepth * percentTolerance) increment )  
//                                   |> min maxAbsoluteTolerance 

//   let approximateTarget = targetDepth  + targetIncrementTolerance  //CORRECT 
//   let functionStart = curveParams.[1]
//   //printfn "%A Fs, Ft , approxTarget " (functionStart , curveParams.[3] , approximateTarget) 

//   let tanhFcn = tanhEqGen (curveParams)
//   let tanhPart = (evaluateFcnBetweenMinMax increment tanhFcn initTime (outputNotExceeding approximateTarget)  )
//                  |>  addTargetNodeIfEmpty  (initTime,   increment)   (functionStart, approximateTarget )

//   let getLastTime = Seq.last >> fst 

//   let  lastTime  = getLastTime tanhPart 

//   let extraTime = curveParams.[4]  // minutes 
    
//   let constantLegDeltaTime =  seq{0.0 .. increment .. extraTime}
   
//   let constantLeg = constantLegDeltaTime
//                     |> Seq.map (fun deltaT ->  let x = lastTime + deltaT 
//                                                let y =  tanhFcn  x 
//                                                x , y    )
//                     |> Seq.skip 1 

//   let output = seq{   yield! tanhPart
//                       yield! constantLeg   }
   
//   output 

//let getNumberofCurves degreesOfFreedom =
//    // four parameters define one line-tanh curve (the last one is missing)
//    (degreesOfFreedom + 1) / 5  

//let createComputationPipeLine numberOfLegs oneLegComputation  = 
//    oneLegComputation
//    |> Seq.replicate numberOfLegs

//let computeCriticalAlpha (myParamsSection:Vector<float>) = 
//    let slope = myParamsSection.[0]
//    let initValue = myParamsSection.[1]
//    let targetValue = myParamsSection.[3]
//    -0.5 * slope / (initValue - targetValue)

//let param2AlphaValue (myParamsSection:Vector<float>) =  
//    let alphaParam =  min (  max myParamsSection.[2]  -20.0 ) 30.0   
//    let alphaComputed = exp(alphaParam) + (  computeCriticalAlpha myParamsSection )
//    let returnVec = Vector.Create (myParamsSection.ToArray())
//    returnVec.[2] <- alphaComputed
//    returnVec

//let oneLegComputation computationSeq myParams (bc:Vector<float>)    = 

//    let initSeqState = seq{bc.[0] , bc.[1]} 
//    let myParamsWithAlpha = param2AlphaValue myParams
     
//    let folderFcn (state: seq<float*float>)  (genFcn) = 
//        let bcTime, bcDepth = state |> Seq.last 
//        let bc =  Vector.Create(bcTime, bcDepth )
//        concat myParamsWithAlpha  bc
//        |> genFcn

//    Seq.scan folderFcn initSeqState  computationSeq
//    |> Seq.map (Seq.skip 1) 
//    |> Seq.concatlet oneLegComputation computationSeq myParams (bc:Vector<float>)    = 

//    let initSeqState = seq{bc.[0] , bc.[1]} 
//    let myParamsWithAlpha = param2AlphaValue myParams
     
//    let folderFcn (state: seq<float*float>)  (genFcn) = 
//        let bcTime, bcDepth = state |> Seq.last 
//        let bc =  Vector.Create(bcTime, bcDepth )
//        concat myParamsWithAlpha  bc
//        |> genFcn

//    Seq.scan folderFcn initSeqState  computationSeq
//    |> Seq.map (Seq.skip 1) 
//    |> Seq.concat

//let defineSegmentDefiner (integrationTime:float)  sectionGenerator  =
//    sectionGenerator integrationTime

//let addFinalDepthToParams (myParams:Vector<float>) targetDepth = 
//    let numOfParams = myParams |> Seq.length
//    let returnVecArray = Vector.Create<float> (numOfParams + 1 )   
//    returnVecArray.[Range(0, numOfParams - 2  )] <- myParams.GetSlice(0, numOfParams - 2 )
//    returnVecArray.[numOfParams] <- myParams.[numOfParams - 1 ] 
//    returnVecArray.[numOfParams - 1 ] <- targetDepth
//    returnVecArray 
//    :> Vector<float>


//let  folderForMultipleFunctions (paramsCompleted:Vector<float>)   (paramsIdxInit:int  , previousLegSequence:seq<float*float>  )   (segmetDefiner:seq<SegmentDefiner>)   = 
//    let actualSubParams = paramsCompleted.GetSlice(5*paramsIdxInit, 5* paramsIdxInit + 4) // included constant segment 

//    let bcTime, bcDepth = previousLegSequence |> Seq.last 
//    let bcVector = Vector.Create( bcTime, bcDepth)
//    let oneLegAscent = oneLegComputation segmetDefiner  actualSubParams  bcVector 
//    (paramsIdxInit + 1  ,  oneLegAscent   )  

//let createThreeLegAscentWithTheseBCs (  initState:State<LEStatus>) (targetDepth:float) (integrationTime:float) (myParams :Vector<float>)    = 
    
//    let leStatus2BCInit initState = 
//        let initDepth = leStatus2Depth initState
//        let initTimeNDepth =  seq{ leStatus2ModelTime initState,  initDepth }
//        (0 , initTimeNDepth), initDepth 

//    let numberOfCurves = getNumberofCurves  myParams.Length  
//    let initBC , initDepth  = leStatus2BCInit initState
     
//    let threeAscentComptPipeline: seq<seq<SegmentDefiner>> = seq{straightLineSectionGen ; tanhSectionGen }
//                                                             |> Seq.map ( defineSegmentDefiner  integrationTime )
//                                                             |> createComputationPipeLine numberOfCurves
   
    
//    let depthsIndices = [|1;3;6;8;11|]
//    let paramsCompleted = addFinalDepthToParams myParams targetDepth
//    let paramsCompletedTransformed = positionDepthsAccordingToConstraints depthsIndices initDepth  targetDepth   paramsCompleted 

//    let folderWithTheseParams = folderForMultipleFunctions paramsCompletedTransformed 

        
//    let seqOfLegs  = Seq.scan folderWithTheseParams initBC  threeAscentComptPipeline
//                     |> Seq.map snd 
    

//    seqOfLegs
//    |> Seq.concat 
//    //|> Seq.map snd 


//let ascentWithOneStep (  initState:State<LEStatus>) (targetDepth:float) (integrationTime:float) (myParams :Vector<float>)    = 
    
//    let leStatus2BCInit initState = 
//        let initDepth = leStatus2Depth initState
//        let initTimeNDepth =  seq{ leStatus2ModelTime initState,  initDepth }
//        (0 , initTimeNDepth), initDepth 

//    let numberOfCurves = 2
//    let initBC , initDepth  = leStatus2BCInit initState
     
//    let threeAscentComptPipeline: seq<seq<SegmentDefiner>> = seq{straightLineSectionGen ; tanhSectionGen }
//                                                             |> Seq.map ( defineSegmentDefiner  integrationTime )
//                                                             |> createComputationPipeLine numberOfCurves
   
    
//    let depthsIndices = [|1;3;6 |]
//    let paramsCompleted = addFinalDepthToParams myParams targetDepth
//    let paramsCompletedTransformed = positionDepthsAccordingToConstraints depthsIndices initDepth  targetDepth   paramsCompleted 

//    let folderWithTheseParams = folderForMultipleFunctions paramsCompletedTransformed 

        
//    let seqOfLegs  = Seq.scan folderWithTheseParams initBC  threeAscentComptPipeline
//                     |> Seq.map snd 
    

//    seqOfLegs
//    |> Seq.concat 
//    //|> Seq.map snd 

//let mutable globalVar = 1.0

//let initFcn x = 
//    globalVar <- x
//    x

//deltaTimeSurface

    //let solveAscentProblemWithTheseGrids (paramsGrid:float[][]) (initCondGrid:float[][]) (integrationTime, controlToIntegration) = 
    //    let toTuple (x:float[]) = 
    //        x.[0], x.[1], x.[2]

    //    let createOutputName (x:float[]) = 
    //        let bt, maxDepth, probBound = toTuple x 
    //        printfn "solving %A" x
    //        "BT_" +  (sprintf "%i" (int bt) ) + "_" + "MD_"  
    //        + (sprintf "%i" (int maxDepth) ) + "PB" + sprintf "%.1f" (probBound*100.0) + ".csv"
        
    //    initCondGrid
    //    |> Array.map  ( fun initCond -> let outputName = createOutputName initCond
    //                                    initCond
    //                                    |> toTuple   
    //                                    |> getOptimalForThisInputCondition  paramsGrid (integrationTime, controlToIntegration) 
    //                                    |>  bruteForceToDisk outputName   )
           
    //let example = initConditionsGrid |> Array.take 2
    //solveAscentProblemWithTheseGrids  paramsGrid  initConditionsGrid  (integrationTime, controlToIntegration)
    //|> ignore

//let allInputs = create3DGrid breakFracSeq exponents deltaTimeSurface

//let resultsToArray (inputVec:float[], result:StrategyResults) =
//    (inputVec.[0], inputVec.[1], inputVec.[2], result.AscentTime, result.AscentRisk, result.SurfaceRisk,
//     result.TotalRisk, result.InitTimeAtSurface)

//let getOptimalForThisInputCondition (bottomTime, maximumDepth, pDCS) =
//    let maxAllowedRisk = pDCSToRisk pDCS
//    allInputs
//    |> getAllSolutionsForThisProblem  (integrationTime, controlToIntegration) (bottomTime, maximumDepth, pDCS)
//    |> Array.zip allInputs 
//    |> Array.filter (fun  (inputVec, result )  -> result.TotalRisk < maxAllowedRisk )
//    |> Array.sortBy ( fun (inputV, res) -> res.AscentTime)
//    |> Array.map resultsToArray

//let getOptimalForThisInputCondition  paramsGrid (integrationTime, controlToIntegration) (bottomTime, maximumDepth, pDCS) =
//    let maxAllowedRisk = pDCSToRisk pDCS
//    paramsGrid
//    |> getAllSolutionsForThisProblem  (integrationTime, controlToIntegration) (bottomTime, maximumDepth, pDCS)
//    |> Array.zip paramsGrid 
//    |> Array.filter (fun  (inputVec, result )  -> result.TotalRisk < maxAllowedRisk )
//    |> Array.sortBy ( fun (inputV, res) -> res.AscentTime)
//    |> Array.map resultsToArray

//breakParams is the grid of internal params (break , exp) 

//  let initCondition = [| bottomTime; maximumDepth; pDCS|] 

//(fun (x:StrategyResults) -> x.TotalRisk <= maxAllowedRisk) )

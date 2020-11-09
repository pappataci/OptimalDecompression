

let addSteadyStateValue increment (seqTimeDepth:seq<float*float>)  steadyStateValue = 
    let lastElementAtSteadyState = (seqTimeDepth 
                                    |> Seq.last 
                                    |> fst 
                                    |> (+) increment , steadyStateValue ) 
    Seq.append seqTimeDepth  (Seq.singleton  lastElementAtSteadyState  )

    
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
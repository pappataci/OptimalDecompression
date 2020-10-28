

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

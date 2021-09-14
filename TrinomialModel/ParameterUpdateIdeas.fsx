
//let curveGen = linPowerCurveGenerator decisionTime initialNode curveParams


//let curveText (curveGen:seq<DepthTime>) = 
//    curveGen
//    |> Seq.map (fun x -> x.Time.ToString() + ",  " + x.Depth.ToString())


//let curveDescriptions = curveGen |> curveText

//open System.IO
//File.WriteAllLines(@"C:\Users\glddm\Desktop\New folder\text.txt" , curveDescriptions)




//getInitialConditionNode profileOut ascentParams.[0]

//let testProblematicOut = runModelOnProfile problematicProfile  
//module  Params = 
//    let mutable A = 1.0


//module Mammolo = 
//    let f x = Params.A * x 

//module ChangeParams  =
//    let changeParams x = 
//        Params.A <- x 

// OK THis is interesting for changing the parameter



// %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%% //



//let runOptimizationForThisTableEntry (tableEntry:TableMissionMetrics) 
//                                     (InitialGuessFcn initialGuessFcn)
//                                     (TrajGen trajectoryGenerator )  = 
    
//    let initialNode = tableEntry.InitAscentNode
//    let initialGuess = initialGuessFcn initialNode
    


//    0.0
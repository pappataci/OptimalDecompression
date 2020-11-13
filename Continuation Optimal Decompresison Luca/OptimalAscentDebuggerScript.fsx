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

open ReinforcementLearning
open LEModel
open Extreme.Mathematics
open AscentBuilder
open AscentOptimizer
     


let mapRealValueToDepth minDepth maxDepth realValue      = 
     
    let buffer = 0.01
    let xScalingFactor = 0.1 
    let delta  = maxDepth - minDepth - 2.0* buffer 
    let linearFactor = realValue
                       |> (*) xScalingFactor
                       |> exp
                       |> tanh
    
    max (minDepth + linearFactor * delta ) (minDepth + buffer) 


let getDepthsVectorFromParams (depthsIndices:int[])  (myParams:Vector<float>) = 
    depthsIndices
    |> Seq.map (fun index -> myParams.[index])


let positionDepthsAccordingToConstraints  (depthsIndices:int[]) (maxDepth:float) (targetDepth:float)   (originalParams:Vector<float>)   =
    
    let mapRealValueToDepthForThisTarget = mapRealValueToDepth targetDepth

    let  params2Depths:Vector<float> -> seq<float>   =   (getDepthsVectorFromParams depthsIndices)
                                                          >> ( Seq.scan mapRealValueToDepthForThisTarget  maxDepth  )
    let updatedParamsValues = originalParams  
                              |> params2Depths
                              |> Seq.toArray
                              |> Array.skip 1 

    let substituteTheseComponents (initVector:Vector<float>) (updatedValues:float[]) (indeces: int[])=
        
        let targetVector = initVector.Clone()
         
        [| 0 .. ( indeces.Length - 1)  |] 
        |> Array.iter (fun index -> targetVector.[indeces.[index]]  <- updatedValues.[index]  )
        targetVector

    let updateParamsVector = substituteTheseComponents  originalParams updatedParamsValues depthsIndices
     
    updateParamsVector


let createThreeLegAscentWithTheseBCs (  initState:State<LEStatus>) (targetDepth:float) (integrationTime:float) (myParams :Vector<float>)    = 
    
    let leStatus2BCInit initState = 
        let initDepth = leStatus2Depth initState
        let initTimeNDepth =  seq{ leStatus2ModelTime initState,  initDepth }
        (0 , initTimeNDepth), initDepth 

    let numberOfCurves = getNumberofCurves  myParams.Length  
    let initBC , initDepth  = leStatus2BCInit initState
     
    let threeAscentComptPipeline: seq<seq<SegmentDefiner>> = seq{straightLineSectionGen ; tanhSectionGen }
                                                             |> Seq.map ( defineSegmentDefiner  integrationTime )
                                                             |> createComputationPipeLine numberOfCurves
   
    
    let depthsIndices = [|1;3;6;8;11|]
    let paramsCompleted = addFinalDepthToParams myParams targetDepth
    let paramsCompletedTransformed = positionDepthsAccordingToConstraints depthsIndices initDepth  targetDepth   paramsCompleted 

    let folderWithTheseParams = folderForMultipleFunctions paramsCompletedTransformed 

        
    let seqOfLegs  = Seq.scan folderWithTheseParams initBC  threeAscentComptPipeline
                     |> Seq.map snd 
    

    seqOfLegs
    |> Seq.concat 
    //|> Seq.map snd 

let ascentWithOneStep (  initState:State<LEStatus>) (targetDepth:float) (integrationTime:float) (myParams :Vector<float>)    = 
    
    let leStatus2BCInit initState = 
        let initDepth = leStatus2Depth initState
        let initTimeNDepth =  seq{ leStatus2ModelTime initState,  initDepth }
        (0 , initTimeNDepth), initDepth 

    let numberOfCurves = 2
    let initBC , initDepth  = leStatus2BCInit initState
     
    let threeAscentComptPipeline: seq<seq<SegmentDefiner>> = seq{straightLineSectionGen ; tanhSectionGen }
                                                             |> Seq.map ( defineSegmentDefiner  integrationTime )
                                                             |> createComputationPipeLine numberOfCurves
   
    
    let depthsIndices = [|1;3;6 |]
    let paramsCompleted = addFinalDepthToParams myParams targetDepth
    let paramsCompletedTransformed = positionDepthsAccordingToConstraints depthsIndices initDepth  targetDepth   paramsCompleted 

    let folderWithTheseParams = folderForMultipleFunctions paramsCompletedTransformed 

        
    let seqOfLegs  = Seq.scan folderWithTheseParams initBC  threeAscentComptPipeline
                     |> Seq.map snd 
    

    seqOfLegs
    |> Seq.concat 
    //|> Seq.map snd 


let integrationTime = 0.1
let initTime = 120.0  // mins
let maxDepth = 60.0 // ft
let targetDepth = 0.0
let controlTime = integrationTime * 10.0
let initState = createFictitiouStateFromDepthTime (initTime, maxDepth) 



// DEBUGGING OBJECTIVE FUNCTION (CHECK)
let maxPDCS , maxSimTime = 0.032 , 50000.0
let controlToIntegration = 10 
let maximumDepth = 60.0 
let bottomTime = 120.0
let initAscentStateAndEnv = initStateAndEnvAfterAscent maxSimTime  (integrationTime, controlToIntegration)   maximumDepth  bottomTime
 
let costToGoApproximator = None 

// THIS HAS TO BE ABSTRACTED OUT (with automatic identification of number of params)
//let  objectiveFunction  = defineThreeLegObjectiveFunction initAscentStateAndEnv targetDepth controlTime  maxPDCS costToGoApproximator
let  objectiveFunction : System.Func<Vector<float>, float>  = defineOneStepObjFcn initAscentStateAndEnv targetDepth controlTime  maxPDCS costToGoApproximator


let x0 = Vector.Create (-5.0, -19.5 , 0.0,  0.0 , 26.0,  -5.0, 0.0 , 0.0,    5.0 )  :> Vector<float> 

let (gradient: System.Func<Vector<float>,Vector<float>, Vector<float>> )  = FunctionMath.GetNumericalGradient  objectiveFunction

let y = x0.Clone()
 

let perturb (dt:float)   i = 
    let t = Vector.Create<float>( x0|> Seq.length )
    t.[i] <- dt
    t

let gr x0= 
    let dt = 0.01
    [|0 ..  ( (x0|> Seq.length) - 1 )  |]
    |> Array.map ( fun idx -> let k = perturb dt idx 
                              let perturbed = k + x0 
                              let refValue = objectiveFunction.Invoke(x0)
                              (objectiveFunction.Invoke(perturbed) - refValue) / dt )
                              |> Vector.Create :> Vector<float>




let sgd x0 (eta:float) (epsilon:float) =
    let rec desc x = 
        let g = gr x   
        if g.Norm() < epsilon then   x else  printfn "f , g %A , "  ( ( objectiveFunction.Invoke(x0) ) , ( x0|> Seq.toArray ) ); desc  ( x - eta * g )
    desc x0

let out = sgd x0 0.0003 0.00001

let test = Vector.Create([|-5.711321627; -19.34735327; -0.008394359101; 0.07975548012; 26.0; -4.975883364;0.01974830003; -0.1006232377; 5.0|])

objectiveFunction.Invoke(test)
gr(test)

let actual = Vector.Create([|-5.102223817; -19.98396963; -0.02151706647; 5.909616754; 26.0; -11.3107369;-0.3610388302; -3.281399114; 5.0|])
objectiveFunction.Invoke(actual)
gr(actual) |> Seq.toArray


let out2 = sgd actual 0.01 0.00001

actual
|> ascentWithOneStep initState 0.0 0.1 
|> Seq.toArray 
|> Seq.map snd 
|>  writeArrayToDisk "ascentOpt.csv" None 
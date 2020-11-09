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

open ReinforcementLearning
open InitDescent
open LEModel
open Extreme.Mathematics
open  Extreme.DataAnalysis.Linq 

type SegmentDefiner =    ( Vector<float> -> seq<float*float> ) 

let atanh = Extreme.Mathematics.Elementary.Atanh
let concat (x:Vector<'T> )  y  =  LinqExtensions.Concat(x,y)

let outputNotExceeding maxTarget (_:float , y) = 
    y  >= maxTarget - tolerance

let evaluateFcnBetweenMinMax increment fcn initValue (computationBound: float*float -> bool) = 
    let generator x =  
        let y = fcn x 
        match ( computationBound(x,y) ) with 
        | true  -> ( (x, y)   , x + increment  ) |> Some 
        | false -> None
    
    initValue
    |> Seq.unfold generator

let straightLineEqGen( myParams:Vector<float> )   =
    let slope = myParams.[0]
    let initTime  = myParams.[1]
    let initDepth = myParams.[2]  
    let bias = initDepth - slope * initTime
    (fun x -> slope * x + bias )

let tanhEqGen ( myParams:Vector<float> )   =
    let initDerivative = myParams.[0]     
    let initDepth = myParams.[1]          
    let alpha = myParams.[2]             
    let targetDepth = myParams.[3]       
    let initTime = myParams.[5]  // BC    

    let deltaF = initDepth - targetDepth
    let rho = -initDerivative/(alpha*deltaF)
    let beta =  atanh (rho - 1.0) - alpha * initTime
    let delta = tanh (alpha * initTime + beta)
    let A = deltaF / (delta - 1.0)
    let B = targetDepth - A 
    (fun x -> let xHat = alpha * x + beta 
              A * tanh(xHat) + B  ) 
     
let addTargetPointToOutput (actualOutput:seq<float*float>)  increment targetDepth = 
    let actualDepth, actualValue  = actualOutput |> Seq.head 
    let target = actualDepth + increment , targetDepth 
    seq { yield! actualOutput 
          yield  target  }

let straightLineSectionGen increment ( curveParams:Vector<float> )   = 
     
    // curveParams ordering: 
    // 0 r  - Ramp Slope
    // 1 Ft - Target Depth  
    // 4 Tr - Time Ramp (Time Start)
    // 5 Fs - Function Start 
    let targetDepth = curveParams.[1]
    let initTime = curveParams.[5]
    let straightLineFcn = concat (curveParams.GetSlice(0,0)) ( curveParams.GetSlice(5,6) )
                          |> straightLineEqGen  
    let output =  evaluateFcnBetweenMinMax increment straightLineFcn initTime (outputNotExceeding targetDepth)
    
    match ( (output |> Seq.length ) = 1 ) with 
                    |   true    ->  
                                addTargetPointToOutput output increment curveParams.[1]
                    |   false   ->  output 
     
let addTargetNodeIfEmpty  (initTime,   increment)  (functionStart, approximateTarget ) (tanhPart:seq<float*float>) =
    match tanhPart |> Seq.isEmpty with
    | true -> let out = seq{(initTime , functionStart) ; 
                            (initTime + increment, approximateTarget ) }
             // printfn "PASSED"
           //   printfn "INSIDE ADD %A" out 
              out 
    | false ->    //  printfn "PASSED FALSE"
                    //printfn "INSIDE FALSE  %A" tanhPart
                    tanhPart 
    
let tanhSectionGen  increment ( curveParams:Vector<float> ) = 
     
    // curveParams ordering: 
    // 0 r      - Init Function Derivative Value
    // 1 Fs     - Function Start
    // 2 alpha  - Tanh Param
    // 3 Ft     - Function Target
    // 4 Tr     - Time Tanh (Time Start)
    
    let percentTolerance  , maxAbsoluteTolerance = 5.0e-3 , 1.0
    let initTime = curveParams.[5] // CORRECT 
    let targetDepth = curveParams.[3] // CORRECT 
    let targetIncrementTolerance =  (max (targetDepth * percentTolerance) increment )  
                                    |> min maxAbsoluteTolerance 

    let approximateTarget = targetDepth  + targetIncrementTolerance  //CORRECT 
    let functionStart = curveParams.[1]
    //printfn "%A Fs, Ft , approxTarget " (functionStart , curveParams.[3] , approximateTarget) 

    let tanhFcn = tanhEqGen (curveParams)
    let tanhPart = (evaluateFcnBetweenMinMax increment tanhFcn initTime (outputNotExceeding approximateTarget)  )
                   |>  addTargetNodeIfEmpty  (initTime,   increment)   (functionStart, approximateTarget )

    let getLastTime = Seq.last >> fst 

    let  lastTime  = getLastTime tanhPart 

    let extraTime = curveParams.[4]  // minutes 
     
    let constantLegDeltaTime =  seq{0.0 .. increment .. extraTime}
    
    let constantLeg = constantLegDeltaTime
                      |> Seq.map (fun deltaT ->  let x = lastTime + deltaT 
                                                 let y =  tanhFcn  x 
                                                 x , y    )
                      |> Seq.skip 1 

    let output = seq{   yield! tanhPart
                        yield! constantLeg   }
    
    output 

let getNumberofCurves degreesOfFreedom =
    // four parameters define one line-tanh curve (the last one is missing)
    (degreesOfFreedom + 1) / 5  

let createComputationPipeLine numberOfLegs oneLegComputation  = 
    oneLegComputation
    |> Seq.replicate numberOfLegs

let computeCriticalAlpha (myParamsSection:Vector<float>) = 
    let slope = myParamsSection.[0]
    let initValue = myParamsSection.[1]
    let targetValue = myParamsSection.[3]
    -0.5 * slope / (initValue - targetValue)

let param2AlphaValue (myParamsSection:Vector<float>) =  
    let alphaParam =  min (  max myParamsSection.[2]  -20.0 ) 30.0   
    let alphaComputed = exp(alphaParam) + (  computeCriticalAlpha myParamsSection )
    let returnVec = Vector.Create (myParamsSection.ToArray())
    returnVec.[2] <- alphaComputed
    returnVec

let oneLegComputation computationSeq myParams (bc:Vector<float>)    = 

    let initSeqState = seq{bc.[0] , bc.[1]} 
    let myParamsWithAlpha = param2AlphaValue myParams
     
    let folderFcn (state: seq<float*float>)  (genFcn) = 
        let bcTime, bcDepth = state |> Seq.last 
        let bc =  Vector.Create(bcTime, bcDepth )
        concat myParamsWithAlpha  bc
        |> genFcn

    Seq.scan folderFcn initSeqState  computationSeq
    |> Seq.map (Seq.skip 1) 
    |> Seq.concat
 
let defineSegmentDefiner (integrationTime:float)  sectionGenerator  =
    sectionGenerator integrationTime

let addFinalDepthToParams (myParams:Vector<float>) targetDepth = 
    let numOfParams = myParams |> Seq.length
    let returnVecArray = Vector.Create<float> (numOfParams + 1 )   
    returnVecArray.[Range(0, numOfParams - 2  )] <- myParams.GetSlice(0, numOfParams - 2 )
    returnVecArray.[numOfParams] <- myParams.[numOfParams - 1 ] 
    returnVecArray.[numOfParams - 1 ] <- targetDepth
    returnVecArray 
    :> Vector<float>

let  folderForMultipleFunctions (paramsCompleted:Vector<float>)   (paramsIdxInit:int  , previousLegSequence:seq<float*float>  )   (segmetDefiner:seq<SegmentDefiner>)   = 
    let actualSubParams = paramsCompleted.GetSlice(5*paramsIdxInit, 5* paramsIdxInit + 4) // included constant segment 

    let bcTime, bcDepth = previousLegSequence |> Seq.last 
    let bcVector = Vector.Create( bcTime, bcDepth)
    let oneLegAscent = oneLegComputation segmetDefiner  actualSubParams  bcVector 
    (paramsIdxInit + 1  ,  oneLegAscent   )  


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


let mapRealValueToDepthForThisTarget = mapRealValueToDepth targetDepth



let initialGuess =  Vector.Create (-10.0, 0.0 ,  -10.0,  15.0 , 25.0,  // first leg with constant times 
                                   -5.0, 10.0 , 0.0,    5.0 ) 

//let initialGuess' = Vector.Create (-1.0, 10.0 , 0.0,    5.0 )  // first leg with constant times 



let simpleAscentPath = ascentWithOneStep initState 0.0 0.1 initialGuess
                       |> Seq.toArray 

simpleAscentPath 
|> Seq.map snd 
|>  writeArrayToDisk "testSimple.csv" None 

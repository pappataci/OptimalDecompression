#r @"C:\Users\glddm\source\repos\DecompressionL20190920A\packages\Extreme.Numerics.7.0.15\lib\net46\Extreme.Numerics.dll"
#r @"C:\Users\glddm\source\repos\DecompressionL20190920A\packages\Microsoft.ML.Probabilistic.0.3.1912.403\lib\netstandard2.0\Microsoft.ML.Probabilistic.dll"

#load "SeqExtension.fs"
#load "PredefinedDescent.fs"
#load "ReinforcementLearning.fs"
#load "Gas.fs"
#load "LEModel.fs"
 
open InitDescent
open Extreme.Mathematics
open  Extreme.DataAnalysis.Linq 
open LEModel

let atanh = Extreme.Mathematics.Elementary.Atanh
let concat (x:Vector<'T> )  y  =  LinqExtensions.Concat(x,y)

let outputNotExceeding maxTarget (_:float , y) = 
    y  >= maxTarget - tolerance

let inputNotExceeding bound (x , _:float ) = 
     x <= bound + tolerance

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
    let initTime = myParams.[4]  // BC
    
    let deltaF = initDepth - targetDepth
    let rho = -initDerivative/(alpha*deltaF)
    let beta =  atanh (rho - 1.0) - alpha * initTime
    let delta = tanh (alpha * initTime + beta)
    let A = deltaF / (delta - 1.0)
    let B = targetDepth - A 
    (fun x -> let xHat = alpha * x + beta 
              A * tanh(xHat) + B  ) 
     
let straightLineSectionGen increment ( curveParams:Vector<float> ) (bc:Vector<float>)  = 
    // curveParams ordering: 
    // 0 r  - Ramp Slope
    // 1 Ft - Target Depth  
    // 4 Tr - Time Ramp (Time Start)
    // 5 Fs - Function Start 
    let targetDepth = curveParams.[1]
    let initTime = bc.[0]
    
    let straightLineFcn = concat (curveParams.GetSlice(0,0)) ( bc )
                          |> straightLineEqGen  
    evaluateFcnBetweenMinMax increment straightLineFcn initTime (outputNotExceeding targetDepth)
    
let tanhSectionGen  increment ( curveParams:Vector<float> )  (bc:Vector<float>) = 
    // curveParams ordering: 
    // 0 r      - Init Function Derivative Value
    // 1 Ft     - Function Start
    // 2 alpha  - Tanh Param
    // 3 Ft     - Function Target
    // 4 Tr     - Time Tanh (Time Start)
    
    let percentTolerance = 5.0e-3
    let initTime = bc.[0]
    let targetDepth = curveParams.[4]
    let targetIncrementTolerance =  (max (targetDepth * percentTolerance) increment )   
    let approximateTarget = targetDepth  + targetIncrementTolerance

    let tanhCurveParams = concat (curveParams.GetSlice(0,3))( bc.GetSlice(0,0) )

    let tanhFcn = tanhEqGen (curveParams)
    evaluateFcnBetweenMinMax increment tanhFcn initTime (outputNotExceeding approximateTarget)  

let getNumberofCurves degreesOfFreedom =
    // four parameters define one line-tanh curve (the last one is missing)
    (degreesOfFreedom + 1) / 5  

let createComputationPipeLine numberOfLegs oneLegComputation  = 
    oneLegComputation
    |> Seq.replicate numberOfLegs

let param2AlphaValue (myParamsSection:Vector<float>) =  
    let computeCriticalAlpha (myParamsSection:Vector<float>) = 
        let slope = myParamsSection.[0]
        let initValue = myParamsSection.[1]
        let targetValue = myParamsSection.[3]
        -0.5 * slope / (initValue - targetValue)
    let alphaParam =  min (  max myParamsSection.[2]  -20.0 ) 30.0   
    let alphaComputed = exp(alphaParam) + (  computeCriticalAlpha myParamsSection )
    let returnVec = Vector.Create (myParamsSection.ToArray())
    returnVec.[2] <- alphaComputed
    let returnVec = returnVec :> Vector<float>
    returnVec 

type SegmentDefiner = Vector<float> -> Vector<float> -> seq<float*float> 

let oneLegComputation (computationSeq:seq<SegmentDefiner> ) myParams (bc:Vector<float>)    = 
    let initSeqState = seq{bc.[0] , bc.[1]} 
    let myParamsWithAlpha = param2AlphaValue myParams
    
    let folderFcn (state: seq<float*float>)  (genFcn) = 
        let bcTime, bcDepth = state |> Seq.last 
        let bc =  (Vector.Create(bcTime, bcDepth )) :> Vector<float>
        printfn "PASSED TO FUNCTION %A" (myParamsWithAlpha |> Seq.toArray ,   bc |> Seq.toArray ) 
        genFcn myParamsWithAlpha  bc
         
    Seq.scan folderFcn initSeqState  computationSeq
    //|> Seq.map (Seq.skip 1) 
    |> Seq.concat

let integrationTime = 0.05 

let initTime = 102.0  // mins
let maxDepth = 60.0 // ft

let targetDepth = 5.0


let myParams = Vector.Create (-10.0, 50.0 , 0.0, 10.0 , 30.0, // first leg
                              -20.0, 25.0 , -1.0, 20.0, 18.0 , // second leg
                              -8.0,  12.0,  1.5, 20.0 ) // third leg WITH CONSTANT TIMES

let numberOfCurves = getNumberofCurves  myParams.Length  


let firstSegmentDefiner : SegmentDefiner  = straightLineSectionGen integrationTime
let secondSegmentDefiner  : SegmentDefiner= tanhSectionGen integrationTime
let  computationSeq  : seq<SegmentDefiner> = seq{firstSegmentDefiner ;  secondSegmentDefiner }  
let threeAscentComptPipeline = createComputationPipeLine numberOfCurves  computationSeq
let paramsCompleted = concat myParams  ( Vector.Create(targetDepth) ) 

let  folderForMultipleFunctions (paramsCompleted:Vector<float>)   (paramsIdxInit:int  , previousLegSequence:seq<float*float>  )   (segmetDefiner:seq<SegmentDefiner>)   = 
    
    let actualSubParams = paramsCompleted.GetSlice(5*paramsIdxInit, 5* paramsIdxInit + 4 ) // included constant segment 
    
    let bcTime, bcDepth = previousLegSequence |> Seq.last 
    let bcVector = Vector.Create( bcTime, bcDepth)  :> Vector<float>
    let oneLegAscent = oneLegComputation segmetDefiner  actualSubParams  bcVector 

    (paramsIdxInit + 1  ,  oneLegAscent   )  

let folderWithTheseParams = folderForMultipleFunctions paramsCompleted 

let bc =  ( initTime ,  maxDepth )

let initBC = 0 , seq{bc}   // this is a stub 

let A = Seq.scan folderWithTheseParams initBC  threeAscentComptPipeline
        //|> Seq.toArray
// how do I execute ONE leg? 

//let get idx =  A.[idx]|> snd|>Array.ofSeq |> Array.map snd 

let bc4OneLeg  =  Vector.Create( initTime ,  maxDepth )  :> Vector<float>

oneLegComputation computationSeq myParams (bc4OneLeg ) 
|> Seq.toArray  // THIS WORKS
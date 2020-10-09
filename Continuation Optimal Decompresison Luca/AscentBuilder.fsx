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
open SeqExtension
open LEModel

let atanh = Extreme.Mathematics.Elementary.Atanh
let concat (x:Vector<'T> )  y  =  LinqExtensions.Concat(x,y)

type LegComputation = {Generator : Vector<float> -> seq<float*float> ; Dispatcher :  Vector<float> ->  Vector<float> -> Vector<float>  }
 
//let increment = 0.1

// this includes the final point (maxTarget)

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
    let initTime  = myParams.[0]
    let initDepth = myParams.[1] 
    let slope = myParams.[2]
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
     
let straightLineSectionGen increment ( curveParams:Vector<float> )   = 
    // curveParams ordering: 
    // 0 Tr - Time Ramp (Time Start)
    // 1 Fs - Function Start
    // 2 r  - Ramp Slope
    // 3 Ft - Function Target - this is how the dispatcher 
    let straightLineFcn = straightLineEqGen ( curveParams.GetSlice(0,2)  )  
    let initTime = curveParams.[0]
    let targetDepth = curveParams.[3]
    evaluateFcnBetweenMinMax increment straightLineFcn initTime (outputNotExceeding targetDepth)

let tanhSectionGen  increment ( curveParams:Vector<float> ) = 
    // curveParams ordering: 
    // 0 r      - Init Function Derivative Value
    // 1 Fs     - Function Start
    // 2 alpha  - Tanh Param
    // 3 Ft     - Function Target
    // 4 Tr     - Time Tanh (Time Start)
    
    let percentTolerance = 1.0e-2
    let initTime = curveParams.[4]
    let targetDepth = curveParams.[3]
    let targetIncrementTolerance =  (max (targetDepth * percentTolerance) increment )   
    let approximateTarget = targetDepth  + targetIncrementTolerance
    let tanhFcn = tanhEqGen (curveParams)
    evaluateFcnBetweenMinMax increment tanhFcn initTime (outputNotExceeding approximateTarget)  
    
let straightLineDispatcher (paramsVec : Vector<float> ) ( bcVector: Vector<float>) = 
    let freeParams = paramsVec.GetSlice(0 , 1 )
    concat bcVector freeParams 

let tanhDispatcher (paramsVec : Vector<float> ) (  bcVector: Vector<float>) = 
    //let freeParams = paramsVec.GetSlice(4*sectionIdx, 4*sectionIdx+3)
    concat paramsVec (bcVector.GetSlice(0 ,0))

let getNumberofCurves degreesOfFreedom =
    // four parameters define one line-tanh curve (the last one is missing)
    (degreesOfFreedom + 1) / 4  

let createComputationPipeLine numberOfLegs oneLegComputation  = 
    oneLegComputation
    |> Array.toSeq
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

let createThreeLegAscentWithTheseBCs (  initState:State<LEStatus>) (targetDepth:float) (integrationTime:float) (myParams :Vector<float>) : seq<float>  = 
    
    let numberOfCurves = getNumberofCurves  myParams.Length  
    let firstSegmentDefiner  = {Generator = straightLineSectionGen integrationTime; Dispatcher =  straightLineDispatcher } 
    let secondSegmentDefiner = {Generator = tanhSectionGen         integrationTime; Dispatcher =  tanhDispatcher         }
    let singleLegComputation = [|firstSegmentDefiner ;  secondSegmentDefiner|]   
    
    
    let threeAscentComptPipeline = createComputationPipeLine numberOfCurves  singleLegComputation
    
    let computationFolder state output =
        state

    let initTime, initDepth = leStatus2ModelTime  initState , leStatus2Depth  initState 
    let initStateVec = Vector.Create(initTime, initDepth )
    
    // this computation is for creating one leg
    //Seq.scan computationFolder 

    // include final BC
    let generalizedParams = concat myParams  ( Vector.Create( targetDepth  ) ) 

    //let generator 


    //let firstLegDefinition = () 
    

    
    seq{0.0}


let oneLegComputation (computationSeq ) myParams (bc:Vector<float>)    =
    
    let initSeqState = seq{bc.[0] , bc.[1]} 

    let myParamsWithAlpha = param2AlphaValue myParams
    
    let folderFcn (state: seq<float*float>)  ({Generator = genFcn; Dispatcher = dispatchFcn }) = 
        
        let bcTime, bcDepth = state |> Seq.last 
        let bc=  Vector.Create(bcTime, bcDepth )

        let injectedParams =  dispatchFcn myParamsWithAlpha  bc
        printfn "%A" injectedParams
        genFcn injectedParams

    Seq.scan folderFcn ( initSeqState )  computationSeq


// TESTING AREA //

let integrationTime = 0.1  

let firstSegmentDefiner  = {Generator = straightLineSectionGen integrationTime; Dispatcher =  straightLineDispatcher } 
let secondSegmentDefiner = {Generator = tanhSectionGen integrationTime; Dispatcher =  tanhDispatcher         }


let initTime = 102.0  // mins
let maxDepth = 60.0 // ft

let bc = Vector.Create( initTime ,  maxDepth )

let myParams   = Vector.Create (-10.0, 50.0 , 1.0,  20.0)

let computationSeq = seq{firstSegmentDefiner  }

oneLegComputation computationSeq myParams bc 
|>Seq.toArray




let addSteadyStateValue increment (seqTimeDepth:seq<float*float>)  steadyStateValue = 
    let lastElementAtSteadyState = (seqTimeDepth 
                                    |> Seq.last 
                                    |> fst 
                                    |> (+) increment , steadyStateValue ) 
    Seq.append seqTimeDepth  (Seq.singleton  lastElementAtSteadyState  )
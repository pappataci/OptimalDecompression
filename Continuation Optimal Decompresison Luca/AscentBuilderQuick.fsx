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

type SegmentDefiner =    ( Vector<float> -> seq<float*float> ) 

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
    let initDerivative = myParams.[0]    // CORRECT 
    let initDepth = myParams.[1]         // CORRECT
    let alpha = myParams.[2]             // CORRECT
    let targetDepth = myParams.[3]       // CORRECT
    let initTime = myParams.[5]  // BC   // WRONG 

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
    // 0 r  - Ramp Slope
    // 1 Ft - Target Depth  
    // 4 Tr - Time Ramp (Time Start)
    // 5 Fs - Function Start 
    let targetDepth = curveParams.[1]
    let initTime = curveParams.[5]
    let straightLineFcn = concat (curveParams.GetSlice(0,0)) ( curveParams.GetSlice(5,6) )
                          |> straightLineEqGen  

    evaluateFcnBetweenMinMax increment straightLineFcn initTime (outputNotExceeding targetDepth)

let tanhSectionGen  increment ( curveParams:Vector<float> ) = 
     
    // curveParams ordering: 
    // 0 r      - Init Function Derivative Value
    // 1 Ft     - Function Start
    // 2 alpha  - Tanh Param
    // 3 Ft     - Function Target
    // 4 Tr     - Time Tanh (Time Start)
    
    let percentTolerance = 5.0e-3
    let initTime = curveParams.[5] // CORRECT 
    let targetDepth = curveParams.[3] // CORRECT 
    let targetIncrementTolerance =  (max (targetDepth * percentTolerance) increment )    // CORRECT
    let approximateTarget = targetDepth  + targetIncrementTolerance  //CORRECT 

    let tanhFcn = tanhEqGen (curveParams)
    let tanhPart = evaluateFcnBetweenMinMax increment tanhFcn initTime (outputNotExceeding approximateTarget)  
    let getLastTime = Seq.last >> fst 
    let  lastTime  = getLastTime tanhPart 

    let extraTime = curveParams.[4]  // minutes 
     
    let constantLegDeltaTime =  seq{0.0 .. increment .. extraTime}
    let constantLeg = constantLegDeltaTime
                      |> Seq.map (fun deltaT ->  let x = lastTime + deltaT 
                                                 let y =  tanhFcn  x 
                                                 x , y    )
                      |> Seq.skip 1 
    
    seq{   yield! tanhPart
           yield! constantLeg   }

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

let integrationTime = 0.1

let initTime = 102.0  // mins
let maxDepth = 60.0 // ft


let targetDepth = 5.0

let myParams   = Vector.Create (-10.0, 50.0 , 0.0,  30.0 , 12.0,  // first leg with constant times 
                                -20.0, 25.0 , -1.0, 18.0,  1.5,  // second leg
                                -8.0 , 12.0 , 1.5  , 2.5  )       // third leg 

let numberOfCurves = getNumberofCurves  myParams.Length  // DONE


let defineSegmentDefiner (integrationTime:float)  sectionGenerator  =
    sectionGenerator integrationTime
     
let firstSegmentDefiner   = defineSegmentDefiner  integrationTime straightLineSectionGen                                       
let secondSegmentDefiner   =  defineSegmentDefiner integrationTime tanhSectionGen

let  computationSeq  : seq<SegmentDefiner> = seq{firstSegmentDefiner ;  secondSegmentDefiner }  

let threeAscentComptPipeline = createComputationPipeLine numberOfCurves  computationSeq

let addFinalDepthToParams (myParams:Vector<float>) targetDepth = 
    let numOfParams = myParams |> Seq.length
    let returnVecArray = Vector.Create<float> (numOfParams + 1 )   
    returnVecArray.[Range(0, numOfParams - 2  )] <- myParams.GetSlice(0, numOfParams - 2 )
    returnVecArray.[numOfParams] <- myParams.[numOfParams - 1 ] 
    returnVecArray.[numOfParams - 1 ] <- targetDepth
    returnVecArray 
    :> Vector<float>

let paramsCompleted = addFinalDepthToParams myParams targetDepth 

let  folderForMultipleFunctions (paramsCompleted:Vector<float>)   (paramsIdxInit:int  , previousLegSequence:seq<float*float>  )   (segmetDefiner:seq<SegmentDefiner>)   = 
    let actualSubParams = paramsCompleted.GetSlice(5*paramsIdxInit, 5* paramsIdxInit + 4) // included constant segment 
     
    let bcTime, bcDepth = previousLegSequence |> Seq.last 
    let bcVector = Vector.Create( bcTime, bcDepth)
     
    let oneLegAscent = oneLegComputation segmetDefiner  actualSubParams  bcVector 

    (paramsIdxInit + 1  ,  oneLegAscent   )  

let folderWithTheseParams = folderForMultipleFunctions paramsCompleted 

let bc =  ( initTime ,  maxDepth )

let initBC = 0 , seq{bc}   // this is a stub 

let A' = Seq.scan folderWithTheseParams initBC  threeAscentComptPipeline
        |> Seq.toArray
// how do I execute ONE leg? 

let A = A' |> Seq.map snd |> Seq.toArray 

let get idx =  A'.[idx]|> snd|>Array.ofSeq |> Array.map snd 


let createThreeLegAscentWithTheseBCs (  initState:State<LEStatus>) (targetDepth:float) (integrationTime:float) (myParams :Vector<float>)    = 
    
    let leStatus2BCInit initState = 
        let initTimeNDepth =  seq{ leStatus2ModelTime initState,  leStatus2Depth initState}
        0 , initTimeNDepth

    let numberOfCurves = getNumberofCurves  myParams.Length  
    
    let firstSegmentDefiner   = defineSegmentDefiner  integrationTime straightLineSectionGen                                       
    let secondSegmentDefiner   =  defineSegmentDefiner integrationTime tanhSectionGen
    
    let threeAscentComptPipeline: seq<seq<SegmentDefiner>> = seq{straightLineSectionGen ; tanhSectionGen }
                                                             |> Seq.map ( defineSegmentDefiner  integrationTime )
                                                             |> createComputationPipeLine numberOfCurves
    

    let paramsCompleted = addFinalDepthToParams myParams targetDepth 
    let folderWithTheseParams = folderForMultipleFunctions paramsCompleted 
    let seqOfLegs  = Seq.scan folderWithTheseParams initBC  threeAscentComptPipeline
                     |> Seq.map snd 
                      
    
    seqOfLegs
    |> Seq.concat 
    |> Seq.map snd 

let createFictitiouState (initTime, initDepth) = 

    let tensions = [|Tension 1.0; Tension 1.0; Tension 1.0|]
    let temporalValue = TemporalValue {Time = initTime ; Value = initDepth}
    let leState = {TissueTensions = tensions  
                   CurrentDepthAndTime = temporalValue } 

    let fictitiousRisk = {   AccruedRisk       =          0.0 
                             IntegratedRisks   =  [|0.0;0.0;0.0|] }

    {LEPhysics = leState ; Risk = fictitiousRisk}
    |> State

let testInitState = createFictitiouState (initTime, maxDepth) 

let out = createThreeLegAscentWithTheseBCs testInitState targetDepth integrationTime myParams
          |> Seq.toArray
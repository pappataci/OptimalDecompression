#r @"C:\Users\glddm\source\repos\DecompressionL20190920A\packages\Extreme.Numerics.7.0.15\lib\net46\Extreme.Numerics.dll"
#load "SeqExtension.fs"
#load "PredefinedDescent.fs"

open InitDescent
open SeqExtension
open Extreme.Mathematics
open Extreme.Mathematics.Generic
open Extreme.Mathematics.Optimization

let atanh = Extreme.Mathematics.Elementary.Atanh


// goal is: given these parameters return a function of the optimization problem 
// which ingests the params and spits out a sequence of depths

// subgoal: array of functions with given defined intervals ( ? ) 

// small example with a fixed straight line

 
let increment = 0.1

// this includes the final point (maxTarget)

let outputNotExceeding maxTarget (_:float , y) = 
    y  >= maxTarget - tolerance

let inputNotExceeding bound (x , _:float ) = 
     x <= bound + tolerance

let evaluateFcnBetweenMinMax increment fcn initValue (computationBound: float*float -> bool) = 
    let generator = fun x -> let y = fcn x 
                             match ( computationBound(x,y) ) with 
                             | true  -> ( (x, y)   , x + increment  ) |> Some 
                             | false -> None
    initValue
    |> Seq.unfold generator


let straightLineEqGen (myParams:float[]) (x:float) =
    let initDepth = myParams.[0] 
    let initTime  = myParams.[1]
    let slope = myParams.[2]
    let bias = initDepth - slope * initTime
    slope * x + bias

let tanhEqGen ( myParams:float[] )   =
    let initDepth = myParams.[0] // BC
    let initTime = myParams.[1]  // BC
    let initDerivative = myParams.[2]   // BC
    let alpha = myParams.[3]
    let targetDepth = myParams.[4]

    let deltaF = initDepth - targetDepth
    let rho = -initDerivative/(alpha*deltaF)
    let beta =  atanh (rho - 1.0) - alpha * initTime
    let delta = tanh (alpha * initTime + beta)
    let A = deltaF / (delta - 1.0)
    let B = targetDepth - A 
    (fun x -> let xHat = alpha * x + beta 
              A * tanh(xHat) + B  ) 
     




// we start experimenting with only wiht one straight line
let oneToyLegExample (increment:float) (TemporalValue {Time = initTime; Value = initDepth} ) (targetDepth:float) = 
    let initDerivative = -5.0
    let maxTime = 10.0
    let tanhExample (mySliceOfParameters:Vector<float>) = // iin this case x is pretty much: hInit, t0r, t0t
        
        let straightLineFcn = straightLineEqGen( [|initDepth ; initTime; mySliceOfParameters.[0] |])
        let m = evaluateFcnBetweenMinMax increment straightLineFcn initTime (outputNotExceeding targetDepth)
        
        

        let tanhFcn = tanhEqGen( [|initDepth ; initTime; initDerivative ; mySliceOfParameters.[0]; mySliceOfParameters.[1]  |])
        evaluateFcnBetweenMinMax increment tanhFcn  initTime ( inputNotExceeding maxTime) 

    tanhExample
    

let defLegSegmentGenerator (bc:float[]) (actualParams:float[]) = 
    seq{(0.0, 0.0);(0.1, 0.1)}


let bcPropagatorGen  (connector: (float*float) -> float[]) = 
     Seq.last >> connector 

let dummyConnector (x:float,y) = 
    [|x + y|] 

let (bcDummyPropagator:seq<float*float>->float[])  = bcPropagatorGen dummyConnector

let lin2TanhConnector (lastValue: float*float) =
    [|1.0;2.0|]

let oneLegExample (increment:float) (TemporalValue {Time = initTime; Value = initDepth} ) (targetDepth:float) = 
    // the rest of the parameters should be optimized
    // for now hard type the two equations, then abstract them out

    // Straight Line
    let oneLegEqn(optParams:Vector<float>) = 
        
        // here we partition the input for the first leg 
        // FIRST LEG
        let bc0 = [|initDepth ; initTime |]
        let params0 = optParams.GetSlice(0,1).ToArray()

        // suppose we have legSegmentGenerator
        let firstLegGen = defLegSegmentGenerator bc0 params0 

        // SECOND LEG -tanh- 
        //let bcForSecondLeg = getBCForTanh firstLegGen 
        let lastValue = firstLegGen |> Seq.last 

        let bcForTanh = lin2TanhConnector lastValue


        seq{0.0}

    oneLegEqn


// dummy example; suppose we have the following
let line (bc: Vector<float>) (actualParams:Vector<float>) = 
    seq{0.0;0.1}

let myTanh(bc:Vector<float>)(tanParams:Vector<float>)=
    seq{0.1;1.1;1.2;1.3}


let (seqFcn:seq<Vector<float> -> Vector<float> -> seq<float>>)  = seq{line; myTanh}

let testOut initCondition seqFcn = 
    Seq.mapFold

//hyperbolicTangentExample
// small test
let targetDepth =  5.0

let (seqout )  = oneToyLegExample 0.1 (TemporalValue { Time = 5.0; Value = 30.0 }) targetDepth 

let ascentExample = Vector.Create( [| 1.5; targetDepth|] )
                    |> seqout


//mergeSequenceSkippingIC
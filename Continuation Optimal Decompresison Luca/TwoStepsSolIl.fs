module TwoStepsSolIl

open InitDescent
open LEModel
open InputDefinition
open ILNumerics 
//open type ILMath
open type ILNumerics.Toolboxes.Optimization

let maximumAscentTime = 2000.0
let maxSimTime = 5.0e4  // this is irrelevant

let initState2TimeAndDepthGen initState = 
    let bottomTime = initState |> leStatus2ModelTime
    let maximumDepth = initState |> leStatus2Depth
    bottomTime, maximumDepth

let getExponentialPartGen linearAscent deltaTimeToSurface  exponent (bottomTime:float, maximumDepth:float ) controlTime =
    
    let initTime, initDepth = match Seq.isEmpty linearAscent with
                              | true  -> bottomTime, maximumDepth
                              | false -> linearAscent |> Seq.last 
    
    let timeSteps =  ( deltaTimeToSurface / controlTime )  
                     |> int 

    //printfn "TimeSteps:  %A" deltaTimeToSurface
     
    let incrementTime idx = initTime + float(idx  ) * controlTime
    let estimatedDiscreteSurfTime = incrementTime timeSteps 
    let breakOutToSurf = estimatedDiscreteSurfTime - initTime

    let idx2TimeIncrement idx = (controlTime * (float  idx  ) )  

    let timeIncrementPercent idx = (idx2TimeIncrement idx) / (breakOutToSurf)  
    let linearPath idx =  MissionConstraints.ascentRateLimit * (idx2TimeIncrement idx ) + initDepth  
    let exponentialPath idx = initDepth  - initDepth * ( ( timeIncrementPercent idx )  ** exponent )

    Seq.init timeSteps ( fun idxSeq  -> let idx = idxSeq  + 1 
                                        let actualTime = incrementTime idx
                                        let actualDepth =  Operators.max   (linearPath idx)   (exponentialPath idx )
                                        actualTime   , actualDepth )

let generateAscentStrategyGen (initState:State<LEStatus>) (solutionParams:double[])  controlTime = 
    let bottomTime , maximumDepth =  initState2TimeAndDepthGen initState
    
    let breakFractionUnconstrained = solutionParams.[0]
    let breakFraction =   Operators.max ( Operators.min  breakFractionUnconstrained   1.0) 0.0 
    
    let exponent = solutionParams.[1]
    let deltaTimeToSurface =  solutionParams.[2] 

    let linearAscent =  getSeqOfDepthsForLinearAscentSection  (bottomTime, maximumDepth)  MissionConstraints.ascentRateLimit (breakFraction*maximumDepth) controlTime
    let exponentialPart = getExponentialPartGen linearAscent deltaTimeToSurface exponent (bottomTime, maximumDepth ) controlTime

    seq { yield! linearAscent 
          yield! exponentialPart}

let generateBounceDiveStrategyGen (initState:State<LEStatus>)  controlTime = 
    let initDepthTime =  initState2TimeAndDepthGen initState
    let atSurfaceDepth = 0.0
    controlTime
    |> getSeqOfDepthsForLinearAscentSection  initDepthTime  MissionConstraints.ascentRateLimit atSurfaceDepth


let getSimParams (integrationTime, controlToIntegration)  (bottomTime, maximumDepth , pDCS )   = 
    let controlTime = controlToIntegration 
                      |> float 
                      |> (*) integrationTime

    let initState , myEnv = initStateAndEnvDescent maxSimTime (integrationTime, controlToIntegration) maximumDepth bottomTime
    let residualRiskBound = pDCSToRisk pDCS
    initState, residualRiskBound , myEnv  ,  controlTime 


let inline  (!!!) (x:float[]) :InArray<float>  = Array.map float32 x 
                                                 |> InArray.op_Implicit
    

let defineFcnForOptimizer (fcn: float[] -> float) =
    let fcn' (x:InArray<double>) = 
        use _scope = Scope.Enter(x)
        
        let internalVec = [| 0L .. (x.Length - 1L) |]
                          |> Array.map (fun idx -> x.GetValue(idx)  )
                          
        let out = fcn internalVec 
        out
        |> RetArray<double>.op_Implicit
    ObjectiveFunction fcn'


let getAnalyticalCostForThisDive  initState myEnv residualRiskBound controlTime  (solutionParams:double[]) =  
    let deltaTimeToSurface = solutionParams.[2]
    let ascentStrategyForGivenParams = generateAscentStrategyGen initState   solutionParams controlTime  
    let strategyResults = getTimeAndAccruedRiskForThisStrategy myEnv initState ascentStrategyForGivenParams
    let residualRisk = residualRiskBound - strategyResults.TotalRisk 
    (deltaTimeToSurface, residualRisk , ascentStrategyForGivenParams)

let surfaceWithPenaltyGenFcn initState myEnv residualRiskBound controlTime = 
    
    let penaltyWeight = 1.0e3
    let getRiskPenalty residualRisk  = 
        if residualRisk > 0.0 then
            0.0
        else
            (residualRisk * 100.0 ) ** 2.0 
            |> (*) penaltyWeight

    let actualFcn (solutionParams:double[]) = 
        
        let timeToSurf, residualRisk , _  = getAnalyticalCostForThisDive  initState myEnv residualRiskBound controlTime solutionParams
        let riskPenalty = getRiskPenalty residualRisk  
         
        timeToSurf + riskPenalty
        
    actualFcn 


let findOptimalAscentGen (integrationTime, controlToIntegration) (bottomTime, maximumDepth, pDCS) (initialGuess:double[]) =
    let initState, residualRiskBound, myEnv, controlTime  = getSimParams (integrationTime, controlToIntegration) (bottomTime, maximumDepth, pDCS)
    
    let bounceStrategy   = generateBounceDiveStrategyGen initState  controlTime
    let bounceDiveResult = bounceStrategy |> 
                           getTimeAndAccruedRiskForThisStrategy myEnv initState

    let residualRisk = residualRiskBound -  bounceDiveResult.TotalRisk

    if residualRisk >= 0.0 then
        bounceDiveResult , None 
    else
        let ascentObjFcn = surfaceWithPenaltyGenFcn initState myEnv residualRiskBound controlTime
        let objectiveFunctionIL = defineFcnForOptimizer ascentObjFcn
        let optimalParams = fmin(objectiveFunctionIL  , !!!initialGuess)
                            |> Seq.map float
                            |> Seq.toArray

        let optimalStrategy = generateAscentStrategyGen initState optimalParams controlTime
        let optimizedDiveResult = optimalStrategy
                                  |> getTimeAndAccruedRiskForThisStrategy myEnv initState
        optimizedDiveResult , Some optimalParams
    
let simulateStratWithParams (integrationTime, controlToIntegration) (initCond:float[]) deltaTimeToSurface  (breakFractExpVec:float[])    =
    let bottomTime = initCond.[0]
    let maximumDepth = initCond.[1]
    let pDCS = initCond.[2]
    let simParams = Array.append breakFractExpVec [|deltaTimeToSurface|]
    let initState, _ , myEnv, controlTime  = getSimParams (integrationTime, controlToIntegration) (bottomTime, maximumDepth, pDCS)
    let optimalStrategy = generateAscentStrategyGen initState simParams controlTime
    //let strategyResult = 
    {AscentResults = getTimeAndAccruedRiskForThisStrategy myEnv initState optimalStrategy 
     AscentParams = { BreakFraction = breakFractExpVec.[0]
                      Exponent      = breakFractExpVec.[1]
                      TimeToSurface = deltaTimeToSurface} }

let create3DGrid (seqBreakFractions:seq<float>) (seqExponents:seq<float>) (seqDeltaTimeToSurface:seq<float>) = 
    seq{ for breakFraction in seqBreakFractions do
            for exponent in seqExponents do
                for deltaTimeToSurface in seqDeltaTimeToSurface -> [|breakFraction ; exponent; deltaTimeToSurface|] }
    |> Seq.toArray

let create2DGrid (breakFracSeq:seq<float>)  (seqExponents :seq<float>)  = 
    seq { for breakFraction in breakFracSeq do
            for exponent in seqExponents  -> [|breakFraction ; exponent|] }

let resultsToArray (inputVec:float[], result:StrategyResults) =
    (inputVec.[0], inputVec.[1], inputVec.[2], result.AscentTime, result.AscentRisk, result.SurfaceRisk,
     result.TotalRisk, result.InitTimeAtSurface)

let hasExceededMaxRisk maxAllowedRisk (s:SimulationResults)  = 
    s.AscentResults.TotalRisk > maxAllowedRisk 

let getLastIfValid maxAllowedRisk (strategyRes : Option<SimulationResults> )  = 
    match strategyRes with
    | None -> None
    | Some s -> if ( s |> (hasExceededMaxRisk maxAllowedRisk) ) then
                    None
                else
                    Some s

//let getOptimalForThisInputCondition  paramsGrid (integrationTime, controlToIntegration) (bottomTime, maximumDepth, pDCS) =
//    let maxAllowedRisk = pDCSToRisk pDCS
//    paramsGrid
//    |> getAllSolutionsForThisProblem  (integrationTime, controlToIntegration) (bottomTime, maximumDepth, pDCS)
//    |> Array.zip paramsGrid 
//    |> Array.filter (fun  (inputVec, result )  -> result.TotalRisk < maxAllowedRisk )
//    |> Array.sortBy ( fun (inputV, res) -> res.AscentTime)
//    |> Array.map resultsToArray

//breakParmas is the grid of internal params (break , exp) 

//  let initCondition = [| bottomTime; maximumDepth; pDCS|] 

//(fun (x:StrategyResults) -> x.TotalRisk <= maxAllowedRisk) )


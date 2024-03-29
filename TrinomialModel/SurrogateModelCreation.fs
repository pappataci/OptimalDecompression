﻿[<AutoOpen>]
module SurrogateModel
open FSharp.Stats.Interpolation

let deltaPressure = 0.001

let numberOfTissues = (modelParams.Gains |> Seq.length )
let allTissues = [|0 ..  ( numberOfTissues - 1 ) |]
let surfaceAmbientCondition = depth2AmbientCondition 0.0

let getPressureRangesForTissue tables tissueIdx = 
    let getRangeFromOrdered (x:'T[]) = 
         [|Array.head; Array.last|]
         |> Array.map (fun f -> f x)
    
    let getPressuresOfThisTissueFromTMM mission = 
         mission.InitAscentNode.TissueTensions.[tissueIdx]

    tables
    |> Array.sortBy getPressuresOfThisTissueFromTMM
    |> getRangeFromOrdered
    |> Array.map getPressuresOfThisTissueFromTMM

let createPressureForTissueNValue tissueIdx value = 
    let zeroRiskPressure = surfaceAmbientCondition.Nitrogen 
                            |> Array.create numberOfTissues
    zeroRiskPressure.[tissueIdx] <- value
    zeroRiskPressure

let createSurfInitNodeForTissueWithValueAtDepth depth tissueIdx pressValue = 
    let envInfo = {Depth = depth; Time = 0.0}
    let tissueTensions = createPressureForTissueNValue tissueIdx pressValue
    let dummyZeroes = Array.zeroCreate numberOfTissues

    {EnvInfo = envInfo
     MaxDepth = 0.0
     AscentTime = 0.0
     TissueTensions = tissueTensions
     ExternalPressures = depth2AmbientCondition  depth
     InstantaneousRisk = dummyZeroes
     IntegratedRisk = dummyZeroes
     IntegratedWeightedRisk = dummyZeroes
     AccruedWeightedRisk = dummyZeroes
     TotalRisk  = 0.0}

let createSurfInitNodeForTissueWithValue = 
    createSurfInitNodeForTissueWithValueAtDepth 0.0 

let createGridPressureForTissue tables iTissue = 

    let ranges  =  allTissues
                        |> Array.map (getPressureRangesForTissue tables)

    let minPressure = surfaceAmbientCondition.Nitrogen
    let maxPressure = ranges.[iTissue].[1] 
    [| minPressure .. deltaPressure .. maxPressure|]

let createSolutionForTissue tables iTissue = // this could be done with a linear algo
    let actualGrid = createGridPressureForTissue tables  iTissue
    
    let solveSinglePressure = (createSurfInitNodeForTissueWithValue iTissue)
                              >> runModelUntilZeroRisk
    actualGrid
    |> Array.Parallel.map solveSinglePressure

let mappingForThisTissue tables = (createSolutionForTissue tables)
                                    >> Array.map (fun x-> x.TotalRisk)

let createPressureToRiskData tables =
    let pressureGrids = allTissues
                         |> Array.map (createGridPressureForTissue tables)
    let pressureToRiskMap = allTissues
                            |> Array.Parallel.map (mappingForThisTissue tables)
    {PressureGrid = pressureGrids
     RiskEstimate = pressureToRiskMap}

let getPressureGridFromDisk  = 
    readObjFromFile<SurfacePressureData> 

let getInterpolatingFunction (pressureGrid:double[])  (pressureToRisk:double[])=
    LinearSpline.initInterpolate pressureGrid pressureToRisk
    |> LinearSpline.interpolate

let createInterpolatingRiskFcns (surfacePressureData:SurfacePressureData) = 
    Array.map2  getInterpolatingFunction surfacePressureData.PressureGrid surfacePressureData.RiskEstimate

//let optionToInterpolatingFcns(x : Option<(float -> float)[] > ) : (float -> float)[] =
//    match x with
//    | Some y -> y
//    | None ->  printf "Problems in interpolating function: setting to null function"
//               (fun _ -> 0.0)
//               |> Array.create numberOfTissues

let runModelUntilZeroRiskSurrogate interpolatingRiskFcns (nodeAtSurface:Node)=
    let initTensions = nodeAtSurface.TissueTensions
    let riskPrediction = Array.map2 (<|) interpolatingRiskFcns initTensions
    let accruedWeigthedRisk = Array.map2 (+) nodeAtSurface.AccruedWeightedRisk riskPrediction
    let totalRiskAtSurface = riskPrediction |> Array.sum
    {    EnvInfo = {Depth = 0.0 ; Time = nodeAtSurface.EnvInfo.Time}
         MaxDepth = nodeAtSurface.MaxDepth
         AscentTime = nodeAtSurface.AscentTime
         TissueTensions  = Array.create numberOfTissues surfaceAmbientCondition.Nitrogen 
         ExternalPressures  = surfaceAmbientCondition
         InstantaneousRisk = riskPrediction
         IntegratedRisk = riskPrediction
         IntegratedWeightedRisk = riskPrediction
         AccruedWeightedRisk = accruedWeigthedRisk
         TotalRisk = nodeAtSurface.TotalRisk + totalRiskAtSurface }

let createRunningUntilZeroRiskSurrogateFromDisk = getPressureGridFromDisk 
                                                  >> Option.map ( createInterpolatingRiskFcns 
                                                              >> runModelUntilZeroRiskSurrogate )
                                              //>> Option.map 
                                              //>> optionToInterpolatingFcns

//let createRunningUntilZeroRiskSurrogateFromDisk = createPressureRiskInterpolatorsFromDisk
//                                                  >> runModelUntilZeroRiskSurrogate
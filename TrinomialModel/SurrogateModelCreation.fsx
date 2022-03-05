#r @"C:\Users\glddm\.nuget\packages\newtonsoft.json\13.0.1\lib\net45\Newtonsoft.Json.dll"
#r @"C:\Users\glddm\.nuget\packages\fsharp.data\3.3.3\lib\net45\FSharp.Data.dll"
#r @"C:\Users\glddm\.nuget\packages\fsharp.stats\0.4.3\lib\netstandard2.0\FSharp.Stats.dll"

#load "SeqExtension.fs"
#load "Gas.fs"
#load "ELModelCommon.fs"
#load "TrinomialModel.fs"
#load "TableDataInputs.fs"
#load "TableReader.fs"
#load "ProfileIntegrator.fs"
#load "MissionDefinerFromTables.fs"
#load "MissionSerializer.fs"
#load "TableToDiscreteActionsSeq.fs"
#load "TrinomialModelToPython.fs"


open TrinomialModToPython.ToPython



let tables , _ = getTables()


let getPressureRangesForTissue tissueIdx = 
    let getRangeFromOrdered (x:'T[]) = 
         [|Array.head; Array.last|]
         |> Array.map (fun f -> f x)
    
    let getPressuresOfThisTissueFromTMM mission = 
         mission.InitAscentNode.TissueTensions.[tissueIdx]

    tables
    |> Array.sortBy getPressuresOfThisTissueFromTMM
    |> getRangeFromOrdered
    |> Array.map getPressuresOfThisTissueFromTMM


let numberOfTissues = (modelParams.Gains |> Seq.length )

let allTissues = [|0 ..  ( numberOfTissues - 1 ) |]

let ranges =  allTissues
              |> Array.map getPressureRangesForTissue

let surfaceAmbientCondition = depth2AmbientCondition 0.0


let createPressureForTissueNValue tissueIdx value = 
    let zeroRiskPressure = surfaceAmbientCondition.Nitrogen 
                            |> Array.create numberOfTissues
    zeroRiskPressure.[tissueIdx] <- value
    zeroRiskPressure


let createSurfInitNodeForTissueWithValue tissueIdx pressValue = 
    let envInfo = {Depth = 0.0; Time = 0.0}
    let tissueTensions = createPressureForTissueNValue tissueIdx pressValue
    let dummyZeroes = Array.zeroCreate numberOfTissues

    {EnvInfo = envInfo
     MaxDepth = 0.0
     AscentTime = 0.0
     TissueTensions = tissueTensions
     ExternalPressures = surfaceAmbientCondition
     InstantaneousRisk = dummyZeroes
     IntegratedRisk = dummyZeroes
     IntegratedWeightedRisk = dummyZeroes
     AccruedWeightedRisk = dummyZeroes
     TotalRisk  = 0.0}
    
let deltaPressure = 0.001

let createGridPressureForTissue iTissue = 
    let minPressure = surfaceAmbientCondition.Nitrogen
    let maxPressure = ranges.[iTissue].[1] 
    [| minPressure .. deltaPressure .. maxPressure|]


let createSolutionForTissue iTissue = // this could be done with a linear algo
    let actualGrid = createGridPressureForTissue  iTissue
    
    let solveSinglePressure = (createSurfInitNodeForTissueWithValue iTissue)
                              >> runModelUntilZeroRisk
    actualGrid
    |> Array.Parallel.map solveSinglePressure

let mappingForThisTissue = createSolutionForTissue
                           >> Array.map (fun x-> x.TotalRisk)

let pressureGrids = allTissues
                    |> Array.map createGridPressureForTissue

let pressureToRiskMap = allTissues
                        |> Array.Parallel.map mappingForThisTissue

open FSharp.Stats.Interpolation


let getInterpolatingFunction (pressureGrid:double[])  (pressureToRisk:double[])=
    LinearSpline.initInterpolate pressureGrid pressureToRisk
    |> LinearSpline.interpolate


let interpolatingRiskFcns = 
    Array.map2  getInterpolatingFunction pressureGrids pressureToRiskMap


let runModelUntilZeroRiskSurrogagte (nodeAtSurface:Node)=
    let initTensions = nodeAtSurface.TissueTensions

    let riskPrediction = Array.map2 (fun f x -> f x) interpolatingRiskFcns initTensions
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


let outputData = {PressureGrid = pressureGrids
                  RiskEstimate = pressureToRiskMap}

// trying to save to disk and get it back
dumpObjectToFile outputData surfacePressureFileName

let getPressureGridFromDisk  = 
    readObjFromFile<SurfacePressureData> 

let testPressData  = getPressureGridFromDisk surfacePressureFileName
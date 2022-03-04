#r @"C:\Users\glddm\.nuget\packages\newtonsoft.json\13.0.1\lib\net45\Newtonsoft.Json.dll"
#r @"C:\Users\glddm\.nuget\packages\fsharp.data\3.3.3\lib\net45\FSharp.Data.dll"

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

open ModelRunner
 

let shallowInitConditions = getMapOfDenseInitConditions() minDepthForStop

let getPressureValuesFrommission mission tissueIdx = 
    mission.InitAscentNode.TissueTensions.[tissueIdx]




let getPressureRangesForTissue tissueIdx = 

    let getRangeFromOrdered (x:'T[]) = 
         [|Array.head; Array.last|]
         |> Array.map (fun f -> f x)
    
    let getPressuresOfThisTissueFromTMM mission = 
         mission.InitAscentNode.TissueTensions.[tissueIdx]

    shallowInitConditions
    |> Array.sortBy getPressuresOfThisTissueFromTMM
    |> getRangeFromOrdered
    |> Array.map getPressuresOfThisTissueFromTMM


let ranges = [|0 .. 2|]
                |> Array.map getPressureRangesForTissue


let zeroRiskPressure = (depth2AmbientCondition 0.0).Nitrogen 
                       |> Array.create 

let setTensionsOfTissueWithOtherNoRiskBearing tissueIdx = 
    

// check that at minimum value we get zero risk
let createSurfNodeForTissueWithValue tissueIdx pressValue = 
     
        let envInfo = {Depth = 0.0; Time = 0.0}
        let tensions = [|t0;t1;t2|]
                        
         
        let dummyZeroes : float[]= Array.zeroCreate (modelParams.Gains|>Seq.length )

        {EnvInfo = envInfo
         MaxDepth = dpth
         AscentTime = 0.0
         TissueTensions = tensions
         ExternalPressures = depth2AmbientCondition dpth 
         InstantaneousRisk = dummyZeroes
         IntegratedRisk = dummyZeroes
         IntegratedWeightedRisk = dummyZeroes
         AccruedWeightedRisk = dummyZeroes
         TotalRisk = totRisk}

let createPressureToSurfRiskApproximator   :  double[] -> double =
    

    fun x -> 0.0
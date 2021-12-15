#r @"C:\Users\glddm\source\repos\DecompressionL20190920A\packages\FSharp.Data.3.3.3\lib\net45\FSharp.Data.dll"
#load "Logger.fs"
#load "SeqExtension.fs"
#load "Gas.fs"
#load "ELModelCommon.fs"
#load "TrinomialModel.fs"
#load "TableDataInputs.fs"
#load "TableReader.fs"
#load "ProfileIntegrator.fs"
#load "MissionDefinerFromTables.fs"
#load "SurfaceTableCreator.fs"
#load "Diagnostics.fs"
#load "TableToDiscreteActionsSeq.fs"
#load "FromPythonImporter.fs"
open ModelRunner

//let vectorsOfActions = Array.map2 getAscentProfileFromSingleDepthProfile initialConditions  depthProfiles
//                       |> Array.map toVectorOfActions



//let initialConditions, vectorsOfActions = getTableInitialConditionsAndTableStrategies table9FileName

let tableFileName = table9FileName
let initialConditions, depthProfiles = getTableOfInitialConditions tableFileName

let offendingProfileLbl = 282
let initCond = initialConditions.[offendingProfileLbl]
let depthProf = depthProfiles.[offendingProfileLbl] |> Seq.toArray

let ascentProfile = getAscentProfileFromSingleDepthProfile initCond depthProf



toVectorOfActions ascentProfile // has a bug, as expected

// try to isolate just the two last steps (assuming everything else is functional)
//let ascentProfVec = ascentProfile|>Seq.toArray

//let prevDepth, actDepth = ascentProfVec.[2..]
//                           |> (fun x -> x.[0].Depth , x.[1].Depth)
                           
//getActionForAscent  prevDepth actDepth
open FSharp.Data
let profileFolder = @"C:\Users\glddm\Documents\Duke\Research\OptimalAscent\repos\PythonRLOptimalAscent\data\rl_tables"


type profileReader = CsvProvider< @"C:\Users\glddm\Documents\Duke\exampls.csv", HasHeaders=true>

let profileExample = profileReader.Load(@"C:\Users\glddm\Documents\Duke\exampls.csv")

let rowContentToDepthTime (rowContent :profileReader.Row) = {Time = rowContent.Time |> double
                                                             Depth = rowContent.Depth |> double  } 

let profileReaderToProfile (profile:profileReader) =    profile.Rows 
                                                        |> Seq.map rowContentToDepthTime
                       

let getProfileFileName (idx:int) = 
    profileFolder + @"\profile" + string(idx) + ".csv"

let getProfileFromRL idx =
    idx 
    |> getProfileFileName
    |> (fun  fileName ->  profileReader.Load(fileName) )
    |> profileReaderToProfile


let maxProfileNumber = 149

let profilesFromRL = [0..maxProfileNumber]
                     |> List.map getProfileFromRL
                     |> List.toArray

let hasCompletedTheMission envInfo =
    envInfo
    |> Seq.last
    |> (fun x -> x.Depth < 1.0E-5 )


type AscentResult = {AscentTime : double
                     FinalRisk : double}   

let computeAscentResultForValidProfile (aProfile:seq<DepthTime> ) (initNode: Node) = 
    runModelOnProfile initNode aProfile
    |> Seq.last
    |> (fun lastNode -> { AscentTime =  lastNode.AscentTime
                          FinalRisk = lastNode.TotalRisk } )

    

let getMissionMetricsFromRL aProfile initNode : Option<AscentResult> =
    match hasCompletedTheMission aProfile with
    | true -> computeAscentResultForValidProfile  aProfile initNode
              |> Some
    | false -> None

let initialNodes = initialConditions
                   |> Seq.map ( fun x -> x.InitAscentNode)
                   |> Seq.take maxProfileNumber

let RLMetrics = Seq.map2  getMissionMetricsFromRL profilesFromRL initialNodes


type TableComparison = 
                        {RLRiskPercentage: float
                         TimeAbsAdvantage : float
                         TimePercAdvantage: float}

let compareProfileWithTable (ascentResult:AscentResult) (tableResults:TableMissionMetrics) =
    let rlRiskPercentage = ascentResult.FinalRisk / tableResults.TotalRisk * 100.0
    let timeAbsAdvantage = ascentResult.AscentTime - tableResults.MissionInfo.TotalAscentTime
    let timePercAdvantage = timeAbsAdvantage / tableResults.MissionInfo.TotalAscentTime * 100.0
    {RLRiskPercentage = rlRiskPercentage;
     TimeAbsAdvantage = timeAbsAdvantage
     TimePercAdvantage = timePercAdvantage}


let compareTableWithRL (rlMetrics:Option<AscentResult>) (tableResults:TableMissionMetrics) = 
    match rlMetrics with
    | Some ascentResult -> compareProfileWithTable ascentResult tableResults
                           |> Some
    | None -> None 

let comparisons = Seq.map2 compareTableWithRL RLMetrics  ( initialConditions |> Array.take maxProfileNumber)

let comparisonsVec = comparisons
                        |> Seq.toArray


let test= comparisonsVec.[0..149]
        |> Seq.filter (fun  x -> match x with 
                                 | Some x -> true 
                                  | None -> false)

test |> Seq.length ;;

let t = test |> Seq.map (fun (Some x ) -> x) |> Seq.toArray

let t'' = t |> Seq.filter (fun x -> x.TimeAbsAdvantage > 1.0) |> Seq.toArray
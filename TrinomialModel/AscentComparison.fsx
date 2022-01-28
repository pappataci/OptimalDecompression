#r @"C:\Users\glddm\.nuget\packages\fsharp.data\3.3.3\lib\net45\FSharp.Data.dll"
#r @"C:\Users\glddm\.nuget\packages\newtonsoft.json\13.0.1\lib\net45\Newtonsoft.Json.dll"
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

open ModelRunner
open FSharp.Data
type profileReader  = CsvProvider<"exampls.csv">

let maxProfileNumber = 149

let generatedProfilesIdx = [|0 .. maxProfileNumber|]

let profileFolder = @"C:\Users\glddm\Documents\Duke\Research\OptimalAscent\repos\PythonRLOptimalAscent\data\rl_tables\"

let getAscentDepthTimeFromProfile idxNumber = 
    let profileName = profileFolder + "profile" + (string idxNumber) + ".csv"
    let content = profileReader.Load profileName
    content.Rows
    |> Seq.map (fun r -> {Depth = (float r.Depth) 
                          Time = (float r.Time)} )

open TrinomialModToPython.ToPython
let tables , _  = getTables() // this is used to have the initial conditions

let usedTables = tables.[0 .. maxProfileNumber]

let getSolutionProfileWIdx idx = 
    let initNode = tables.[idx].InitAscentNode
    let solution = runModelOnProfile initNode ( getAscentDepthTimeFromProfile idx )
    let lastNode = solution |> Seq.last 
    lastNode.TotalRisk , lastNode.AscentTime

let comparisons = generatedProfilesIdx
                  |> Array.map (fun idx -> (getSolutionProfileWIdx idx), (tables.[idx].TotalRisk, tables.[idx].MissionInfo.TotalAscentTime) )
                  |> Array.indexed

let goodTime (_,( (rlRisk, rlTime),(tableRisk, tableTime) )) =
    tableTime > rlTime - 0.1

let acceptableRisk (_,( (rlRisk, rlTime),(tableRisk, tableTime) )) =
    rlRisk <= 1.01 * tableRisk 

let timeAdvantage = comparisons
                    |> Seq.filter  goodTime
                    
let getPercentage x =  (x |> Seq.length |> float) / (float (maxProfileNumber + 1 ) ) * 100.0

let timeAdvantagePercentage = getPercentage timeAdvantage   

let acceptableRiskAscent = timeAdvantage 
                           |> Seq.filter  acceptableRisk

let advantegeousProfileIndexes = acceptableRiskAscent
                                 |> Seq.map (fun  (x,  _  ) -> x)

//runModelOnProfileUsingFirstDepthAsInitNode
let betterProfilesPercentage = getPercentage  acceptableRiskAscent



let allProfiles = generatedProfilesIdx
                  |> Array.map getAscentDepthTimeFromProfile
                  //|> Array.indexed

let goodAScents = advantegeousProfileIndexes
                  |> Seq.map (fun i -> allProfiles.[i])


let lengths = allProfiles |> Seq.map (fun x -> x |> Seq.length)

let indexWithLength = Seq.zip advantegeousProfileIndexes  lengths 


let usefulDataWithLength  = Seq.zip   acceptableRiskAscent lengths
                             |> Seq.filter ( fun (x, y)  ->  y < 100  )


let strictlyLessRisky = usefulDataWithLength
                        |> Seq.filter (fun ((x, ( (rlRisk,rlTime),(tRisk,tTime))),y) ->rlRisk <= tRisk )
                       
let advantage = strictlyLessRisky 
                |> Seq.map (fun ((x, ( (rlRisk,rlTime),(tRisk,tTime))),y) -> x , tTime - rlTime )

let advantageousIndeces = advantage  |> Seq.map fst


let bestProfiles = advantageousIndeces
                    |> Seq.map (fun index -> allProfiles.[index])

bestProfiles |> Seq.toArray

let bestProfileOverAll = advantage
                         |> Seq.maxBy ( fun (idx, adv) -> adv)

let bestProfilesWithTwoStopsIndeces = advantageousIndeces
                                       |>  Seq.filter (fun idx ->  allProfiles.[idx]
                                                                   |> Seq.item 1
                                                                   |> (fun {Depth = d} -> d > 20.0 ) )
let bestTwoStepProfileIdx = bestProfilesWithTwoStopsIndeces |> Seq.head
                  

// most advantageous overall is profile 93;
// best with two stops is profile 89

let tableContent = table9FileName
                    |>getDataContent

let bestProfileIndexOverall = bestProfileOverAll |> fst

let profileOfInterest = [bestProfileIndexOverall ; bestTwoStepProfileIdx]

let tableDataCorrespondentToBestProfiles = profileOfInterest
                                            |> Seq.map (fun idx ->data2SequenceOfDepthAndTime tableContent.[idx]  )

let externalFolder = @"C:\Users\glddm\Documents\Duke\Research\ReportJan26_22\"

let buildRow {Time = t ; Depth = d} = 
    profileReader.Row(decimal  t, decimal d)

let writeAscentToDisk (prefix:string) (ascent:seq<DepthTime> , missionInfo : MissionInfo)   = 
    
    

    let getOutputfileName (m:MissionInfo) = 
        externalFolder + prefix + 
        "ascentMaxDepth" + (string m.MaximumDepth) +
        "BottomTime" + (string m.BottomTime) +  
        ".csv"

    let outputName = getOutputfileName missionInfo
    ascent
    |> Seq.map buildRow
    |> (fun x -> new profileReader(x) )     
    |> ( fun profileSaver -> profileSaver.Save(outputName)  )


let writeRLAscentToDisk :seq<DepthTime>*MissionInfo -> unit = writeAscentToDisk "RL"
let writeTableAscentToDisk  :seq<DepthTime>*MissionInfo -> unit= writeAscentToDisk "Table"

let includeAscentFromTable (tableStrategy:seq<DepthTime> , missionInfo:MissionInfo) ascentProfile :seq<DepthTime> = 
    
    let ascentPart = tableStrategy
                     |> Seq.filter (fun x -> x.Time < missionInfo.BottomTime)

    let ascentProfileTraslated = ascentProfile
                                 |> Seq.map (fun p -> {p with Time = p.Time + missionInfo.BottomTime })

    Seq.concat [ascentPart; ascentProfileTraslated]

let rlBestProfiles = profileOfInterest 
                     |> Seq.map (fun idx -> let ascentProfileContent =  allProfiles.[idx] 
                                            let tableInfo = data2SequenceOfDepthAndTime tableContent.[idx] 
                                            let completeProfile = includeAscentFromTable tableInfo  ascentProfileContent
                                            completeProfile , tableInfo |> snd)

// write best profiles to disk
rlBestProfiles
|> Seq.toArray
|> Array.map writeRLAscentToDisk

tableDataCorrespondentToBestProfiles
|> Seq.toArray
|>Array.map writeTableAscentToDisk


//Sanity check

let getOutputNodes profiles = profiles
                              |> Seq.map ( fun (ascentStrategy, _) -> runModelOnProfileUsingFirstDepthAsInitNode ascentStrategy) 

let getLastSolutionNode : seq<DepthTime>*MissionInfo -> Node = fst 
                                                               >> runModelOnProfileUsingFirstDepthAsInitNode 
                                                               >> Seq.last

let rlSolutions = rlBestProfiles
                    |> Seq.map getLastSolutionNode
                    |> Seq.toArray

let tableSolutions = tableDataCorrespondentToBestProfiles
                     |> Seq.map getLastSolutionNode
                     |> Seq.toArray


// production of risk function for profile 93 

let completeBestProfile, optProfTableMissionInfo = rlBestProfiles |> Seq.item 0 

let completeBestProfileSolution = runModelOnProfileUsingFirstDepthAsInitNode completeBestProfile

let initialNode = completeBestProfileSolution 
                  |> Seq.filter (fun n -> n.EnvInfo.Time > 223.0 && n.EnvInfo.Time < 224.0)
                  |> Seq.head

let initNodeTime = initialNode.EnvInfo.Time

// create sequence of missions
let missions = [|1.0 .. 110.0|]
                |> Array.map ( fun timeIncrement -> let firstTime =  initNodeTime + timeIncrement
                                                    let surfaceTime = firstTime + 2.0/3.0
                                                    [{Time = firstTime ; Depth = 20.0} ; 
                                                      {Time = surfaceTime ; Depth = 0.0} ]
                                                     |> List.toSeq) 

// test 
let r1 = runModelOnProfile initialNode  missions.[1] |> Seq.last |> (fun n -> n.TotalRisk)

let riskValues = missions
                |> Array.Parallel.map (fun strategy ->strategy
                                                      |> runModelOnProfile initialNode
                                                      |> Seq.last 
                                                      |> (fun n -> n.TotalRisk) )
                 
let toOutput = Array.map2 (fun m r -> {Time = m ; Depth= r} )  [|1.0 .. 110.0|] riskValues

toOutput
|> Seq.map buildRow
|> (fun x -> new profileReader(x) )     
|> ( fun profileSaver -> profileSaver.Save(@"C:\Users\glddm\Documents\Duke\Research\ReportJan26_22\riskValues.csv")  )

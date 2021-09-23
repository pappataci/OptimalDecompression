[<AutoOpen>]
module TableReader
open System.IO

type TimeMinSec = {Minutes: int
                   Seconds: int}

type GasType = | Air
               | Unknown

type AirTable = { MaximumDepth: double
                  BottomTime : double
                  TimeToFirstStop: TimeMinSec
                  GasMix : GasType
                  StopTimes: double[] 
                  TotalAscentTime: TimeMinSec
                  ChamberO2Periods: double
                  RepetGroup: string option}

type AnsData = { Header: string 
                 NodeSeq: seq<DepthTime> }

type MissionInfo = { MaximumDepth: double
                     BottomTime : double
                     TotalAscentTime : double}

let getDataContent  fileName = 
    let completeFileName = dataSrcFolder  + @"\" + fileName

    try 
        File.ReadAllLines completeFileName
    with
       | :? DirectoryNotFoundException as ex -> printfn "Exception! %s " (ex.Message); [|""|]
       | :? FileNotFoundException as ex -> printfn "File Not Found %s " (ex.Message) ; [|""|]

let getGasType (s:string) =
    match s.ToLower() with 
    | "air" -> Air
    | _ -> Unknown

let string2MinutesSeconds (s:string) =
    let minsSec = s.Split(':')
    {Minutes = int minsSec.[0]
     Seconds = int minsSec.[1]}

let minutesSecToFloat {Minutes = mins ; Seconds = secs} = 
    let m = double mins
    let s = (double secs) 
    m + s / 60.0

let getStopTimesNLength (s:string[]) = 
    let index = s 
                |> Array.findIndex (fun x -> x.Contains(":") )
    s.[0..index-1]
    |> Array.map double , index

let getRepetGroup (s:string[]) = 
    let sLength = s |> Array.length
    if sLength = 2 then Some (s|>Array.last)
    else None

let string2TableInfo (s:string) : AirTable= 
    let components = s.Split(' ')
                     |> Array.filter (fun x -> not (x = "") )
    let maxDepth = double components.[0]
    let bottomTime = double components.[1]
    let timeToFirstStop = string2MinutesSeconds components.[2]
    let gasType = getGasType components.[3]
    let stopTimes , numberOfElementsToSkip = getStopTimesNLength components.[4..]
    let totalAscentTime = string2MinutesSeconds components.[ 4 + numberOfElementsToSkip]
    let chamberO2Periods = double components.[5 + numberOfElementsToSkip]
    let repeatGroup = getRepetGroup components.[5+numberOfElementsToSkip..]

    { MaximumDepth = maxDepth
      BottomTime = bottomTime
      TimeToFirstStop = timeToFirstStop
      GasMix = gasType
      StopTimes = stopTimes
      TotalAscentTime = totalAscentTime
      ChamberO2Periods = chamberO2Periods
      RepetGroup= repeatGroup}

let defineDepthAndTime (depth, time) = 
    {Depth = depth
     Time = time }

let createDescentNodes ({MaximumDepth = maxDepth} : AirTable) =
    seq{
        defineDepthAndTime (0.0, 0.0)
        defineDepthAndTime ( maxDepth, maxDepth/descentRate )
        }

let createMaxDepthNode ({MaximumDepth = maxDepth; BottomTime = bottomTime} : AirTable) (actualSeq:seq<DepthTime>) =
    seq{
        defineDepthAndTime(maxDepth, bottomTime) 
        }

let addEndNodeAtStopDepth  (timeAtConstantDepth:double) lastTime (nextDepth:double)  = 
    if (timeAtConstantDepth > 0.0 ) && (nextDepth >= 1.0e-7) then
        seq{defineDepthAndTime(nextDepth, lastTime + timeAtConstantDepth ) }
    else
        Seq.empty

let createNodesOfFirstStop {StopTimes = stopTimes; TimeToFirstStop = timeToFirstStop} (nodeSeq:seq<DepthTime>) =
    let {Depth = lastDepth; Time = lastTime}  = nodeSeq |> Seq.last
    let ascentTime = minutesSecToFloat timeToFirstStop
    let nextDepth = lastDepth + ascentRate *  ascentTime
    let nextTime = lastTime + ascentTime
    let nodeAtFirstStopStart = defineDepthAndTime(nextDepth, nextTime)
    seq{ yield nodeAtFirstStopStart
         yield! addEndNodeAtStopDepth stopTimes.[0] nextTime  nextDepth }

let createNodesToStartOfAscent (airTable:AirTable)  =
    let descentNodes  = createDescentNodes airTable
    let maxDepthNode = createMaxDepthNode airTable descentNodes
    seq{ yield! descentNodes
         yield! maxDepthNode}

let defineAscentMiddleNodes nodeSeq totalWaitingTime  = 
    let {Depth = lastDepth; Time = lastTime}  = nodeSeq |> Seq.last 
    let ascentTime = defaultAscentStep / ascentRate
    let targetDepth = lastDepth + defaultAscentStep
    let firstNodeAtLevel = defineDepthAndTime( targetDepth, lastTime + ascentTime ) 
    let secondNodeAtLevel = defineDepthAndTime ( targetDepth, lastTime + totalWaitingTime)
    seq{ yield firstNodeAtLevel
         yield secondNodeAtLevel  }

let insertMiddleNodes aTable ascentNodes   = 
    let lastNodeIsAtSurface nodesSeq =
        let {Depth = lastDepth; Time = lastTime}  = nodesSeq |> Seq.last 
        lastDepth < 1.0e-7

    let middleSeq = aTable.StopTimes.[1..]
                  |> Array.scan defineAscentMiddleNodes ascentNodes
                  |> Seq.concat
    if Seq.isEmpty middleSeq  || lastNodeIsAtSurface middleSeq then 
        Seq.empty
    else
        middleSeq
        |> Seq.skip 2

let createAscentUpToSurface aTable toAscentNodes = 
    let firstNodeAscents = createNodesOfFirstStop aTable toAscentNodes
    let middleNodes = insertMiddleNodes aTable firstNodeAscents
    seq {yield! firstNodeAscents
         yield! middleNodes}

let insertLastNode nodeSeq  =
    let {Depth = lastDepth; Time = lastTime}  = nodeSeq |> Seq.last 
    if lastDepth < 1.0e-7 then
        Seq.empty
    else
        let zeroDepth = 0.0
        let deltaTime = -lastDepth / ascentRate 
        seq{defineDepthAndTime(zeroDepth, deltaTime + lastTime)}

let getAscentParams (airTable:AirTable)  =
    { MaximumDepth = airTable.MaximumDepth 
      BottomTime = airTable.BottomTime
      TotalAscentTime =  minutesSecToFloat  airTable.TotalAscentTime}
    
let data2SequenceOfDepthAndTime (originalString:string) = 
    let airTable = string2TableInfo originalString
    let toAscentStart = createNodesToStartOfAscent airTable
    let ascentNodesToSurface = createAscentUpToSurface airTable toAscentStart
    let initSeq = Seq.concat ( seq{yield toAscentStart 
                                   yield ascentNodesToSurface} )
    let finalNode = insertLastNode  initSeq
    
    let ascentParams = getAscentParams airTable
    Seq.concat  ( seq{initSeq; finalNode} )  , ascentParams

let defNodeWithTensionAtDepthAndTime initDepthTime = // needed more generic function with initRisk and pressures

    let {Depth = initDepth; Time = initTime} = initDepthTime
    let externalPressures = depth2AmbientCondition initDepth
    let zeroVector = Array.zeroCreate modelParams.Gains.Length
    {EnvInfo = initDepthTime
     AscentTime = 0.0
     MaxDepth = 0.0
     TissueTensions = getTissueTensionsAtDepth externalPressures
     ExternalPressures = depth2AmbientCondition initDepth
     InstantaneousRisk = zeroVector
     AccruedWeightedRisk = zeroVector
     IntegratedRisk = zeroVector
     IntegratedWeightedRisk = zeroVector
     TotalRisk = 0.0}
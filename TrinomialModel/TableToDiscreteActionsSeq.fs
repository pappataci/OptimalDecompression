[<AutoOpen>]
module TableToDiscreteActionsSeq

let getAscentProfileFromSingleDepthProfile (initialCondition:TableMissionMetrics) (depthProfile:seq<DepthTime>) = 
    let timeTolerance = 1.0e-5
    depthProfile
    |> Seq.filter ( fun x -> x.Time >= initialCondition.MissionInfo.BottomTime - timeTolerance  ) // initial condition is included
    

let getAscentProfilesFromDepthProfiles (initialConditions:TableMissionMetrics[]) (depthProfiles:seq<DepthTime> [])  = 
    Array.map2 getAscentProfileFromSingleDepthProfile initialConditions depthProfiles


let getNumOfActsForConstantDepth (init:double) final =
    let decisionTime = 1.0
    (final - init) / decisionTime
    |> int

let getNumberOfActsForAscent (initDepth:double) finalDepth = 
    let depthStep = 10.0 // ft
    let numOfSteps = (initDepth - finalDepth)/depthStep
                   |> ceil
                   |> int 

    let correction = if ( finalDepth < 0.5 ) then
                        -1
                     else
                        0
    numOfSteps + correction

let getActionVector (getNumberOfActions:double->double->int) actionValue initLevel finalLevel = 
    let numberOfActions =  getNumberOfActions initLevel finalLevel
    Seq.init numberOfActions (fun _ -> actionValue)

let getActionConstantDepth =
    getActionVector getNumOfActsForConstantDepth 1.0

let getActionForAscent = 
    getActionVector getNumberOfActsForAscent 0.0

let getInternalSeq strategy = strategy   
                                |> Seq.pairwise
                                |> Seq.map (fun (prev, actual)  ->  let isAlmostEqualTo (x:float) y = abs(x-y) < 1.0e-3
                                                                    if (prev.Depth |> isAlmostEqualTo actual.Depth ) then 
                                                                       getActionConstantDepth  prev.Time actual.Time 
                                                                    else
                                                                       getActionForAscent prev.Depth actual.Depth  )
                                |> Seq.concat

let toVectorOfActions (strategy: seq<DepthTime> )  =   
    let internalSeq = getInternalSeq strategy
    seq {   yield! internalSeq}
    |> Seq.toArray

let getTableInitialConditionsAndTableStrategies tableFileName = 
    let initialConditions, depthProfiles = getTableOfInitialConditions tableFileName
    let vectorsOfActions = Array.map2 getAscentProfileFromSingleDepthProfile initialConditions  depthProfiles
                           |> Array.map toVectorOfActions
    initialConditions , vectorsOfActions
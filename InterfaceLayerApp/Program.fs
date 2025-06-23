open TrinomialModToPython.ToPython

let transitionStepFcnResLocal = stepFcnResidual
let transitionStepFcnSurrogateLoc = stepFcnSurrogateResidual
let nodeBuilder = createNode // (dpth, tm, t0,t1,t2,  totRisk)

[<EntryPoint>]
let main argv =
    let dpth, currentTime , t0, t1, t2, toRisk = 20.0, 0.0, 1.2, 0.95, 0.9, 1.0
    let initNode = nodeBuilder(dpth, currentTime, t0, t1, t2, toRisk)
    
    let nextTime, nextDepth = 0.1, 10.0

    let nextNode = stepFunction(initNode, nextTime, nextDepth)

    printfn "Init Node %A" initNode
    printfn "Next Node %A" nextNode  

    0 // return an integer exit code

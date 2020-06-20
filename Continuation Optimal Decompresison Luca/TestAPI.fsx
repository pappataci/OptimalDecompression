#load "Learner.fs"
#load "ReinforcementLearning.fs"
#load "Gas.fs"
#load "PredefinedDescent.fs"
#load "LEModel.fs"
#load "OptimalAscentLearning.fs"
#load "IOUtilities.fs"
#load "InputDefinition.fs"
#load "EnvironmentToPython.fs"

open ReinforcementLearning
open Gas
open InitDescent
open LEModel
open OptimalAscentLearning
open System
open ToPython


let maxPDCS = 3.3e-2
let penaltyForExceedingRisk = 5000.0
let integrationTime = 0.1 
let controlToIntegrationTimeRatio = 10

// MISSION PARAMETERS
let descentRate = 60.0
let maximumDepth = 120.0
let bottomTime = 30.0
let legDiscreteTime =  integrationTime


// Python equivalent helper function
let env, initState ,  ascentLimiter , nextAscLimit  =  getEnvInitStateAndAscentLimiter  ( maxPDCS    , 
                                                                           penaltyForExceedingRisk    , 
                                                                           integrationTime  ,
                                                                           controlToIntegrationTimeRatio,  
                                                                           descentRate , 
                                                                           maximumDepth  , 
                                                                           bottomTime  , 
                                                                           legDiscreteTime   )  

let answer = getNextEnvResponseAndBoundForNextAction(env, initState , 120.0, ascentLimiter)

let (nextState, transitionRew, isTerminalState , ascentRateLimit)  = answer 

let a = 0.0
        |> Array.create 150 
        |> Array.scan ( fun (actualState, _)  actualDepth -> getNextEnvResponseAndBoundForNextAction(env, actualState , actualDepth , ascentLimiter) 
                                                             |> (fun (x,_,_, z) -> (x , z)   ) ) (initState, ascentRateLimit) 

let (states, depths ) = ( a |> Array.unzip)
let lastState = states |> Array.last 

let (State lastStateContent ) = lastState

//let tensions = lastStateContent.LEPhysics.TissueTensions

//let noiseLevel = 0.02

//let perturbedTensions tensions noiseLevel = tensions
//                                            |> Array.map ( fun  x  ->  (new System.Random()).NextDouble() * noiseLevel
//                                                                        |> Tension
//                                                                        |> (+>) x  )


let perturbedState  =  perturbState (lastState , 0.00 , 1.0e3)

getTissueDepth
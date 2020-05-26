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

let initDepth = 0.0
//let descentParameters = {DescentRate = 60.0 ; MaximumDepth = 120.0; BottomTime = 30.0}

let discretizationTimeForLegs = 0.1 

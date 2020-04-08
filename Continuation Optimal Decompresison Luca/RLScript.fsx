//#load "Learner.fs"
#load "ReinforcementLearning.fs"
#load "PredefinedDescent.fs"
#load "Gas.fs"
#load "LEModel.fs"

open ReinforcementLearning
open InitDescent
open LEModel

let testDeltaT = 0.1

//let toyExample = discretizeConstantDescentPath initImmersionLeg testDeltaT
//                  |> Seq.take 5
//                  |> Seq.toArray // use array now for simplicity

type LEConstants = { CrossOvers             : float[] 
                     Rates                  : float[] 
                     ThalmanErrorHypothesis : bool    }
type Params<'P> = ModelParams of 'P
type LEModelParams = LEParams of Params<LEConstants>

type ExternalPressureConditions = { Ambient   : float
                                    Nitrogen  : float }

let crossover = [|     9.9999999999E+09   ;     2.9589519286E-02    ;      9.9999999999E+09    |]
let rates     = [| 1.0 / 1.7727676636E+00 ; 1.0 / 6.0111598753E+01  ;  1.0 / 5.1128788835E+02  |]
let defaultThalmanErrorIsTrue = true

type ActualParams = { XO : float[]; IntegrationTime: float; Rates: float[] }

let paramsExample = { XO = [|1.1|]  ; IntegrationTime = 0.1 ; Rates =  [|1.1|]} // dummy example

let internalComputationEx   ( x: ActualParams ) ( s: float) (_:float[] ) = 
    s + x.IntegrationTime

// this solution is faster than then recursive solution
let defiInfMarchingSequence computeNextState initState = 
    seq{0.0 .. infinity} // irrelevant: just used to generate an infinite sequence
    |> Seq.scan computeNextState initState

let rec recursiveMarchingCondition transition init = 
    seq { 
          yield init 
          let nextElement = transition init 0.0
          yield! recursiveMarchingCondition transition nextElement }

let computeNextState initState _ =
    initState * 0.99

let initState = 1.3
initState |> defiInfMarchingSequence computeNextState // test
initState |> recursiveMarchingCondition computeNextState
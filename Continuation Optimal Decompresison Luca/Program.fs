﻿// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.
open System
open LEModel
open OptimalAscentLearning

let pressAnyKey() = Console.Read() |> ignore

[<AutoOpen>]
module ModelParams = 
    let crossover               = [|     9.9999999999E+09   ;     2.9589519286E-02    ;      9.9999999999E+09    |]
    let rates                   = [| 1.0 / 1.7727676636E+00 ; 1.0 / 6.0111598753E+01  ;  1.0 / 5.1128788835E+02  |]
    let thalmanErrorHypothesis  = true                           
    let gains                   = [| 3.0918150923E-03 ; 1.1503684782E-04 ; 1.0805385353E-03 |]
    let threshold               = [| 0.0000000000E+00 ; 0.0000000000E+00 ; 6.7068236527E-02 |]
    let fractionO2  = 0.21

    let integrationTime = 0.1 // minute

[<EntryPoint>]
let main _ = 
    
    // init leg definition
    let initDepth = 0.0

    let descentParameters = {DescentRate = 60.0 ; MaximumDepth = 120.0; BottomTime = 30.0}

    let discretizationTimeForLegs = 0.1 

    let initialState = ModelDefinition.model
                       |> getInitialStateWithTheseParams descentParameters 
                          discretizationTimeForLegs initDepth
                           
    let stateSeq , nodeSeq = initialState
    Console.WriteLine(stateSeq |> Seq.length)
    Console.WriteLine(nodeSeq |> Seq.length)
    Console.WriteLine (stateSeq |> Seq.last)
    Console.WriteLine (nodeSeq |> Seq.last)


    pressAnyKey()
    0 // return an integer exit code

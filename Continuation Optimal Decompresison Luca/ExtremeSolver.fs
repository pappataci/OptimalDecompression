[<AutoOpen>]
module ExtremeSolver
open TwoLegAscent

open System
open Extreme.Mathematics
open Extreme.Mathematics.Optimization

let nlp = new NonlinearProgram()
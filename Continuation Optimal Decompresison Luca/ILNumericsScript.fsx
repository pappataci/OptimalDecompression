#r @"C:\Program Files (x86)\ILNumerics\ILNumerics Ultimate VS\bin\ILNumerics.Computing.dll"
#r @"C:\Program Files (x86)\ILNumerics\ILNumerics Ultimate VS\bin\ILNumerics.Core.dll"
#r @"C:\Program Files (x86)\ILNumerics\ILNumerics Ultimate VS\bin\ILNumerics.numpy.dll"
#r @"C:\Program Files (x86)\ILNumerics\ILNumerics Ultimate VS\bin\ILNumerics.Toolboxes.Optimization.dll"

open ILNumerics 
open type ILMath
open type ILNumerics.Toolboxes.Optimization

let inline (!) (x :RetArray<'T>) = Array<'T>.op_Implicit(x)
let inline (!!) (x:Array<'T> ) = InArray<'T>.op_Implicit(x) 


let (!!!) (x:float[]) :InArray<float>  = Array.map float32 x 
                                       |> InArray.op_Implicit

 //compute cost abstract example // for now we avoid extra fcns (at the end)
 //let optimalSolutionForThisMission  
 //       missionSimulationParams // missionParams contain init condition, bottom time, max depth, target depth, costToGoFcn
 //       initialState // LEState
 //       optimizer // optimizer takes care of the optimization
 //       initialGuess
 //       =


//let a = ILMath.vector<float>(1.0, 2.0)

//let b = ILMath.vector<float>(3.2,2.2)

//let c = a  + b 

let a = !(vector<float>(1.0,2.0, 4.3))
let b = !vector<float>(3.2,2.2 , 1.23)
let c =  (a+b)


let d = !(ones(2L)) 


//let a' =  

// function experiment

let myFunction (x: InArray<double>) =
    use _scope =  Scope.Enter(x)
    let square (x:float) = x * x 
    let firstTerm = x.GetValue(0L)**2.0 - 2.0
                   |> square
    let secondTerm = x.GetValue(1L) |> square
    let out = firstTerm + secondTerm
    out 
    |> RetArray<double>.op_Implicit




let initGuess = !!![|1.0;1.0|] 

//myFunction initGuess


let myF = ObjectiveFunction myFunction


let solution = fmin(myF  , initGuess)

ILMath.sqrt !(vector<float>(2.0))
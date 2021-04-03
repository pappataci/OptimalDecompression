#r @"C:\Program Files (x86)\ILNumerics\ILNumerics Ultimate VS\bin\ILNumerics.Computing.dll"
#r @"C:\Program Files (x86)\ILNumerics\ILNumerics Ultimate VS\bin\ILNumerics.Core.dll"
#r @"C:\Program Files (x86)\ILNumerics\ILNumerics Ultimate VS\bin\ILNumerics.numpy.dll"
#r @"C:\Program Files (x86)\ILNumerics\ILNumerics Ultimate VS\bin\ILNumerics.Toolboxes.Optimization.dll"

open ILNumerics 
open type ILMath
open type ILNumerics.Toolboxes.Optimization

let inline (!) (x :RetArray<'T>) = Array<'T>.op_Implicit(x)
let inline (!!) (x:Array<'T> ) = InArray<'T>.op_Implicit(x) 


let inline  (!!!) (x:float[]) :InArray<float>  = Array.map float32 x 
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

//let a = !(vector<float>(1.0,2.0, 4.3))
//let b = !vector<float>(3.2,2.2 , 1.23)
//let c =  (a+b)


//let d = !(ones(2L)) 


//let a' =  

// function experiment

let myFunction (x: InArray<double>) =
    use _scope =  Scope.Enter(x)
    let square (x:float) = x * x 
    let firstTerm = x.GetValue(0L)**3.0 - 2.0
                   |> square
    let secondTerm = x.GetValue(1L) |> square
    let out = firstTerm + secondTerm
    out 
    |> RetArray<double>.op_Implicit


let defineObjectiveFunction (computationalFcn:double[] -> double) = 
    let internalFcn(x:InArray<double>) = 
        use _scope = Scope.Enter(x)
        let x_value = x.GetArrayForRead()
        computationalFcn(x_value)
        |> RetArray<double>.op_Implicit
    
    ObjectiveFunction internalFcn


let initGuess = !!![|5.0;1.0|] 

let anotherGuess = [|100.2;2.3|]

let myF = ObjectiveFunction myFunction



let myF' (internalVec:float[]) = 
    let square(x:float) = x * x 
    let firstTerm = internalVec.[0]**3.0 - 2.0
                   |> square
    let secondTerm = (( (   internalVec.[1]** 3.0  - 15.0)  )   ) |> square
    let out = firstTerm + secondTerm
    out 



let defineFcnForOptimizer (fcn: float[] -> float) =
    let fcn' (x:InArray<double>) = 
        use _scope = Scope.Enter(x)
        
        let internalVec = [| 0L .. (x.Length - 1L) |]
                          |> Array.map (fun idx -> x.GetValue(idx)  )
                          
        let out = fcn internalVec 
        out
        |> RetArray<double>.op_Implicit
    ObjectiveFunction fcn'


//let solution4 = fmin( defineFcnForOptimizer myF', initGuess)
//                |> Seq.map float
//                |> Seq.toArray



let optimize (fcn:double[] -> double) (initGuess:double[]) =
    fmin( defineFcnForOptimizer fcn, !!!initGuess)
    |> Seq.map float
    |> Seq.toArray


optimize myF'  [|100.2;2.3|]
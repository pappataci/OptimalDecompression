#r @"C:\Program Files (x86)\ILNumerics\ILNumerics Ultimate VS\bin\ILNumerics.Computing.dll"
#r @"C:\Program Files (x86)\ILNumerics\ILNumerics Ultimate VS\bin\ILNumerics.Core.dll"
#r @"C:\Program Files (x86)\ILNumerics\ILNumerics Ultimate VS\bin\ILNumerics.numpy.dll"
#r @"C:\Program Files (x86)\ILNumerics\ILNumerics Ultimate VS\bin\ILNumerics.Toolboxes.Optimization.dll"
#r @"C:\Program Files (x86)\ILNumerics\ILNumerics Ultimate VS\bin\ILNumerics.Toolboxes.Interpolation.dll"
#r @"C:\Users\glddm\source\repos\DecompressionL20190920A\FuncApprox\bin\Debug\FuncApprox.dll"

open ILNumerics 
open type ILMath
open type Toolboxes.Interpolation
open FuncApprox

let inline (!) (x :RetArray<'T>) = Array<'T>.op_Implicit(x)
let inline (!!) (x:Array<'T> ) = InArray<'T>.op_Implicit(x) 

let inline  (!!!) (x:float[]) :InArray<float>  =  x 
                                                  |> InArray.op_Implicit

let inline (!~) (x:float[]) : Array<float> = x 
                                             |> Array.op_Implicit


let X3 :OutArray<float>= null    

let V (x:'T[]) :Array<'T> = (ILMath.vector<'T> x) |>  Array.op_Implicit 

let X1':Array<float> =   (linspace<float>((!!!)[|-3.0|],(!!!) [|3.0|],(!!!)[|20.0|]))
                            |>  (Seq.toArray >> V )

let Y:Array<float> = sin(X1')|> (Seq.toArray >> V)

let result = kriging( (!!)Y, (!!) X1', (!!) X1', null,  X3) |> (Seq.toArray >> V)

// linear interpolation example

let interpolator = KrigingInterpolator(X1', Y)

         
         //|> Seq.map float
         //|> Seq.toArray
         //|> (!~)

//let X1 =   (linspace<float>((!!!)[|-3.0|],(!!!) [|3.0|],(!!!)[|20.0|]))
//|> Seq.toArray  
//|> (!~)
//|> (!!)
           


//let X2 = X1 
#r @"C:\Program Files (x86)\ILNumerics\ILNumerics Ultimate VS\bin\ILNumerics.Computing.dll"
#r @"C:\Program Files (x86)\ILNumerics\ILNumerics Ultimate VS\bin\ILNumerics.Core.dll"
#r @"C:\Program Files (x86)\ILNumerics\ILNumerics Ultimate VS\bin\ILNumerics.numpy.dll"
#r @"C:\Program Files (x86)\ILNumerics\ILNumerics Ultimate VS\bin\ILNumerics.Toolboxes.Optimization.dll"
#r @"C:\Program Files (x86)\ILNumerics\ILNumerics Ultimate VS\bin\ILNumerics.Toolboxes.Interpolation.dll"

open ILNumerics 
 

open type ILMath



let inline (!) (x :RetArray<'T>) = Array<'T>.op_Implicit(x)
let inline (!!) (x:Array<'T> ) = InArray<'T>.op_Implicit(x) 


let inline  (!!!) (x:float[]) :InArray<float>  =  x 
                                                  |> InArray.op_Implicit

let inline (!~) (x:float[]) : Array<float> = x 
                                             |> Array.op_Implicit


let vec1 = (!~) [|1.0 .. 0.1 .. 2.0 |]

let X1 =   (linspace<float>((!!!)[|-3.0|],(!!!) [|3.0|],(!!!)[|20.0|]))
            |> Seq.toArray  
            |> (!~)
            |> (!!)
            
let X2 = X1 

//linspace(-3.0f,3.0f,20.0f)


let X3 :OutArray<float>= null
let T = meshgrid(X1,X2, X3)
         

let V (x:'T[]) :Array<'T> = (ILMath.vector<'T> x) |>  Array.op_Implicit 

         
         //|> Seq.map float
         //|> Seq.toArray
         //|> (!~)
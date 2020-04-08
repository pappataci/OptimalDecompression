[<AutoOpen>]
module IOUtilities
open System.IO
open System

let writeArrayToFile fileName (array:'T[]) = 
    use streamWriter = new StreamWriter(fileName , false)  
    array
    |> Array.iter ( fun x  -> streamWriter.WriteLine ( " {0}" , x   ) )

let writeTuplesToFile fileName (matrix: ('T*'T)[])  = 
    use streamWriter = new StreamWriter(fileName , false)  
    matrix
    |> Array.iter ( fun (x , y )  -> streamWriter.WriteLine ( " {0}, {1}" , x , y ) ) 

let writeTupleMatrixToFile fileName (matrix: ('T*'T)[,])  = 
    use streamWriter = new StreamWriter(fileName , false)  
    matrix
    |> Array2D.iter ( fun (x , y )  -> streamWriter.WriteLine ( " {0}, {1}" , x , y ) ) 

let writeMatrixToFileByRow fileName separator (matrix:float[,]) = 
    use sw = new StreamWriter(fileName , false)
    let createStringFromMatrixRow  separator (aMatrixRow:float[]) = 
         aMatrixRow
         |> Array.map string
         |> String.concat separator 

    let writeThisRowOnFile iRow = 
        matrix.[iRow, *]
        |> createStringFromMatrixRow  separator
        |> (fun x ->  sw.WriteLine("{0}" , x))

    let   nx = matrix |> Array2D.length1
    let forAllMatrixRows = [|0 .. nx - 1 |]
    forAllMatrixRows
    |> Array.iter writeThisRowOnFile 

let array2DToJaggedArray (array2D:'T[,]) = 
    let xSize = array2D |> Array2D.length1
    let ySize = array2D |> Array2D.length2
    Array.init xSize ( fun i -> 
        Array.init ySize  (fun j ->  array2D.[i,j]  ) )

let jaggedArray2Array2D (seqArray:seq<float[] > ) = 
    let nrows = seqArray |> Seq.length
    let ncols = seqArray |> Seq.head |> Array.length 
    let jaggedArray = seqArray|> Seq.toArray 
    Array2D.init nrows ncols 
        (fun  i j -> jaggedArray.[i].[j])

let writeGridToFile fileName ( xPoints:'T[,] ) ( yPoints :'T[,]) = 
    let linearize2DArray = (Seq.cast<'T> >> Seq.toArray )  
    Array.zip ( xPoints |> linearize2DArray ) (yPoints |> linearize2DArray) 
    |> writeTuplesToFile fileName 

let fileToMatrix (fileName:string) =
    fileName
    |> File.ReadAllLines 
    |> Array.Parallel.map ( fun s ->  s.Split(',')  |> Array.map (fun x ->  double x) ) 

let addToPath (folderName:string) = 
    let addToStringandPutSemicolomn addedString (originalString:string)   = originalString   + addedString + ";"
    let pathVar = "PATH"
    pathVar
    |> Environment.GetEnvironmentVariable
    |> addToStringandPutSemicolomn folderName
    |> ( fun x -> Environment.SetEnvironmentVariable( pathVar  , x) )

let getAmbientPressuresFromFile  =
    fileToMatrix
    >> jaggedArray2Array2D
    >> ( fun x -> x.[*,1])

let pressures2DepthsFromFile  =
    getAmbientPressuresFromFile
    >> Array.Parallel.map (ambientPressureToDepthInFt >> (fun x -> System.Math.Round( x , 4) ) )
    >> Array.toSeq
    >>  Seq.skip 1 
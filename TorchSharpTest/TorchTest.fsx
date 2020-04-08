#r @"C:\Users\glddm\source\repos\DecompressionL20190920A\packages\TorchSharp.0.2.0-preview-27930-2\lib\netstandard2.0\TorchSharp.dll"
#r @"C:\Users\glddm\source\repos\DecompressionL20190920A\packages\AtenSharp.0.1.0\lib\netstandard2.0\AtenSharp.dll"
let torchPath = @"C:\Users\glddm\source\repos\DecompressionL20190920A\packages\TorchSharp.0.2.0-preview-27930-2\lib\netstandard2.0\"
open System

let libTorchPath = @"C:\Users\glddm\source\repos\DecompressionL20190920A\packages\LibTorch.Redist.0.2.0-preview-27930-2\runtimes\win-x64\native"

let addToPath (folderName:string) = 
    let addToStringandPutSemicolomn addedString (originalString:string)   = originalString   + addedString + ";"
    let pathVar = "PATH"
    pathVar
    |> Environment.GetEnvironmentVariable
    |> addToStringandPutSemicolomn folderName
    |> ( fun x -> Environment.SetEnvironmentVariable( pathVar  , x) )


addToPath libTorchPath
open TorchSharp
open TorchSharp.NN
open TorchSharp.Tensor 

let aa = NN.LossFunction.MSE()
let actualPath = Environment.GetEnvironmentVariable "PATH"

//Environment.SetEnvironmentVariable("PATH" , )


let b = TorchSharp.Torch.IsCudaAvailable()
Torch.IsCudaAvailable()


// OPTIMIZER is in Torchsharp.NN.Optimizer.Adam (e.g.)

//let a = TorchSharp.Tensor.IntTensor()
//let a = new Linear(10L, 150L ) // TorchSharp

//let x = new FloatTensor(100L)
//let result = new FloatTensor(100L)

//FloatTensor.Add ( x, 23.0f , result)

//Console.WriteLine( x.Item(12L))
//printfn "%O" x 

// Training example
let linearModel = new NN.Linear( 1L , 1L)

let optimizer = NN.Optimizer.SGD ( linearModel.Parameters() , learningRate = 1.0e-2 )
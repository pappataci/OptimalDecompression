// Learn more about F# at https://fsharp.org
// See the 'F# Tutorial' project for more help.
open TorchSharp
open TorchSharp.NN
open TorchSharp.Tensor 

type MyModelWrapper(nnModules :NN.Module[] )   =
    inherit NN.Module()
    
    do
        for aModule in nnModules  do
            base.RegisterModule(aModule)
         
    member this.NNArchitecture  = nnModules 

    // dummy example
    override this.Forward (input:TorchTensor) = 
        
        input

[<EntryPoint>]
let main argv =
    printfn "%A" argv
   
    
    
    Torch.SetSeed(1 |> int64)
    let _dataLocation = ""
    let trainBatchSize = 64 |> int64
    let testBatchSize = 1000 |> int64
    let train = Data.Loader.MNIST(_dataLocation, trainBatchSize) 
    
    let test = Data.Loader.MNIST(_dataLocation, testBatchSize)
    
    let conv1 = NN.Module.Conv2D(1L, 10L ,5L)
    let conv2 = NN.Module.Conv2D(10L, 20L , 5L)
    let fc1 = NN.Module.Linear(320L, 50L)
    let fc2 = NN.Module.Linear(50L, 10L)
    
    
         
        
    
    0 // return an integer exit code
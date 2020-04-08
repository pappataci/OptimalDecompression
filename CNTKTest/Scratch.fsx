#r @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7.2\Microsoft.CSharp.dll"
#r @"C:\Users\glddm\source\repos\DecompressionL20190920A\packages\Numpy.Bare.3.7.1.6\lib\netstandard2.0\Numpy.Bare.dll"
#r @"C:\Users\glddm\source\repos\DecompressionL20190920A\packages\Python.Runtime.NETStandard.3.7.1\lib\netstandard2.0\Python.Runtime.dll"
#r @"C:\Users\glddm\source\repos\DecompressionL20190920A\packages\Torch.NET.3.7.1.1\lib\netstandard2.0\Torch.NET.dll"
#r @"C:\Users\glddm\source\repos\DecompressionL20190920A\Utilities\bin\Debug\netstandard2.0\Utilities.dll"
open System
open Torch
open Numpy.Models
open Python.Runtime
open System.Linq
open System.Diagnostics
open Utilities.IO

let dtype = torch.float
let device = Torch.torch.device("cuda:0")

let (N, D_in, H, D_out ) = (64, 1000, 100 , 10)

let model = new  torch.nn.Sequential ( 
                        new torch.nn.Linear(D_in, H),  
                        new torch.nn.ReLU(), 
                        new torch.nn.Linear(H, D_out) )

let loss_fn = new  torch.nn.MSELoss(reduction = "sum") 

//let optimizer = Torch.torch.

//let optimizer = torch.nn.optim.Adam(model.parameters(), lr=0.01) 

let x = torch.randn( Shape(N, D_in)  , device = device, requires_grad = Nullable(true))

//let y = torch.randn( Shape(N,D_out))

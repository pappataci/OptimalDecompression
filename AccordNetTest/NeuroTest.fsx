#r @"C:\Users\glddm\source\repos\DecompressionL20190920A\packages\Accord.3.8.0\lib\net462\Accord.dll"
#r @"C:\Users\glddm\source\repos\DecompressionL20190920A\packages\Accord.Math.3.8.0\lib\net462\Accord.Math.dll"
#r @"C:\Users\glddm\source\repos\DecompressionL20190920A\packages\Accord.Neuro.3.8.0\lib\net462\Accord.Neuro.dll"
#r @"C:\Users\glddm\source\repos\DecompressionL20190920A\packages\Accord.Statistics.3.8.0\lib\net462\Accord.Statistics.dll"
open Accord.Neuro.Learning
open Accord.Statistics.Models.Regression
open Accord.Statistics.Models.Regression.Fitting
open Accord.Math.Decompositions
open Accord.MachineLearning
open Accord.Neuro

//let inputs = [|  [|0.0;1.0|]  ;   [|1.0;1.0|]   |]
//let inputdimension = inputs.[0] |> Array.length 
//let outputs = [|  [|1.0|]  ;   [|-20.2|]   |]
//let outputdimension = outputs.[0] |>  Array.length 

let a (x:Neuron) = 
    x
//let network =  Accord.Neuro.ActivationNetwork (
//                SigmoidFunction () , // transfer function
//                inputdimension,
//                10 , // two neuron in first layer
//                outputdimension ) // one neuron in second laye
//NguyenWidrow(network).Randomize()
//let teacher = network |>  LevenbergMarquardtLearning 
//teacher.RunEpoch(inputs,outputs)
////let error = teacher.RunEpoch(inputs, outputs)

////network.Output
//network.Compute( [|1.0;1.0|] )

////network.
//let numInputs = 3
//let numOfClasses = 4
//let hiddenNeurons = 5

//open System.IO
//open System
//Environment.CurrentDirectory <- "C:\Users\glddm\Desktop"
//let inputsValues = 
//     "inputs.csv"
//     |> File.ReadAllLines
//     |> Array.map (fun s ->
//                          (s.Trim().Split(',') )
//                          |> Array.map ( fun x -> double x )  )

//let outputValues = 
//    "outputs.csv"
//    |> File.ReadAllLines
//    |> Array.map ( fun s -> s.Trim().Split(','))

//    // Test from Mastering .NET Machine Learning

//open Accord
//open Accord.Statistics.Analysis
//open Accord.Statistics.Models.Regression
//open Accord.Statistics.Models.Regression.Fitting

//#r @"C:\Users\glddm\source\repos\DecompressionL20190920A\packages\numl.0.8.26.0\lib\net40\numl.dll"
//open numl
//open numl.Model
//open numl.Supervised.NeuralNetwork

//type Student = {[<Feature>]Study: float; 
//                [<Feature>]Beer: float; 
//                [<Label>] mutable Passed: bool}

//let data = 
//            [{Study=2.0;Beer=3.0;Passed=false};
//             {Study=3.0;Beer=4.0;Passed=false};
//             {Study=1.0;Beer=6.0;Passed=false};
//             {Study=4.0;Beer=5.0;Passed=false};
//             {Study=6.0;Beer=2.0;Passed=true};
//             {Study=8.0;Beer=3.0;Passed=true};
//             {Study=12.0;Beer=1.0;Passed=true};
//             {Study=3.0;Beer=2.0;Passed=true};]

//let data' = data |> Seq.map box
//let descriptor = Descriptor.Create<Student>()
//let generator = NeuralNetworkGenerator()
//generator.Descriptor <- descriptor
//let model = Learner.Learn(data', 0.80, 100, generator)
//let accuracy = model.Accuracy


//let activationFunction = BipolarSigmoidFunction()
//let network' = new ActivationNetwork(activationFunction, 1, 10, 1)

//(new NguyenWidrow(network')).Randomize()

//let teacher' = new ParallelResilientBackpropagationLearning(network')

//let rec teach(inp, out) : unit = 
//    match teacher'.RunEpoch( inp, out) with 
//    | x when x > 0.01 -> teach(inp,out)
//    | _ -> ()

//network'.Layers.[0].Neurons

let inputs = [|  [|0.0;1.0|]  ;   [|1.0;1.0|]   |]
let inputdimension = inputs.[0] |> Array.length 
let outputs = [|  [|1.0|]  ;   [|0.0|]   |]
let outputdimension = outputs.[0] |>  Array.length 

let network =  Accord.Neuro.ActivationNetwork (
                SigmoidFunction () , // transfer function
                inputdimension,
                4 , // two neuron in first layer
                10,
                4,
                100,
                outputdimension ) // one neuron in second layer

let teacher = network |>  LevenbergMarquardtLearning 
teacher.RunEpoch(inputs,outputs)

network.Layers |> Array.length

network.Layers.[1].Neurons

//let allParameters (network:ActivationNetwork) = 
//    network.Layers
//    |> Array.map ( fun layer -> layer.Neurons  
//                                |> Array.map (fun neuron -> neuron.Weights) )
let getWeigths (n:ActivationNeuron ) =
    (n.Weights, n.Threshold)

let getNetworkParameters (network:ActivationNetwork) = 
    network.Layers
    |> Array.map ( fun layer -> layer.Neurons  
                                |> Array.map  (fun neuron -> 
                                                    neuron :?> ActivationNeuron 
                                                    |> getWeigths) )
                                                       
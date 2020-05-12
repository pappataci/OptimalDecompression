namespace Mammolo

module Calculate = 

    open Dependency
    
    let testSumThisUp ( x:float[] , b,c )  = 
        (x
         |> Array.sum 
         |> (+) b
         |> (+) c , c * 3.1) 


    let createyVectorInFSharp( x:float ) numOfComponents = 
        Array.create numOfComponents x 

    let doubleThisVec ( x:float[]) =
        x 
        |> Array.map (fun x -> 2.0*x)

    let testDoubleOutput x =
        (x |> doubleThisVec , x )

    let easyNested = CalledLibrary.doubleFloat

    let outputExample = easyNested 2.2

// python code to use F# functions
//import os, sys
//sys.path.append(r'C:\Users\glddm\source\repos\DecompressionL20190920A\TestLibraryFramework\bin\Debug')
//clr.AddReference('TestLibraryFramework')
//from TestLibraryFramework import Calculate

//to_numpy(Calculate.createyVectorInFSharp(1.2, 3))
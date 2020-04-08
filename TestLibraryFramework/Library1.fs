namespace TestLibraryFramework

module Calculate = 

    let createyVectorInFSharp( x:float ) numOfComponents = 
        Array.create numOfComponents x 

    let doubleThisVec ( x:float[]) =
        x 
        |> Array.map (fun x -> 2.0*x)

    let testDoubleOutput x =
        (x |> doubleThisVec , x )
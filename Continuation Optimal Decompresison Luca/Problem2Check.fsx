open System.Numerics
open System

let getUnitVectorInNonHomogCoords (homogenousVectorCoordinates:float32[]) =
    let square x = x**(2.0f)
    let nonHomCoordinates = homogenousVectorCoordinates.[0..2] // discard last component
    let vectorNorm =
        nonHomCoordinates
        |> Array.map square
        |> ( Array.sum >> sqrt)
    nonHomCoordinates 
    |> Array.map (fun x -> x / vectorNorm)  // divide by the length

let defineQuaternionFrom realPart (vectorialPart:float32[])  = 
    let vectorialPart = Vector3( vectorialPart.[0] , vectorialPart.[1] , vectorialPart.[2])
    Quaternion( vectorialPart , realPart)

let defineRotationQuaternionFromAxisAndAngle axisVec angle = 
    let halfAngle = angle/2.0f
    let immaginaryPart = axisVec
                        |>  getUnitVectorInNonHomogCoords
                        |> Array.map ( ( * )  (sin( halfAngle )) )
    let realPart = cos(halfAngle)
    defineQuaternionFrom realPart immaginaryPart

let quaternionAssociatedToaPoint (aPoint:float32[]) = 
    aPoint.[0..2] // get rid of the final one
    |> defineQuaternionFrom aPoint.[3]

let getRotatedPointAndRotationQuaternion initPoint rotationAxis rotationAngle =
    let rotationQuaternion = defineRotationQuaternionFromAxisAndAngle rotationAxis rotationAngle
    let quaternionAssociatedToInitPoint = quaternionAssociatedToaPoint initPoint
    
    let firstProduct = Quaternion.Multiply(rotationQuaternion , quaternionAssociatedToInitPoint)
    let rotatedPointQuaternionForm = Quaternion.Multiply(firstProduct, 
                                            Quaternion.Conjugate(rotationQuaternion))
    let rotatedPointHomogenous = [|rotatedPointQuaternionForm.X ; 
                                   rotatedPointQuaternionForm.Y ; 
                                   rotatedPointQuaternionForm.Z ; 
                                   rotatedPointQuaternionForm.W|]
    (rotatedPointHomogenous, rotationQuaternion)

let rotationAngle = float32 (Math.PI/3.0)
let rotationAxisVectorHomogenous = [|-1.0f; 1.0f; 1.0f; 0.0f|]
let unrotatedPointHomCoordinates = Array.create 4 1.0f // it happens to be all ones

let (rotatedPoint, rotationQuat) = 
    getRotatedPointAndRotationQuaternion unrotatedPointHomCoordinates rotationAxisVectorHomogenous rotationAngle
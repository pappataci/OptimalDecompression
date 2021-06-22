#r @"C:\Program Files (x86)\ILNumerics\ILNumerics Ultimate VS\bin\ILNumerics.Computing.dll"
#r @"C:\Program Files (x86)\ILNumerics\ILNumerics Ultimate VS\bin\ILNumerics.Core.dll"
#r @"C:\Program Files (x86)\ILNumerics\ILNumerics Ultimate VS\bin\ILNumerics.numpy.dll"
#r @"C:\Program Files (x86)\ILNumerics\ILNumerics Ultimate VS\bin\ILNumerics.Toolboxes.Optimization.dll"
#r @"C:\Program Files (x86)\ILNumerics\ILNumerics Ultimate VS\bin\ILNumerics.Toolboxes.Interpolation.dll"
#r @"C:\Users\glddm\source\repos\DecompressionL20190920A\FuncApprox\bin\Debug\FuncApprox.dll"

open FuncApprox

let pressureGridFileName = @"C:\Users\glddm\Documents\Duke\Research\OptimalAscent\maps\pressuresGridLast.mat"

let riskFileName = @"C:\Users\glddm\Documents\Duke\Research\OptimalAscent\maps\risks.mat"
let timeFileName = @"C:\Users\glddm\Documents\Duke\Research\OptimalAscent\maps\times.mat"

let surfaceMapValues = SurfacePressureGridCreator.getSurfaceMapsFromDisk(pressureGridFileName, riskFileName, timeFileName)

let surfaceApproximator = SurfaceMapper surfaceMapValues

let approxSol  = [|3.5; 1.1; 0.3|]
                 |> surfaceApproximator.EstimateRisk
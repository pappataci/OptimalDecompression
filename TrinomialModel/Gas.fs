﻿[<AutoOpen>]
module Gas 
    
    [<AutoOpen>]
    module Constants = 
        let dFO2Air = 0.2100
        let dPACO2  =  0.0460
        let dPVO2   = 0.0605
        let dPVCO2  = 0.0696
        let dPH2O   = 0.0617
        let dPFVG   = 0.1917
        let dPFVG2  = 0.19210526315789
        let dPTMG   = 0.153947368421053
        let dDepthOverrelativePress = 33.066
        let bThalmannError = false

//     get pressure in atm (depth in ft)
    let depth2AmbientPressure depth = 
        1.0 + depth / dDepthOverrelativePress

    let ambientPressureToDepthInFt ambientPressureAmb = 
        dDepthOverrelativePress * ( ambientPressureAmb - 1.0)
        
    let externalN2Pressure ambientPressure  = 
         // deduct also dPACO2 from ambient pressure if Thalmann error is set to true
        (ambientPressure - dPH2O - dPACO2 * (System.Convert.ToDouble  bThalmannError) ) * (1.0 - dFO2Air)

    let depth2N2Pressure depth =
        depth 
        |> depth2AmbientPressure
        |> externalN2Pressure

    let n2Pressure2Depth n2Pressure = 
        let k = dPH2O  + dPACO2 * (System.Convert.ToDouble  bThalmannError) 
        (( n2Pressure / ( 1.0 - dFO2Air) ) + k - 1.0) * dDepthOverrelativePress
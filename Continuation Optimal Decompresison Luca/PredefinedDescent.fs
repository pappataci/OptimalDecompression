﻿namespace InitDescent

module MissionConstraints = 
    let ascentRateLimit = -30.0  // ft/min: ascent is negative  (decreasing depth)
    let descentRateLimit = 60.0 // ft/min : descent is positive (increasing depth)
    let depthTolerance = 1.0 // 1 ft: this is considered to be surface

[<AutoOpen>]
module PredefinedDescent = 

    type TemporalValue<'T> = {Time :  float 
                              Value : 'T}

    type DepthInTime = | TemporalValue of TemporalValue<float>

    type DescentParams = { DescentRate  : float  // (ft/min)
                           MaximumDepth : float  // (ft)
                           } 

    type FixedLeg = { DescentLegParams : DescentParams 
                      BottomTime : float  // (min)
                     }  

    let defineFixedImmersion descentRate maximumDepth bottomTime =
        { DescentLegParams =  { DescentRate = descentRate
                                MaximumDepth = maximumDepth } 
          BottomTime  = bottomTime}

    let resetTimeOfDepthInTime (TemporalValue x : DepthInTime) = 
        {x with Time = 0.0} |> TemporalValue

    let getTime (TemporalValue x : DepthInTime) = 
        x.Time

    let getValue (TemporalValue x : DepthInTime) = 
        x.Value

    [<AutoOpen>]
    module DescentConstant = 
        let tolerance = 1.0e-6

    let discretizeConstantDescentPath anImmersionLeg deltaTime   = 

        let hasNotReachedTheBottom (TemporalValue aDepthInTime) =
            aDepthInTime.Value <= (anImmersionLeg.DescentLegParams.MaximumDepth + tolerance)
            
        let sequenceOfDescendingDepthsAtConstantRate descentRate initialTimeDepth =
            let updateDepthWithThisRate descentRate initDepth = initDepth + descentRate * deltaTime
            let updateDepthAtDescendingRate = updateDepthWithThisRate descentRate

            let updateTime initTime = initTime + deltaTime

            let folder ( TemporalValue {Value = actualDepth ; Time = actualTime} )  _  = 
                TemporalValue { Value = updateDepthAtDescendingRate actualDepth; 
                  Time =  updateTime  actualTime } 

            {1.0 .. infinity}
            |>  Seq.scan folder initialTimeDepth // initial time depth

        let initialDepth = {Time = 0.0 ; Value = 0.0} 

        let descendingSequenceOfTemporalDepths =  
            sequenceOfDescendingDepthsAtConstantRate anImmersionLeg.DescentLegParams.DescentRate  (TemporalValue initialDepth)
            |> Seq.takeWhile hasNotReachedTheBottom
    
        let constantDepthAtMaxDepth = 
            let initialDepthTemporalNode = descendingSequenceOfTemporalDepths |> Seq.last 
            let descendingRate = 0.0 
            let untilBottomTimeIsReached (TemporalValue temporalValue) =
                temporalValue.Time <= (anImmersionLeg.BottomTime + tolerance )
            sequenceOfDescendingDepthsAtConstantRate descendingRate initialDepthTemporalNode
            |> Seq.takeWhile untilBottomTimeIsReached
            |> Seq.skip 1  // get rid of common node
    
        seq { yield! descendingSequenceOfTemporalDepths 
              yield! constantDepthAtMaxDepth } // concatenate the two sequences

    let private seqDepthInTimeToSeqDepths seqDepthInTime = 
        seqDepthInTime
        |> Seq.map ( fun (TemporalValue x ) -> x.Value )

    let getSeqOfDepthsFromDescentParams deltaTime =
        discretizeConstantDescentPath deltaTime >> seqDepthInTimeToSeqDepths
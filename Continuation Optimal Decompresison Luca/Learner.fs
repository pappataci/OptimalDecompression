[<AutoOpen>]
module Learner

type State<'T> = State of 'T
type Action<'A>  = Control of  'A

//type PreviousKnowledge<'FP> = | FunctionParams of 'FP


//type ActualEstimator<'FP, 'S, 'A> = 
//    | QFactor of ( PreviousKnowledge<'FP> -> State<'S> -> Action<'A> -> float )
//    | CostToGo of (PreviousKnowledge<'FP> -> State<'S> -> float )

//type ParameterUpdater<'U> = UpdateFcn of 'U

//type CostToGoEstimate<'FP, 'S , 'A,  'U>  = 
//    {ActualEstimate  : ActualEstimator<'FP, 'S, 'A> 
//     KnowledgeUpdate : PreviousKnowledge<'FP> -> ParameterUpdater<'U>  -> PreviousKnowledge<'FP> }
////type FunctionalApproximator = Params
[<AutoOpen>]
module ReinforcementLearning

type State<'S>       =         | State of 'S
type Action<'A>      =         |  Control of  'A                       
type Model<'S, 'A>   =         |  Model of (State<'S> -> Action<'A> -> State<'S>) 

type EnvironmentResponse<'S ,'I > = { NextState         : State<'S> 
                                      TransitionReward  : float
                                      IsTerminalState   : bool 
                                      ExtraInfo         : Option<'I> }

type Environment<'S, 'A ,'I> =    |Environment of (State<'S> -> Action<'A> -> EnvironmentResponse<'S ,'I>)

type InstantaneousReward<'S,'A> = |InstantaneousReward of ( State<'S> -> Action<'A> -> State<'S> -> float )

type TerminalStatePredicate<'S> = State<'S> -> bool

type ShortTermRewardEstimator<'S,'A> = { InstantaneousReward : InstantaneousReward<'S,'A>
                                         TerminalReward      : (State<'S> -> float)     }

type ExtraInfoLogger<'S,'A,'I> = | InfoLogger of (State<'S>*State<'S>*Action<'A>*float*bool -> Option<'I> )

let defineEnvironment<'S,'A ,'I> (Model model:Model<'S,'A>) 
    {InstantaneousReward   =  InstantaneousReward instantaneousReward;  TerminalReward = finalReward} 
    (isTerminalState: TerminalStatePredicate<'S>) 
    (InfoLogger extraInfoCreator : ExtraInfoLogger<'S,'A,'I> ) = 

    let innerEnvinromentComputation ( actualState: State<'S> ) ( action:Action<'A> )  = 
        let nextState = model actualState action
        let transitionReward = instantaneousReward actualState action nextState
        let isTerminalState = isTerminalState nextState

        let finalStateReward =  match isTerminalState with 
                                | true -> finalReward nextState
                                | _ -> 0.0

        let totalReward = transitionReward + finalStateReward 
        {NextState = nextState  
         TransitionReward = totalReward
         IsTerminalState = isTerminalState
         ExtraInfo = extraInfoCreator (actualState , nextState , action , totalReward , isTerminalState ) 
         } 

    Environment innerEnvinromentComputation

// this generic type is used to express an iteration
type CountedSequence<'T> = 
    seq< State<'T> * int > 

let fromValueFuncToStateFunc (f: 'S -> 'A -> 'S) = 
    let stateComputation (State x:State<'S>) (Control y: Action<'A>) =
        State (f x y) 
    Model stateComputation

let defineModel (modelParams:'P ) (transitionFunction: 'P -> 'S -> 'A -> 'S) = 
    modelParams
    |> transitionFunction
    |> fromValueFuncToStateFunc
 
let fromModelToValueFunc(model:Model<'S,'A>) =
    let (Model getNextState) = model
    let actualComputation (state:'S)(action:'A) =
        let (State simpleOutput) =  getNextState (State state) (Control action)
        simpleOutput
    actualComputation

let getNextStateFromActualStateModelNAction (Model modelEvaluator:Model<'S,'A>) (actualState: State<'S>) (action:Action<'A>)   = 
    modelEvaluator actualState action

// this function consumes an initial value, passed to a computation, where iterations are counted
let getMarchingCountedLazyComputation (model: Model<'S,'A>) (initialState :State<'S>) (sequenceOfActions : seq<'A> )= 
    let getNextStateNIncreaseCounter (actualState, counter) action =  
        (getNextStateFromActualStateModelNAction model actualState (Control action) , counter + 1 ) 
    sequenceOfActions
    |> Seq.scan getNextStateNIncreaseCounter (initialState , 1)

    
let whileEagerComputation maxIterations statePredicate (initialValue:'T)  (f:'T->'T) =
    let rec loop (acc, i) =
        if (i < maxIterations) && (statePredicate acc) then   
            loop (f acc, (i+1) ) 
        else 
            (acc , i )
    loop (initialValue, 0)

// marching dynamics algorithms are implemented through this function, until either convergence or maximum
// number of iterations are  reached
let whileStatePredicateAndMaxIterations maxIterations statePredicate  (initialSequence:CountedSequence<'T>) =
    initialSequence
    |> Seq.takeWhile ( fun (State state, counter) -> 
                            (statePredicate state)  && (counter <= maxIterations)  )
    |> Seq.map (fst >> fun (State value ) -> value) 
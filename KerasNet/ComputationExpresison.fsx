type FizzBuzzSequenceBuilder() =
    member x.Yield(v) = 
        match ( v % 3, v % 5) with 
        | 0 , 0 -> "FizzBuzz"
        | 0, _ -> "Fizz"
        | _, 0 -> "Buzz"
        | _ -> v.ToString()
    member x.Delay(f) =
        f() |> Seq.singleton
    member x.Delay(f: unit -> string seq) = f()
    member x.Combine(l, r) = 
        Seq.append (Seq.singleton l) r 
    member x.For(g,f) = 
        Seq.map f g

let fizzbuzz = FizzBuzzSequenceBuilder()
fizzbuzz { yield 1
           yield 2 
           yield 3}

type LoggingBuilder() =
    let log p = printfn "expression is %A" p

    member this.Bind(x, f) =
        log x
        f x

    member this.Return(x) =
        x

let logger = new LoggingBuilder()

let loggedWorkflow =
    logger
        {
        let! x = 42
        let! y = 43
        let! z = x + y
        return z
        }

type MaybeBuilder() =

    member this.Bind(x, f) =
        match x with
        | None -> None
        | Some a -> f a

    member this.Return(x) =
        Some x

let maybe = new MaybeBuilder()

let divideBy bottom top =
    if bottom = 0
    then None
    else Some(top/bottom)

let divideByWorkflow init x y z =
    maybe
        {
        let! a = init |> divideBy x
        let! b = a |> divideBy y
        let! c = b |> divideBy z
        return c
        }

let good = divideByWorkflow 12 3 2 1
let bad = divideByWorkflow 12 3 0 1


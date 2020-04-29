let (|Odd|Even|) (num , aParam) = 
    if ((num+aParam) % 2 = 0 ) then 
        Even
    else  Odd

let testActivePattern aNum aParam= 
    match (aNum,  aParam) with
    | Odd -> printfn "Odd"
    | Even -> printfn "Even"
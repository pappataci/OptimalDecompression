// to be experimented later

type ToPython<'S> = {Converter : 'S -> float[] 
                     Content   : 'S}


type Test<'A,'B> = | Test of Option< 'A -> 'B -> 'B > 

let f (x:int)  (c:string) = x.ToString()

let aa =   Test (Some f)

let bb = Test None 
open System.IO

let orig = @"C:\Users\glddm\Documents\Stat\EngBioStats\orig.txt"
let content = File.ReadAllLines orig 
              |>Seq.head 
              |> (fun x -> x.Split(' '))
              |> Array.filter (fun x -> not(x = "") )
              |> Array.chunkBySize 19
              |> Array.map ( fun x -> System.String.Join(", " , x))

            
let output = @"C:\Users\glddm\Documents\Stat\EngBioStats\fat.txt"

File.WriteAllLines(output, content)
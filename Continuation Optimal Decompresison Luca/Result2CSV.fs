[<AutoOpen>]
module Result2CSV
open FSharp.Data
open TwoLegAscent

type AscentCsvProvider = CsvProvider<"TargetDepth, P0, P1, P2, TimeStepsAtConstDepth, ConstantDepthLev, AscentRate, OnSurfaceTime, FinalRisk, FinalTime" , 
                                       Schema= "float, float, float, float, float, float, float, float, float, float" , HasHeaders = true >

let addRowToMyCsv (x:ResultData) = 
    AscentCsvProvider.Row(x.TargetDepth, x.P0 , x.P1 , x.P2 , x.TimeStepsAtConstDepth , x.ConstantDepthLev, x.AscentRate , x.OnSurfaceTime, x.FinalRisk, x.FinalTime)

let myCsvBuildTable data = 
  new AscentCsvProvider(Seq.map addRowToMyCsv data)

let saveToCsv (fileName:string) (csvTable:AscentCsvProvider) = 
    csvTable.Save fileName

let writeResultsToDisk fileName (finalSubFolder:option<string>) results = 
    
    let finalSubFolder = match finalSubFolder with
                         | None -> @"TwoLegStudy\"
                         | Some v -> (v + @"\")
      
    let subFolder =  @"C:\Users\glddm\Desktop\" + finalSubFolder 

    results
    |> myCsvBuildTable
    |> saveToCsv ( subFolder + fileName) 




type OnlyAscentNode = CsvProvider<"Depth" , Schema = "float" , HasHeaders = true> 



let writeArrayToDisk fileName (finalSubFolder:option<string>) (results:seq<float>)  = 
    
    let finalSubFolder = match finalSubFolder with
                         | None -> @"ThreeLegsStudy\"
                         | Some v -> (v + @"\")
      
    let subFolder =  @"C:\Users\glddm\Desktop\" + finalSubFolder 

    let table = new OnlyAscentNode(Seq.map (fun x -> OnlyAscentNode.Row(x)  ) results ) 

    table.Save (subFolder + fileName )


    //results
    //|> myCsvBuildTable
    //|> saveToCsv ( subFolder + fileName) 
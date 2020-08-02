module Result2CSV

open FSharp.Data
open TwoLegAscent

type AscentCsvProvider = CsvProvider<"TargetDepth, P0, P1, P2, TimeStepsAtConstDepth, ConstantDepthLev, AscentRate, OnSurfaceTime, FinalRisk, FinalTime" , 
                                       Schema= "float, float, float, float, float, float, float, float, float, float" , HasHeaders = true >

let addRowToMyCsv (x:ResultData) = 
    AscentCsvProvider.Row(x.TargetDepth, x.P0 , x.P1 , x.P2 , x.TimeStepsAtConstDepth , x.ConstantDepthLev, x.AscentRate , x.OnSurfaceTime, x.FinalRisk, x.FinalTime)

let myCsvBuildTable data = 
  new AscentCsvProvider(Seq.map addRowToMyCsv data)

let myCsvSave (fileName:string) (csvTable:AscentCsvProvider) = 
    csvTable.Save fileName
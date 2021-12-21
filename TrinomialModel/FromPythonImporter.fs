module FromPythonImporter

open FSharp.Data

type profileReader = CsvProvider< @"C:\Users\glddm\Documents\Duke\exampls.csv", HasHeaders=true>
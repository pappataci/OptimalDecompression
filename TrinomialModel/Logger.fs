namespace Logger

[<AutoOpen>]
module LoggerSettings = 
    open System
    open System.IO

    let logFolder = @"C:\Users\glddm\Documents\Duke\Research\OptimalAscent\Logs\"
    let dateFormat = "s"
    
    let createLogFileName()  =
        let now = DateTime.Now
        logFolder + "log_" + now.ToString(dateFormat).Replace(':','_') + ".txt"
        

    let staticLogFileName = 
        createLogFileName()

    let startLogger() =
        File.WriteAllLines(staticLogFileName , [|"Init Logging"|])

    let addToLogger (message) = 
        File.AppendAllLines(staticLogFileName, message)
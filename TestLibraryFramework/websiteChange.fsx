open System.Net
open System.IO 

let address = "http://www.istruzionevicenza.it/wordpress/"

let request = address |> WebRequest.Create                
let response   =  request.GetResponse()

let  source = new StreamReader(response.GetResponseStream())

let referenceContent = source.ReadToEnd()

//let compareFirstNelements referenceString actualString numElements = 
    
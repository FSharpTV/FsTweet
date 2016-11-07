module FsTweet.Web.Program
open Suave
open Suave.Web
open Suave.Successful

[<EntryPoint>]
let main argv = 
    startWebServer defaultConfig (OK "hello")
    0 // return an integer exit code


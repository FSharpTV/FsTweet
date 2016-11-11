﻿module FsTweet.Web.Program

open Suave
open System.Reflection
open System.IO
open Suave.DotLiquid
open Suave.Operators
open Suave.Filters
open Suave.Files
let currentDirectory =
  let mainExeFileInfo = new FileInfo(Assembly.GetEntryAssembly().Location)
  mainExeFileInfo.Directory

let viewsDirectory = Path.Combine(currentDirectory.FullName, "views")

setTemplatesDir viewsDirectory

[<EntryPoint>]
let main argv = 
  let app = 
    choose[
     path "/" >=> page "guest_home.html" ""
     path "/signup" >=> page "signup.html" ""
     browseHome
    ] 
  startWebServer defaultConfig app
  0


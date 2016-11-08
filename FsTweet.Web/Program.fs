module FsTweet.Web.Program

open Suave
open System.Reflection
open System.IO
open Suave.DotLiquid

let currentDirectory =
  let mainExeFileInfo = new FileInfo(Assembly.GetEntryAssembly().Location)
  mainExeFileInfo.Directory

let viewsDirectory = Path.Combine(currentDirectory.FullName, "views")

setTemplatesDir viewsDirectory

[<EntryPoint>]
let main argv = 
  startWebServer defaultConfig (page "guest_home.html" "")
  0


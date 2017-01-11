module FsTweet.Web.Program

open Suave
open System.Reflection
open System.IO
open Suave.DotLiquid
open Suave.Operators
open Suave.Filters
open Suave.Files
open UserSignup
open FsTweet.Persistence.User
open EmailService
open System
let currentDirectory =
  let mainExeFileInfo = new FileInfo(Assembly.GetEntryAssembly().Location)
  mainExeFileInfo.Directory
let viewsDirectory = Path.Combine(currentDirectory.FullName, "views")

let onEmailSent (args : System.ComponentModel.AsyncCompletedEventArgs) = 
  if not (isNull args.Error) then
    printfn "%A" args.Error
let sendFakeEmail email = printfn "%A" email

[<EntryPoint>]
let main argv =     
  setTemplatesDir viewsDirectory

  let smtpConfig = {
    Username = Environment.GetEnvironmentVariable("SMTP_USERNAME")
    Password = Environment.GetEnvironmentVariable("SMTP_PASSWORD")
    Host = Environment.GetEnvironmentVariable("SMTP_HOST")
    Port = Environment.GetEnvironmentVariable("SMTP_PORT") |> int
  }
  let sendEmail = sendEmail onEmailSent smtpConfig
  let hostUrl = Environment.GetEnvironmentVariable("APP_HOST_URL")
  let app = 
    choose[
     path "/" >=> page "guest_home.html" ""
     UserSignup hostUrl userPersistence sendFakeEmail
     browseHome
    ] 
  startWebServer defaultConfig app
  0


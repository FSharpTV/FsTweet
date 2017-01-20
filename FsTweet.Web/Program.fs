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
open Login
open System
open SessionCombinators
open UserProfile
open System.Net
let onEmailSent (args : System.ComponentModel.AsyncCompletedEventArgs) = 
  if not (isNull args.Error) then
    printfn "%A" args.Error
let sendFakeEmail email = printfn "%A" email

[<EntryPoint>]
let main argv =    

  let viewsDirectory = 
    let currentDirectory = (new FileInfo(Assembly.GetEntryAssembly().Location)).Directory    
    Path.Combine(currentDirectory.FullName, "views")
  setTemplatesDir viewsDirectory

  let smtpConfig = {
    Username = Environment.GetEnvironmentVariable("FST_SMTP_USERNAME")
    Password = Environment.GetEnvironmentVariable("FST_SMTP_PASSWORD")
    Host = Environment.GetEnvironmentVariable("FST_SMTP_HOST")
    Port = Environment.GetEnvironmentVariable("FST_SMTP_PORT") |> int
  }
  let sendEmail = sendEmail onEmailSent smtpConfig
  let hostUrl = Environment.GetEnvironmentVariable("FST_SERVER_HOST_URL")

  addFakeUser "tamizh" "tamizh88@gmail.com" "foobar"

  let app = 
    choose[
     path "/" >=> page "guest_home.liquid" ""
     UserSignup hostUrl createUser sendFakeEmail
     UserEmailVerification getUser markUserEmailVeified
     UserLogin getUserByUsername     
     path "/logout" >=> (clearSession >=> Redirection.FOUND loginPath)
     pathRegex "/assets/*" >=> browseHome
     UserProfile getUserByUsername
    ] 
  let serverSecret = Environment.GetEnvironmentVariable("FST_SERVER_SECRET")
  let port =
    match Sockets.Port.TryParse("FST_SERVER_PORT") with
    | true, port -> port
    | _ -> uint16 8083
  let config = {
    defaultConfig with
      serverKey = System.Text.Encoding.UTF8.GetBytes(serverSecret)
      bindings = [HttpBinding.create HTTP IPAddress.Any port]     
  }
  
  startWebServer config app
  0


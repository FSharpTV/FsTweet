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
    Username = Environment.GetEnvironmentVariable("SMTP_USERNAME")
    Password = Environment.GetEnvironmentVariable("SMTP_PASSWORD")
    Host = Environment.GetEnvironmentVariable("SMTP_HOST")
    Port = Environment.GetEnvironmentVariable("SMTP_PORT") |> int
  }
  let sendEmail = sendEmail onEmailSent smtpConfig
  let hostUrl = Environment.GetEnvironmentVariable("APP_HOST_URL")

  addFakeUser "tamizh" "tamizh88@gmail.com" "foobar"

  let app = 
    choose[
     path "/" >=> page "guest_home.liquid" ""
     UserSignup hostUrl createUser sendFakeEmail
     UserEmailVerification getUser markUserEmailVeified
     UserLogin getUserByUsername     
     path "/logout" >=> (clearSession >=> Redirection.FOUND loginPath)
     pathRegex "/assets/*" >=> browseHome
     pathScan "/%s" renderUserProfilePage     
    ] 
  startWebServer defaultConfig app
  0


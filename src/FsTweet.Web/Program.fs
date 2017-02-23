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
open FsTweet.Web.Tweet
open FsTweet.Persistence.Tweet
open FsTweet.Persistence.Follow
open FsTweet.Persistence.Wall
open Follow
open Wall
open Listeners
let onEmailSent (args : System.ComponentModel.AsyncCompletedEventArgs) = 
  if not (isNull args.Error) then
    printfn "%A" args.Error
let sendFakeEmail email = printfn "%A" email

let addFakeData () =
  addFakeUser "tamizh" "tamizh88@gmail.com" "foobar"
  addFakeUser "mark" "mark@fsharp.tv" "foobar"
  addFakeUser "haf" "haf@fsharp.tv" "foobar"
 
[<EntryPoint>]
let main argv =    

  let currentDirectory = 
    (new FileInfo(Assembly.GetEntryAssembly().Location)).Directory.FullName 
  let viewsDirectory =       
    Path.Combine(currentDirectory, "views")
  setTemplatesDir viewsDirectory

  let faviconPath = Path.Combine(currentDirectory, "assets", "favicon.ico")
  let hostUrl = Environment.GetEnvironmentVariable("FST_SERVER_HOST_URL")

  addFakeData ()  

  let onTweetListener = onTweetListener getFollowers addPost

  let app = 
    choose[
     UserHome getWallPosts
     UserSignup hostUrl createUser sendFakeEmail
     UserEmailVerification getUser markUserEmailVeified
     UserLogin getUserByUsername     
     path "/logout" >=> (clearSession >=> Redirection.FOUND loginPath)
     pathRegex "/assets/*" >=> browseHome
     path "/favicon.ico" >=> Files.file faviconPath
     Tweet onTweetListener createPost getTweets
     Follow followUser getFollowers getFollowing
     UserProfile getUserByUsername isFollowing     
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


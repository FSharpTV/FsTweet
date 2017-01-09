module FsTweet.Web.UserSignup

open FsTweet.Domain.User
open FsTweet.Persistence.User
open ResultExtensions
open Suave
open Suave.Operators
open Suave.DotLiquid
open Suave.Filters
open Suave.Form
open EmailService

type UserSignupViewModel = {
  Username : string
  Email : string
  Password: string
  Error : string
}
let toUser vm = newUser vm.Username vm.Email vm.Password  

let emptyUserSignupViewModel = 
  {
    Username = ""
    Email = ""
    Password = ""
    Error = ""
  }

let createUser (userPersistence : UserPersistence) (user : User) = asyncResult {
  let! isUniqueUsername = userPersistence.IsUniqueUsername user.Username
  match isUniqueUsername with
  | false -> return Error "Username already exists"
  | _ -> 
    let! isUniqueEmailAddress = userPersistence.IsUniqueEmailAddress user.EmailAddress
    match isUniqueEmailAddress with
    | false -> return Error "Email address already exists"
    | _ -> return! userPersistence.CreateUser user
}

let sendActivationEmail sendEmail (user : User) = 
  let emailTemplate = """
    Hi {username},   
    Your FsTweet account has been created successfully.   
    <a href="{link}"> Click here </a> to activate your account.
    
    Regards
    FsTweet
  """
  let body username userId = 
    emailTemplate
      .Replace("{username}", username)
      .Replace("{link}", "http://localhost:8083/activate/" + userId.ToString())

  let email = {
    Subject = "Your FsTweet account has been created"
    From = "email@fstweet.com"
    Destination = user.EmailAddress.Value
    Body = body user.Username.Value user.Id.Value.Value
    IsBodyHtml = true
  }
  sendEmail email


let handleUserSignup userPersistence sendEmail ctx = async {
  match bindForm (Form([],[])) ctx.request with
  | Choice1Of2 userSignupViewModel -> 
    match toUser userSignupViewModel with
    | Ok user -> 
      let! userCreateResult = createUser userPersistence user
      match userCreateResult with
      | Ok user ->
        sendActivationEmail sendEmail user
        let redirectPath = sprintf "/signup_success/%s" user.Username.Value
        return! Redirection.FOUND redirectPath ctx
      | Error err ->        
        return! page "signup.html" {userSignupViewModel with Error = err} ctx 
    | Error errs -> 
      return! page "signup.html" {userSignupViewModel with Error = errs.Head} ctx 
  | Choice2Of2 err -> 
    return! page "signup.html" emptyUserSignupViewModel ctx
}

let UserSignup userPersistence sendEmail = 
  choose[
    path "/signup" 
      >=> choose[
          GET >=> page "signup.html" emptyUserSignupViewModel
          POST >=> handleUserSignup userPersistence sendEmail]
    pathScan "/signup_success/%s" (page "signup_success.html")
  ]
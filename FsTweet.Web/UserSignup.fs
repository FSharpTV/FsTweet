module FsTweet.Web.UserSignup

open FsTweet.Domain.Core
open FsTweet.Domain.UserSignup
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

let newCreateUser username emailAddress password : CreateUser = {
  Username = username
  EmailAddress = emailAddress
  Password = password
}

let mapCreateUser vm =
  Ok newCreateUser 
    <*> Username.TryCreate vm.Username
    <*> EmailAddress.TryCreate vm.Email
    <*> Password.TryCreate vm.Password

let emptyUserSignupViewModel = 
  {
    Username = ""
    Email = ""
    Password = ""
    Error = ""
  }

let tryCreateUser (userPersistence : UserPersistence) createUser = async {
  let createUserPersistence : CreateUserPersistence = {
    IsUniqueUsername = userPersistence.IsUniqueUsername
    IsUniqueEmailAddress = userPersistence.IsUniqueEmailAddress
    CreateUser = userPersistence.CreateUser
  }
  return! tryCreateUser createUserPersistence createUser  
}

let sendActivationEmail sendEmail (userCreated : UserCreated) = 
  let emailTemplate = """
    Hi {username},   
    Your FsTweet account has been created successfully.   
    <a href="{link}"> Click here </a> to activate your account.
    
    Regards
    FsTweet
  """
  let body = 
    emailTemplate
      .Replace("{username}", userCreated.Username.Value)
      .Replace("{link}", "http://localhost:8083/activate/" + userCreated.UserId.Value.ToString())

  let email = {
    Subject = "Your FsTweet account has been created"
    From = "email@fstweet.com"
    Destination = userCreated.EmailAddress.Value
    Body = body
    IsBodyHtml = true
  }
  sendEmail email


let handleUserSignup userPersistence sendEmail ctx = async {
  match bindForm (Form([],[])) ctx.request with
  | Choice1Of2 signupRequest -> 
    match mapCreateUser signupRequest with
    | Ok createUser -> 
      let! userCreateResult = tryCreateUser userPersistence createUser
      match userCreateResult with
      | Ok userCreated ->
        sendActivationEmail sendEmail userCreated
        let redirectPath = sprintf "/signup_success/%s" userCreated.Username.Value
        return! Redirection.redirect redirectPath ctx
      | Error err ->
        match err with
        | RequestError e | PersistenceError e -> 
          return! page "signup.html" {signupRequest with Error = e} ctx 
    | Error errs -> 
      return! page "signup.html" {signupRequest with Error = errs.Head} ctx 
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
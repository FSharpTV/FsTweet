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

let doCreateUser (userPersistence : UserPersistence) signupRequest createUser ctx = async {
  let createUserPersistence : CreateUserPersistence = {
    IsUniqueUsername = userPersistence.IsUniqueUsername
    IsUniqueEmailAddress = userPersistence.IsUniqueEmailAddress
    CreateUser = userPersistence.CreateUser
  }
  let! userCreateResult = tryCreateUser createUserPersistence createUser
  match userCreateResult with
  | Ok userCreated ->
    printfn "%A" userCreated
    return! Redirection.redirect "/" ctx
  | Error err ->
    match err with
    | RequestError e | PersistenceError e -> 
      return! page "signup.html" {signupRequest with Error = e} ctx 
}

let handleUserSignup userPersistence ctx = async {
  match bindForm (Form([],[])) ctx.request with
  | Choice1Of2 signupRequest -> 
    match mapCreateUser signupRequest with
    | Ok createUser -> 
      return! doCreateUser userPersistence signupRequest createUser ctx
    | Error errs -> 
      return! page "signup.html" {signupRequest with Error = errs.Head} ctx 
  | Choice2Of2 err -> 
    return! page "signup.html" emptyUserSignupViewModel ctx
}

let UserSignup userPersistence = 
  path "/signup" 
    >=> choose[
          GET >=> page "signup.html" emptyUserSignupViewModel
          POST >=> handleUserSignup userPersistence]
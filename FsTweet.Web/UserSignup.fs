module FsTweet.Web.UserSignup

open FsTweet.Domain.Core
open FsTweet.Domain.UserSignup
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

let newCreateUser username emailAddress password = {
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

let handleUserSignup ctx = async {
  match bindForm (Form([],[])) ctx.request with
  | Choice1Of2 signupRequest -> 
    match mapCreateUser signupRequest with
    | Ok createUser -> 
      return! Redirection.FOUND "/" ctx
    | Error errs -> 
      return! page "signup.html" {signupRequest with Error = errs.Head} ctx 
  | Choice2Of2 err -> 
    return! page "signup.html" emptyUserSignupViewModel ctx
}

let UserSignup = 
  path "/signup" 
    >=> choose[
          GET >=> page "signup.html" emptyUserSignupViewModel
          POST >=> handleUserSignup]

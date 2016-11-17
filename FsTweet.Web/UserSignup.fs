﻿module FsTweet.Web.UserSignup

open FsTweet.Domain.UserSignup
open FsTweet.Domain.ResultExtensions
open Suave
open Suave.Operators
open Suave.DotLiquid
open Suave.Filters

open Suave.Form
open Suave.RequestErrors
open System.Net.Mail

type UserSignupViewModel = {
  Username : string
  Email : string
  Password: string
  ConfirmPassword: string
  Error : string
}

let mapCreateUser vm =
  Ok createUser 
    <*> Username.tryCreate vm.Username
    <*> EmailAddress.tryCreate vm.Email
    <*> Password.tryCreate vm.Password

let emptyUserSignupViewModel = 
  {
    Username = ""
    Email = ""
    Password = ""
    ConfirmPassword = ""
    Error = ""
  }

 
// UserSignupViewModel -> bool * string
// (UserSignupViewModel -> bool) * string
let userSignupViewModelForm : Form<UserSignupViewModel> = 
  let validate vm =
    match mapCreateUser vm with
    | Ok _ -> true, ""
    | Error errs -> false, errs.Head
  let foo vm = (validate vm |> fst)
  Form([],[(foo, "")])

type UserSignupRequest = {
  Username : string
  Email : MailAddress
  Password: Password
  ConfirmPassword: Password
}


let userSignupRequestForm : Form<UserSignupRequest> =
  let userName = TextProp ((fun m -> <@ m.Username @>), [maxLength 8])
  let password = PasswordProp ((fun m -> <@ m.Password @>), [passwordRegex @"(\w){6,20}"])
  let passwordsMatch =
    (fun f -> f.Password = f.ConfirmPassword), "Passwords must match"
  let properties = [userName;password]
  Form(properties,[passwordsMatch])

let handleUserSignup ctx = async {
  match bindForm userSignupRequestForm ctx.request with
  | Choice1Of2 signupRequest -> 
    printfn "%A" signupRequest
    return! NOT_FOUND "" ctx
  | Choice2Of2 err -> 
    match bindForm userSignupViewModelForm ctx.request with
    | Choice1Of2 signupViewModel ->
      return! page "signup.html" {signupViewModel with Error = err} ctx
    | _ -> return! page "signup.html" emptyUserSignupViewModel ctx
}

let UserSignup = 
  path "/signup" 
    >=> choose[
          GET >=> page "signup.html" emptyUserSignupViewModel
          POST >=> handleUserSignup]

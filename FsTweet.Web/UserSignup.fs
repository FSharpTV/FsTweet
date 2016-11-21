﻿module FsTweet.Web.UserSignup

open FsTweet.Domain.UserSignup
open FsTweet.Domain.Core.ResultOperators
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
  Error : string
}

let newCreateUser username emailAddress password = {
  Username = username
  EmailAddress = emailAddress
  Password = password
}

let mapCreateUser vm =
  Ok newCreateUser 
    <*> Username.tryCreate vm.Username
    <*> EmailAddress.tryCreate vm.Email
    <*> Password.tryCreate vm.Password

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
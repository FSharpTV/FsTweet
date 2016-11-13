module FsTweet.Web.UserSignup


open Suave
open Suave.Operators
open Suave.DotLiquid
open Suave.Filters

open Suave.Form
open Suave.RequestErrors
open System.Net.Mail

type UserSignupViewModel = {
  Username : string
  Email : MailAddress
  Password: Password
  ConfirmPassword: Password
}

let userSignupForm : Form<UserSignupViewModel> =
  let userName = TextProp ((fun m -> <@ m.Username @>), [maxLength 8])
  let password = PasswordProp ((fun m -> <@ m.Password @>), [passwordRegex @"(\w){6,20}"])
  let passwordsMatch =
    (fun f -> f.Password = f.ConfirmPassword), "Passwords must match"
  let properties = [userName;password]
  Form(properties,[passwordsMatch])

let handleUserSignup ctx = async {
  match bindForm userSignupForm ctx.request with
  | Choice1Of2 signupViewModel -> 
    printfn "%A" signupViewModel
    return! NOT_FOUND "" ctx
  | Choice2Of2 err -> return! BAD_REQUEST err ctx
}

let UserSignup = 
  path "/signup" 
    >=> choose[
          GET >=> page "signup.html" ""
          POST >=> handleUserSignup]

module FsTweet.Web.Login

open Suave.DotLiquid
open Suave
open Suave.Filters
open Suave.Operators
open SessionCombinators
open Suave.Form
open FsTweet.Domain.User
open ResultExtensions
open Suave.Authentication

let loginPath = "/login"
let loginPage = "login.liquid"
let userCookieName = "fstweet_user"

type LoginViewModel = {
  Username : string
  Password : string
  Error : string
}
let emptyLoginViewModel = {
  Username = ""
  Password = ""
  Error = ""
}

let redirectToLoginPage errMsg =
  sprintf "%s?error=%s" loginPath errMsg
  |> Redirection.FOUND 

let loginSuccess (user : User) =
  authenticated Cookie.CookieLife.Session false      
    >=> sessionSet (redirectToLoginPage "unable to login") userCookieName user
    >=> Redirection.FOUND ("/" + user.Username.Value)

let validateLogin username password getUserByUsername = asyncResult {
  let! getUserResult = getUserByUsername username
  match getUserResult with
  | None -> return Ok None
  | Some (user : User) ->
    match user.Password = password with
    | false -> return Error "password didn't match"
    | _ -> 
      match user.EmailAddress with
      | Unverified _ -> return Error "email address not verified yet"
      | _ -> return Ok (Some user) 
}
let userLogin username password getUserByUsername ctx = async {
  let! loginResult = validateLogin username password getUserByUsername
  match loginResult with
  | Ok None -> return! redirectToLoginPage "Username not found" ctx
  | Error err -> return! redirectToLoginPage err ctx
  | Ok (Some user) -> return! loginSuccess user ctx
}

let handleUserLogin getUserByUsername ctx = async {
  match bindForm (Form([],[])) ctx.request with
  | Choice1Of2 loginViewModel -> 
    match loginCredentials loginViewModel.Username loginViewModel.Password with
    | Ok (username, password) ->
      return! userLogin username password getUserByUsername ctx
    | _ -> return! redirectToLoginPage "invalid login credentials" ctx
  | Choice2Of2 err ->     
    return! redirectToLoginPage err ctx
}
let renderLoginPage vm (request : HttpRequest) =
  let err = defaultArg request.["error"] ""  
  page loginPage {vm with Error = err}
let UserLogin getUserByUsername = 
  path loginPath >=> choose [
    GET >=> request (renderLoginPage emptyLoginViewModel)
    POST >=> handleUserLogin getUserByUsername
  ]

let secured onFail webpart = 
  sessionGet onFail userCookieName webpart

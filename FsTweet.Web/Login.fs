module FsTweet.Web.Login

open Suave.DotLiquid
open Suave
open Suave.Filters
open Suave.Operators

let loginPath = "/login"
let loginPage = "login.liquid"

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

let renderLoginPage vm =
  page loginPage vm

let UserLogin = 
  path loginPath >=> choose [
    GET >=> renderLoginPage emptyLoginViewModel
  ]

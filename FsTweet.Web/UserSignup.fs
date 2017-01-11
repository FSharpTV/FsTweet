module FsTweet.Web.UserSignup
open FsTweet.Domain.User
open FsTweet.Persistence.User
open ResultExtensions
open Suave.Form
open EmailService
open Suave
open Suave.DotLiquid
open Suave.Filters
open Suave.Operators
open System

let signupPath = "/signup"
let signupSuccessPath = "/signup_success"
let activationPath = "/activate"
let signupPage = "signup.html"
let signupSuccessPage = "signup_success.html"

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

let signup createUser sendEmail hostUrl userSignupViewModel ctx = async {
    match toUser userSignupViewModel with
    | Ok user -> 
      let! userCreateResult = createUser user
      match userCreateResult with
      | Ok userCreated ->
        let userId = userCreated.Id.ToString()
        let activationUrl = sprintf "%s%s?userid=%s" hostUrl activationPath userId |> Uri
        sendActivationEmail activationUrl sendEmail user
        let redirectPath = sprintf "%s?username=%s" signupSuccessPath user.Username.Value
        return! Redirection.FOUND redirectPath ctx
      | Error err ->        
        return! page signupPage {userSignupViewModel with Error = err} ctx 
    | Error errs -> 
      return! page signupPage {userSignupViewModel with Error = errs.Head} ctx 
  }

let handleUserSignup hostUrl createUser sendEmail ctx = async {
  match bindForm (Form([],[])) ctx.request with
  | Choice1Of2 userSignupViewModel -> 
    return! signup createUser sendEmail hostUrl userSignupViewModel ctx
  | Choice2Of2 err -> 
    return! page signupPage emptyUserSignupViewModel ctx
}

let renderSignupSuccessPage (req : HttpRequest) =
  match req.["username"] with
  | Some username ->
    page signupSuccessPage username
  | None -> page signupSuccessPage ""

let UserSignup hostUrl createUser sendEmail =   
  choose[
    path signupPath
      >=> choose[
            GET >=> page signupPage emptyUserSignupViewModel
            POST >=> handleUserSignup hostUrl createUser sendEmail]
    path signupSuccessPath 
      >=> request renderSignupSuccessPage
  ]
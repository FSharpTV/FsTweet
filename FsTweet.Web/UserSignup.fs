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
let handleUserSignup hostUrl userPersistence sendEmail ctx = async {
  match bindForm (Form([],[])) ctx.request with
  | Choice1Of2 userSignupViewModel -> 
    match toUser userSignupViewModel with
    | Ok user -> 
      let! userCreateResult = createUser userPersistence user
      match userCreateResult with
      | Ok userCreated ->
        let userId = userCreated.Id.ToString()
        let activationUrl = sprintf "%s%s?userid=%s" hostUrl activationPath userId
        sendActivationEmail activationUrl sendEmail user
        let redirectPath = sprintf "%s?username=%s" signupSuccessPath user.Username.Value
        return! Redirection.FOUND redirectPath ctx
      | Error err ->        
        return! page signupPage {userSignupViewModel with Error = err} ctx 
    | Error errs -> 
      return! page signupPage {userSignupViewModel with Error = errs.Head} ctx 
  | Choice2Of2 err -> 
    return! page signupPage emptyUserSignupViewModel ctx
}

let renderSignupSuccessPage (req : HttpRequest) =
  match req.["username"] with
  | Some username ->
    page signupSuccessPage username
  | None -> page signupSuccessPage ""

let UserSignup hostUrl userPersistence sendEmail =   
  choose[
    path signupPath
      >=> choose[
            GET >=> page signupPage emptyUserSignupViewModel
            POST >=> handleUserSignup hostUrl userPersistence sendEmail]
    path signupSuccessPath 
      >=> request renderSignupSuccessPage
  ]
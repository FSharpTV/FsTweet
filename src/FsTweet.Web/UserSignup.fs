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
let verifyEmailPath = "/verify_email"
let signupPage = "signup.liquid"
let signupSuccessPage = "signup_success.liquid"
let notFoundPage = "not_found.liquid"
let serverErrorPage = "server_error.liquid"
let emailVerifySuccessPage = "email_verify_success.liquid"

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
        let activationUrl = sprintf "%s%s?userid=%s" hostUrl verifyEmailPath userId |> Uri
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

let verifyUserEmail userId getUser markUserEmailVerified ctx = async {
  let! getUserResult = getUser userId
  match getUserResult with
  | Ok (Some _ ) -> 
    let! emailVerifyResult = markUserEmailVerified userId
    match emailVerifyResult with
    | Ok (user : User) -> return! page emailVerifySuccessPage user.Username.Value ctx
    | Error err -> 
      printfn "%A" err
      return! page serverErrorPage "Something went wrong!" ctx 
  | Ok None -> return! page notFoundPage "User Id not found" ctx
  | Error err -> 
    printfn "%A" err
    return! page serverErrorPage "Something went wrong!" ctx
}

let handleEmailVerification getUser markUserEmailVeified ctx = async {
  match ctx.request.["userid"] with
  | Some userId ->  
    match UserId.TryCreate userId with
    | Ok userId -> return! verifyUserEmail userId getUser markUserEmailVeified ctx
    | Error _ -> return! page notFoundPage "Invalid User Id" ctx
  | None -> return! page notFoundPage "404!" ctx  
}

let UserEmailVerification getUser markUserEmailVeified =
  path verifyEmailPath >=> handleEmailVerification getUser markUserEmailVeified
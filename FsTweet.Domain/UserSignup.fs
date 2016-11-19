module FsTweet.Domain.UserSignup
open FsTweet.Domain.ResultExtensions

type Error = 
| RequestError of string
| SystemError of string

let mapValidationError errMsg asyncResult = async {
  let! result = asyncResult
  match result with
  | Ok isValid when isValid -> return Ok isValid
  | Ok x -> return RequestError errMsg |> Error
  | Error err -> return SystemError err |> Error
}

type Username = private Username of string with
  member this.Value = 
    let (Username username) = this
    username
  static member tryCreate (username : string) =
    match username with
    | null | ""  -> Error ["Username should not be empty"] 
    | x when x.Length > 8 -> Error ["Username should not be more than 8 characters"]
    | x -> Username x |> Ok

type EmailAddress = private EmailAddress of string with
  member this.Value =
    let (EmailAddress emailAddress) = this
    emailAddress
  static member tryCreate (emailAddress : string) =
   try 
     let _ = new System.Net.Mail.MailAddress(emailAddress)
     EmailAddress emailAddress |> Ok
   with
     | _ -> Error ["Invalid Email Address"]

type Password = private Password of string with 
  member this.Value =
    let (Password password) = this
    password
  static member tryCreate (password : string) =
    match password with
    | null | ""  -> Error ["Password should not be empty"] 
    | x when x.Length < 4 || x.Length > 8 -> Error ["Password should contain only 4-8 characters"]
    | x -> Password x |> Ok
 
type CreateUser = {
  Username : Username
  EmailAddress : EmailAddress
  Password : Password
}

type UserCreated = {
  UserId : System.Guid
}

type UserPersistence = {
  IsUniqueUsername : Username -> Async<Result<bool,string>>
  IsUniqueEmailAddress : EmailAddress -> Async<Result<bool,string>>
  CreateUser : CreateUser -> Async<Result<UserCreated, string>>
}
let validate persistence createUser = async {
  let! s = 
     createUser.Username
     |> persistence.IsUniqueUsername
     |> mapValidationError "Username already exists"
  let! _ =
    createUser.EmailAddress
     |> persistence.IsUniqueEmailAddress
     |> mapValidationError "Email address already exists"
  return Ok createUser
}

let tryCreateUser persistence createUser = async {
  let! validationR = validate persistence createUser
  Async.RunSynchronously
  // TODO
  return 0
}
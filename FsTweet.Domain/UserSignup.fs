module FsTweet.Domain.UserSignup

open FsTweet.Domain.Core
open FsTweet.Domain.Core.ResultOperators

let mapAsyncResult errMsg asyncResult = async {
 let! result = asyncResult
 match result with
 | Ok isValid when isValid -> return Ok isValid
 | Ok x -> return errMsg |> RequestError |> Error
 | Error err -> return Error err
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
  IsUniqueUsername : Username -> Async<Result<bool,Error>>
  IsUniqueEmailAddress : EmailAddress -> Async<Result<bool,Error>>
  CreateUser : CreateUser -> Async<Result<UserCreated, Error>>
}

let createUserValidations persistence createUser = 
  [ createUser.Username
          |> persistence.IsUniqueUsername
          |> mapAsyncResult "Username address already exists"
    createUser.EmailAddress
          |> persistence.IsUniqueEmailAddress
          |> mapAsyncResult "Email address already exists"]

let rec createUserIfValid validations persistence createUser = async {
  match validations with
  | [] -> return! persistence.CreateUser createUser
  | x :: xs ->
    let! xR = x
    match xR with
    | Ok _ -> return! createUserIfValid xs persistence createUser
    | Error err -> return Error err
}

let tryCreateUser persistence createUser = async {
  let validations = createUserValidations persistence createUser
  return! createUserIfValid validations persistence createUser
}
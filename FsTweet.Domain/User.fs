module FsTweet.Domain.User

open System
open ResultExtensions

let internal error errorMsg = Error [errorMsg]

type Username = private Username of string with
  member this.Value = 
    let (Username username) = this
    username
  static member TryCreate (username : string) =
    match username with
    | null | ""  -> error "Username should not be empty"
    | x when x.Length > 8 -> error "Username should not be more than 8 characters"
    | x -> Username x |> Ok

type EmailAddress = private EmailAddress of string with
  member this.Value =
    let (EmailAddress emailAddress) = this
    emailAddress
  static member TryCreate (emailAddress : string) =
   try 
     new System.Net.Mail.MailAddress(emailAddress) |> ignore
     EmailAddress emailAddress |> Ok
   with
     | _ -> error "Invalid Email Address"

type Password = private Password of string with 
  member this.Value =
    let (Password password) = this
    password
  static member TryCreate (password : string) =
    match password with
    | null | ""  -> error "Password should not be empty"
    | x when x.Length < 4 || x.Length > 8 -> error "Password should contain only 4-8 characters"
    | x -> Password x |> Ok

type UserId = private UserId of Guid with
  member this.Value =
    let (UserId userId) = this
    userId
  static member TryCreate (guid : string) =
    try 
      let id = new Guid(guid)
      UserId id |> Ok
    with
    | _ -> Error "Invalid GUID"

type UserEmailAddress = 
| Verified of EmailAddress
| Unverified of EmailAddress

type User = {
  Username : Username
  EmailAddress : UserEmailAddress
  Password : Password
}

let newUser username emailAddress password =
  let newUser' username emailAddress password =
    { Username = username 
      EmailAddress = Unverified emailAddress
      Password = password}
  Ok newUser'
    <*> Username.TryCreate username
    <*> EmailAddress.TryCreate emailAddress
    <*> Password.TryCreate password
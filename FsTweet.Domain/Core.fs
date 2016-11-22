module FsTweet.Domain.Core
open System

type Error =
| PersistenceError of string
| RequestError of string

type Username = private Username of string with
  member this.Value = 
    let (Username username) = this
    username
  static member TryCreate (username : string) =
    match username with
    | null | ""  -> Error ["Username should not be empty"] 
    | x when x.Length > 8 -> Error ["Username should not be more than 8 characters"]
    | x -> Username x |> Ok

type EmailAddress = private EmailAddress of string with
  member this.Value =
    let (EmailAddress emailAddress) = this
    emailAddress
  static member TryCreate (emailAddress : string) =
   try 
     let _ = new System.Net.Mail.MailAddress(emailAddress)
     EmailAddress emailAddress |> Ok
   with
     | _ -> Error ["Invalid Email Address"]

type Password = private Password of string with 
  member this.Value =
    let (Password password) = this
    password
  static member TryCreate (password : string) =
    match password with
    | null | ""  -> Error ["Password should not be empty"] 
    | x when x.Length < 4 || x.Length > 8 -> Error ["Password should contain only 4-8 characters"]
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
    | _ -> Error ["Invalid GUID"]

type UserStatus = ActivationEmailSent | Activated
 
type User = {
  Id : UserId
  Username : Username
  Password: Password
  EmailAddress : EmailAddress
  Status : UserStatus
}
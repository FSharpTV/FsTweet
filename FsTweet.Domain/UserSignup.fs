module FsTweet.Domain.UserSignup


type Username = private Username of string with 
  static member tryCreate (username : string) =
    match username with
    | null | ""  -> Error ["Username should not be empty"] 
    | x when x.Length > 8 -> Error ["Username should not be more than 8 characters"]
    | x -> Username x |> Ok
  static member value (Username username) = username

type EmailAddress = private EmailAddress of string with 
  static member tryCreate (emailAddress : string) =
   try 
     let _ = new System.Net.Mail.MailAddress(emailAddress)
     EmailAddress emailAddress |> Ok
   with
     | _ -> Error ["Invalid Email Address"]
  static member value (EmailAddress emailAddress) = emailAddress

type Password = private Password of string with 
  static member tryCreate (password : string) =
    match password with
    | null | ""  -> Error ["Password should not be empty"] 
    | x when x.Length < 4 || x.Length > 8 -> Error ["Password should contain only 4-8 characters"]
    | x -> Password x |> Ok
  static member value (Password password) = password
 
type CreateUser = {
  Username : Username
  EmailAddress : EmailAddress
  Password : Password
}

let createUser username emailAddress password = {
  Username = username
  EmailAddress = emailAddress
  Password = password
}
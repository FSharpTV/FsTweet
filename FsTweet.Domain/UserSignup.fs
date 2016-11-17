module FsTweet.Domain.UserSignup


type Username = private Username of string with 
  static member tryCreate (username : string) =
    if username = null || username = "" then 
      Error ["Username should not be empty"] 
    else
      Username username |> Ok
  static member value (Username username) = username

type EmailAddress = private EmailAddress of string with 
  static member tryCreate (emailAddress : string) =
    if emailAddress = null || emailAddress = "" then 
      Error ["Email address should not be empty"]
    else
      EmailAddress emailAddress |> Ok
  static member value (EmailAddress emailAddress) = emailAddress

type Password = private Password of string with 
  static member tryCreate (password : string) =
    if password = null || password = "" then 
      Error ["Password should not be empty"]
    else
      Password password |> Ok
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
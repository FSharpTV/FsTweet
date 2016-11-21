#load "./Core.fs"
#load "./ResultExtensions.fs"
#load "./UserSignup.fs"

open FsTweet.Domain.Core
open ResultExtensions
open FsTweet.Domain.UserSignup

let newCreateUser username emailAddress password = {
  Username = username
  EmailAddress = emailAddress
  Password = password
}

let (Ok user) =
  Ok newCreateUser 
    <*> Username.tryCreate "tamizh"
    <*> EmailAddress.tryCreate "tamizh@fs.com"
    <*> Password.tryCreate "foobar"

let persistence = {
  IsUniqueUsername = fun _ -> true |> Ok |> async.Return
  IsUniqueEmailAddress = fun _ -> true |> Ok |> async.Return
  CreateUser = fun _ -> {UserId = System.Guid.NewGuid()} |> Ok |> async.Return
}

let (result : Result<UserCreated, Error>) = 
  tryCreateUser persistence user |> Async.RunSynchronously
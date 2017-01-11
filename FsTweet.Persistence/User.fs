module FsTweet.Persistence.User

open System.Collections.Generic
open FsTweet.Domain.User
open ResultExtensions
open System

type UserCreated = {
  Id : Guid
  User : User
}

let private users = new Dictionary<Guid, User>()

let internal ok<'T> (a : 'T) = Ok a |> async.Return

let isUniqueUsername username =
  users.Values
  |> Seq.exists (fun user -> user.Username = username)
  |> not
  |> ok
 
let isUniqueEmailAddress emailAddress =
  users.Values
  |> Seq.exists (fun user -> user.EmailAddress = emailAddress)
  |> not
  |> ok

let createUser (user : User) = asyncResult {
  let! isUniqueUsername = isUniqueUsername user.Username
  match isUniqueUsername with
  | false -> return Error "Username already exists"
  | _ -> 
    let! isUniqueEmailAddress = isUniqueEmailAddress user.EmailAddress
    match isUniqueEmailAddress with
    | false -> return Error "Email address already exists"
    | _ -> 
      let id = System.Guid.NewGuid()
      users.Add(id, user)
      return! ok {Id = id; User = user}
}
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

let getUserByUsernameOrEmailAddress username emailAddress =
  users.Values
  |> Seq.tryFind (fun user -> user.Username = username || user.EmailAddress = emailAddress)
  |> Ok |> async.Return

let createUser user = asyncResult {
  let! getUserResult = getUserByUsernameOrEmailAddress user.Username user.EmailAddress
  match getUserResult with
  | Some u ->
    if u.Username = user.Username then
      return Error "username already exists"
    else
      return Error "email address already exists"
  | None ->
    let id = System.Guid.NewGuid()
    users.Add(id, user)
    return Ok {Id = id; User = user}
}
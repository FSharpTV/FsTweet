module FsTweet.Persistence.User

open System.Collections.Generic
open FsTweet.Domain.Core

let private users = new Dictionary<UserId, User>()

let private success<'T> (a : 'T) = Ok a |> async.Return

let isUniqueUsername username =
  users.Values
  |> Seq.exists (fun user -> user.Username = username)
  |> success
 
let isUniqueEmailAddress emailAddress =
  users.Values
  |> Seq.exists (fun user -> user.EmailAddress = emailAddress)
  |> success





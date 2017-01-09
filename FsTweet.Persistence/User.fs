module FsTweet.Persistence.User

open System.Collections.Generic
open FsTweet.Domain.User
open ResultExtensions
open System

type UserPersistence = {
  IsUniqueUsername : Username -> Async<Result<bool,string>>
  IsUniqueEmailAddress : EmailAddress -> Async<Result<bool,string>>
  CreateUser : User -> Async<Result<User,string>>
}

let private users = new Dictionary<UserId, User>()

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

let createUser user = 
  let id = System.Guid.NewGuid()
  match UserId.TryCreate (id.ToString()) with
  | Ok userId -> 
    let newUser = { user with Id = Some userId }
    users.Add(userId, newUser)
    ok newUser
  | Error err -> Error err |> async.Return

let userPersistence = {
  CreateUser = createUser
  IsUniqueEmailAddress = isUniqueEmailAddress
  IsUniqueUsername = isUniqueUsername
}
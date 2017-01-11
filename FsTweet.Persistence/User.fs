module FsTweet.Persistence.User

open System.Collections.Generic
open FsTweet.Domain.User
open ResultExtensions
open System

type UserCreated = {
  Id : Guid
  User : User
}

type UserPersistence = {
  IsUniqueUsername : Username -> Async<Result<bool,string>>
  IsUniqueEmailAddress : EmailAddress -> Async<Result<bool,string>>
  CreateUser : User -> Async<Result<UserCreated,string>>
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

let createUser user = 
  let id = System.Guid.NewGuid()
  users.Add(id, user)
  ok {Id = id; User = user}

let userPersistence = {
  CreateUser = createUser
  IsUniqueEmailAddress = isUniqueEmailAddress
  IsUniqueUsername = isUniqueUsername
}
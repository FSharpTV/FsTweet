module FsTweet.Persistence.User

open System.Collections.Generic
open FsTweet.Domain.Core
open FsTweet.Domain.UserSignup
open ResultExtensions

type UserPersistence = {
  IsUniqueUsername : Username -> Async<Result<bool,Error>>
  IsUniqueEmailAddress : EmailAddress -> Async<Result<bool,Error>>
  CreateUser : CreateUser -> Async<Result<UserCreated, Error>>
}

let private users = new Dictionary<UserId, User>()

let private success<'T> (a : 'T) = Ok a |> async.Return

let isUniqueUsername username =
  users.Values
  |> Seq.exists (fun user -> user.Username = username)
  |> not
  |> success
 
let isUniqueEmailAddress emailAddress =
  users.Values
  |> Seq.exists (fun user -> user.EmailAddress = emailAddress)
  |> not
  |> success

let createUser createUser = 
  let id = System.Guid.NewGuid()
  match UserId.TryCreate (id.ToString()) with
  | Ok userId -> 
    let newUser = {
      Id = userId
      Password = createUser.Password
      Username = createUser.Username      
      EmailAddress = createUser.EmailAddress
      Status = Created
    }
    users.Add(userId, newUser)
    {
      UserCreated.EmailAddress = createUser.EmailAddress
      Username = createUser.Username
      UserId = userId
    } |> success
  | Error errs -> PersistenceError errs.Head |> Error |> async.Return

let userPersistence = {
  CreateUser = createUser
  IsUniqueEmailAddress = isUniqueEmailAddress
  IsUniqueUsername = isUniqueUsername
}
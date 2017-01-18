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
let addFakeUser username emailAddress password = 
  match newUser username emailAddress password with  
  | Ok user -> 
    let verifiedEmailAddress = Verified (User.emailAddress user.EmailAddress)
    users.Add(Guid.NewGuid(), {user with EmailAddress = verifiedEmailAddress})
  | _ -> ()
let ok<'T> (v : 'T)  = Ok v |> async.Return
let getUserByUsernameOrEmailAddress username emailAddress =
  users.Values
  |> Seq.tryFind (fun user -> user.Username = username || user.EmailAddress = emailAddress)
  |> Ok |> async.Return

let getUser (userId : UserId) =
  match users.TryGetValue(userId.Value) with
  | true, user -> ok (Some user) 
  | _ -> ok None 

let getUserByUsername username =
  users.Values
  |> Seq.tryFind (fun user -> user.Username = username)
  |> ok

let markUserEmailVeified (userId : UserId) =
  match users.TryGetValue(userId.Value) with
  | true, user ->
    match user.EmailAddress with
    | Verified _ -> ok user
    | Unverified emailAddress ->
      let updatedUser = {user with EmailAddress = Verified emailAddress}
      users.[userId.Value] <- updatedUser
      ok updatedUser
  | _ -> Error "user id not found" |> async.Return

let createUser user = asyncResult {
  let! getUserResult = getUserByUsernameOrEmailAddress user.Username user.EmailAddress
  match getUserResult with
  | Some u ->
    if u.Username = user.Username then
      return Error "username already exists"
    else
      return Error "email address already exists"
  | None ->
    let id = Guid.NewGuid()
    users.Add(id, user)
    return Ok {Id = id; User = user}
}
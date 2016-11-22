module FsTweet.Domain.UserSignup

open FsTweet.Domain.Core
open ResultExtensions

type CreateUser = {
  Username : Username
  EmailAddress : EmailAddress
  Password : Password
}

type UserCreated = {
  UserId : UserId
}

type UserPersistence = {
  IsUniqueUsername : Username -> Async<Result<bool,Error>>
  IsUniqueEmailAddress : EmailAddress -> Async<Result<bool,Error>>
  CreateUser : CreateUser -> Async<Result<UserCreated, Error>>
}

let private createUserValidations persistence createUser = 
  let isUnique msg = function
  | true -> Ok true
  | false -> RequestError msg |> Error 
  [ createUser.Username
          |> persistence.IsUniqueUsername
          |> mapAsyncOkResult (isUnique "Username address already exists")
    createUser.EmailAddress
          |> persistence.IsUniqueEmailAddress
          |> mapAsyncOkResult (isUnique "Email address already exists")]

let rec private createUserIfValid validations persistence createUser = async {
  match validations with
  | [] -> return! persistence.CreateUser createUser
  | x :: xs ->
    let! xR = x
    match xR with
    | Ok _ -> return! createUserIfValid xs persistence createUser
    | Error err -> return Error err
}

let tryCreateUser persistence createUser = async {
  let validations = createUserValidations persistence createUser
  return! createUserIfValid validations persistence createUser
}
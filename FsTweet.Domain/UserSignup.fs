module FsTweet.Domain.UserSignup
open FsTweet.Domain.AsynResult

type Username = private Username of string with
  member this.Value = 
    let (Username username) = this
    username
  static member tryCreate (username : string) =
    match username with
    | null | ""  -> Error ["Username should not be empty"] 
    | x when x.Length > 8 -> Error ["Username should not be more than 8 characters"]
    | x -> Username x |> Ok

type EmailAddress = private EmailAddress of string with
  member this.Value =
    let (EmailAddress emailAddress) = this
    emailAddress
  static member tryCreate (emailAddress : string) =
   try 
     let _ = new System.Net.Mail.MailAddress(emailAddress)
     EmailAddress emailAddress |> Ok
   with
     | _ -> Error ["Invalid Email Address"]

type Password = private Password of string with 
  member this.Value =
    let (Password password) = this
    password
  static member tryCreate (password : string) =
    match password with
    | null | ""  -> Error ["Password should not be empty"] 
    | x when x.Length < 4 || x.Length > 8 -> Error ["Password should contain only 4-8 characters"]
    | x -> Password x |> Ok
 
type CreateUser = {
  Username : Username
  EmailAddress : EmailAddress
  Password : Password
}

type UserCreated = {
  UserId : System.Guid
}

type UserPersistence = {
  IsUniqueUsername : Username -> Async<Result<bool,string>>
  IsUniqueEmail : EmailAddress -> Async<Result<bool,string>>
  CreateUser : CreateUser -> Async<Result<UserCreated, string list>>
}

// let foo persistence createUser =
//   let f isUniqueUsername isEmailAddress =
//     match isUniqueUsername, isEmailAddress with
//     | true, true -> persistence.CreateUser createUser
//     | _ -> ["Username or Email address already exists"] |> Error |> async.Return

let tryCreateUser persistence createUser = async {
  let! isUniqueUsernameR = persistence.IsUniqueUsername createUser.Username
  match isUniqueUsernameR with
  | Ok isUniqueUsername -> 
    match isUniqueUsername with
    | true -> 
      let! isUniqueEmailR = persistence.IsUniqueEmail createUser.EmailAddress
      match isUniqueEmailR with
      | Ok isUniqueEmail ->
        match isUniqueEmail with
        | true -> 
          return! persistence.CreateUser createUser
        | _ -> return Error [sprintf "Emailaddress '%s' already exists" createUser.EmailAddress.Value]
      | Error err -> return Error [err]
    | _ -> return Error [sprintf "Username '%s' already exists" createUser.Username.Value]
  | Error err -> return Error [err]
}
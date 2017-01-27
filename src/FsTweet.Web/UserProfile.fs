module FsTweet.Web.UserProfile
open Login
open Suave.DotLiquid
open FsTweet.Domain.User
open Suave
open System.Security.Cryptography
open System.Text
open Suave.Filters
open UserSignup
open ResultExtensions

let userProfilePage = "user_profile.liquid"

type ProfileViewModel = {
  Username : string
  GravatarUrl : string
  IsLoggedIn: bool
  IsFollowing : bool
  IsSelf : bool
}
let emailToGravatarUrl (email : EmailAddress) =
  use md5 = MD5.Create()
  email.Value.Trim().ToLowerInvariant()  
  |> Encoding.Default.GetBytes
  |> md5.ComputeHash
  |> Array.map (fun b -> b.ToString("x2"))
  |> String.concat ""
  |> sprintf "http://www.gravatar.com/avatar/%s?s=200"

let profileViewModelOfUser (user : User) = 
  {
    GravatarUrl = emailToGravatarUrl user.EmailAddress.Value
    Username = user.Username.Value
    IsLoggedIn = true
    IsFollowing = true
    IsSelf = false
  }

let user getUserByUsername username = async {
  match Username.TryCreate username with
  | Ok n -> 
    let! userFindResult = getUserByUsername n
    match userFindResult with
    | Ok x -> return Ok x
    | Error err -> return Error err
  | Error _ -> return Ok None
}

let renderProfileForGuest getUserByUsername username ctx = async {
  let! result = user getUserByUsername username
  match result with
  | Ok (Some user) -> 
    let vm = {
      GravatarUrl = emailToGravatarUrl user.EmailAddress.Value
      Username = user.Username.Value
      IsLoggedIn = true
      IsFollowing = false
      IsSelf = false
    }
    return! page userProfilePage vm ctx
  | Ok None -> return! page notFoundPage "user not found" ctx
  | Error err -> return! page serverErrorPage err ctx
} 


let getProfileViewModelForUser getUserByUsername isFollowing username (loggedInUser : User) = asyncResult {
  let! (result : User option) = user getUserByUsername username
  match result with
  | Some userToFollow -> 
    let vm = {
      GravatarUrl = emailToGravatarUrl loggedInUser.EmailAddress.Value
      Username = loggedInUser.Username.Value
      IsLoggedIn = true
      IsFollowing = false
      IsSelf = false
    }
    match userToFollow.Username = loggedInUser.Username with
    | true -> return Some {vm with IsSelf = true} |> Ok
    | _ -> 
      let! isFollowingUser = isFollowing loggedInUser.Username userToFollow.Username
      match isFollowingUser with
      | true -> return Ok (Some vm)
      | _ -> return Some {vm with IsFollowing = true} |> Ok
  | None -> return Ok None
}

let renderProfileForUser getUserByUsername isFollowing username loggedInUser ctx = async {
  let! result = getProfileViewModelForUser getUserByUsername isFollowing username loggedInUser
  match result with
  | Ok (Some vm) -> return! page userProfilePage vm ctx
  | Ok None -> return! page notFoundPage "user not found" ctx
  | Error err -> return! page serverErrorPage err ctx
}
let renderUserProfilePage getUserByUsername isFollowing username =
  let renderProfileForGuest =
    renderProfileForGuest getUserByUsername username  
  let renderProfileForUser loggedInUser =
    renderProfileForUser getUserByUsername isFollowing username loggedInUser
  secured renderProfileForGuest renderProfileForUser    
let UserProfile getUserByUsername isFollowing =
  pathScan "/%s" (renderUserProfilePage getUserByUsername isFollowing)
module FsTweet.Web.UserProfile
open Login
open Suave.DotLiquid
open FsTweet.Domain.User
open Suave
open System.Security.Cryptography
open System.Text
open Suave.Filters
open UserSignup

let userProfilePage = "user_profile.liquid"

type ProfileViewModel = {
  Username : string
  GravatarUrl : string
  CanPost: bool
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
    CanPost = true
  }
let profileViewModelOfGuest (user : User) =
  {
    GravatarUrl = emailToGravatarUrl user.EmailAddress.Value
    Username = user.Username.Value
    CanPost = false
  }

let renderProfileForGuest getUserByUsername username ctx = async {
  match Username.TryCreate username with
  | Ok n -> 
    let! userFindResult = getUserByUsername n
    match userFindResult with
    | Ok (Some user) -> 
      return! page userProfilePage (profileViewModelOfGuest user) ctx
    | Ok _ -> return! page notFoundPage "user not found" ctx
    | Error err ->  
      return! page serverErrorPage err ctx
  | _ -> return! page notFoundPage "user not found" ctx
}

let renderUserProfilePage getUserByUsername username =
  let renderProfile user =
    page userProfilePage (profileViewModelOfUser user)  
  secured (renderProfileForGuest getUserByUsername username) renderProfile  
  
let UserProfile getUserByUsername =
  pathScan "/%s" (renderUserProfilePage getUserByUsername)
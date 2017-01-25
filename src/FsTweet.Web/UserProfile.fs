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
  IsLoggedIn: bool
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
  }
let profileViewModelOfGuest (user : User) =
  {
    GravatarUrl = emailToGravatarUrl user.EmailAddress.Value
    Username = user.Username.Value
    IsLoggedIn = false
  }

let renderProfile getUserByUsername username f ctx = async {
  match Username.TryCreate username with
  | Ok n -> 
    let! userFindResult = getUserByUsername n
    match userFindResult with
    | Ok (Some user) -> 
      return! page userProfilePage (f user) ctx
    | Ok _ -> return! page notFoundPage "user not found" ctx
    | Error err ->  
      return! page serverErrorPage err ctx
  | _ -> return! page notFoundPage "user not found" ctx
}
let renderUserProfilePage getUserByUsername username =
  let renderProfileForGuest =
    renderProfile getUserByUsername username profileViewModelOfGuest  
  let renderProfileForUser _ =
    renderProfile getUserByUsername username profileViewModelOfUser
  secured renderProfileForGuest renderProfileForUser    
let UserProfile getUserByUsername =
  pathScan "/%s" (renderUserProfilePage getUserByUsername)
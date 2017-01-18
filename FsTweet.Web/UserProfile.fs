module FsTweet.Web.UserProfile
open Login
open Suave.DotLiquid
open FsTweet.Domain.User
open Suave
open System.Security.Cryptography
open System.Text

type ProfileViewModel = {
  Username : string
  GravatarUrl : string
}

let emailToGravatarUrl (email : EmailAddress) =
  use md5 = MD5.Create()
  email.Value.Trim().ToLowerInvariant()  
  |> Encoding.Default.GetBytes
  |> md5.ComputeHash
  |> Array.map (fun b -> b.ToString("x2"))
  |> String.concat ""
  |> sprintf "http://www.gravatar.com/avatar/%s?s=200"

let toProfileViewModel user = 
  {
    GravatarUrl = emailToGravatarUrl user.EmailAddress.Value
    Username = user.Username.Value
  }

let renderUserProfilePage username =
  let renderProfile' (user : User) =
    page "user_profile.liquid" (toProfileViewModel user)
  secured (Redirection.FOUND loginPath) renderProfile'
  
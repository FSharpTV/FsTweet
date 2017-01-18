module FsTweet.Web.UserProfile
open Login
open Suave.DotLiquid
open FsTweet.Domain.User
open Suave

let renderUserProfilePage username =
  let renderProfile' (user : User) =
    page "user_home.liquid" user.Username.Value
  secured (Redirection.FOUND loginPath) renderProfile'
  
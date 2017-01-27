module FsTweet.Web.Follow
open Suave
open Suave.Operators
open Suave.Filters
open Login
open FsTweet.Domain.User


let handleFollowUser followUser (user : User) ctx = async { 
  return Some ctx
}
let Follow followUser = 
  choose [
    path "/follow" >=> POST 
      >=> secured (redirectToLoginPage "Please login to follow") (handleFollowUser followUser)
  ]
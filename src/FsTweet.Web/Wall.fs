module FsTweet.Web.Wall

open FsTweet.Domain.User
open Suave.DotLiquid
open Login
open Suave.Filters
open Suave.Operators
open Suave
open JsonCombinators
let userWallPage = "user_wall.liquid"
let guestHomePage = "guest_home.liquid"
let userwall (user : User) =
  page userWallPage user.Username.Value

let handleWall getWallPosts (user : User) ctx = async {
  let! result = getWallPosts user.Username
  match result with
  | Ok posts -> return! JSON posts ctx
  | Error err -> return! ServerErrors.INTERNAL_ERROR err ctx
}
let UserHome getWallPosts = 
  choose [
    path "/" >=> secured (page guestHomePage  "") userwall
    path "/wall" >=> secured (RequestErrors.FORBIDDEN "please log in..") (handleWall getWallPosts)]
module FsTweet.Web.Follow
open Suave
open Suave.Operators
open Suave.Filters
open Login
open FsTweet.Domain.User
open JsonCombinators

let unAuthorized = RequestErrors.UNAUTHORIZED "Please login..."

let handleFollowUser followUser (user : User) ctx = async { 
  match ctx.request.["username"] with
  | Some name -> 
    match Username.TryCreate name with
    | Ok username ->
      let! result = followUser user.Username username 
      match result with
      | Ok _ -> return! Successful.OK "success" ctx
      | Error err -> return! ServerErrors.INTERNAL_ERROR err ctx
    | Error errs -> return! RequestErrors.BAD_REQUEST errs.Head ctx
  | None -> return! RequestErrors.BAD_REQUEST "username not found" ctx
}

let returnUsernames f username ctx = async {
  match Username.TryCreate username with
  | Ok username ->
    let! result = f username
    match result with
    | Ok xs -> return! JSON xs ctx
    | Error err -> return! ServerErrors.INTERNAL_ERROR err ctx
  | Error errs -> return! RequestErrors.BAD_REQUEST errs.Head ctx
}
let Follow followUser getFollowers getFollowing =   
  choose [
    path "/follow" >=> POST 
      >=> secured unAuthorized (handleFollowUser followUser)
    pathScan "/followers/%s" (returnUsernames getFollowers)
    pathScan "/following/%s" (returnUsernames getFollowing)
  ]
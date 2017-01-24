module FsTweet.Web.Tweet

open Suave.Operators
open Login
open Suave
open Suave.Filters
open FsTweet.Domain.Tweet
open Suave.RequestErrors
open Suave.ServerErrors
open FsTweet.Domain.User
open Suave.Successful

let handlePostTweet createTweet (user : User) ctx = async {
  match ctx.request.["tweet"] with
  | Some t ->
    match Tweet.TryCreate t with
    | Ok tweet -> 
      let! createTweetResult = createTweet user.Username tweet
      match createTweetResult with
      | Ok post -> return! OK "posted" ctx
      | Error err -> return! INTERNAL_ERROR err ctx
    | Error err -> return! BAD_REQUEST err ctx
  | None -> return! BAD_REQUEST "tweet not available" ctx
}
let Tweet createTweet = 
  path "/tweet" >=> POST >=> secured (FORBIDDEN "please login to tweet") (handlePostTweet createTweet)
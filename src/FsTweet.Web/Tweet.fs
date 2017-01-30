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
open JsonCombinators

let handleGetUserTweets getTweets username ctx = async {
  match Username.TryCreate username with
  | Ok username -> 
    let! getTweetsResult = getTweets username
    match getTweetsResult with
    | Ok tweets -> return! JSON tweets ctx
    | Error err -> return! INTERNAL_ERROR err ctx
  | Error err -> return! BAD_REQUEST err.Head ctx
} 

let handlePostTweet createTweet (user : User) ctx = async {
  match ctx.request.["tweet"] with
  | Some t ->
    match Tweet.TryCreate t with
    | Ok tweet -> 
      let! createTweetResult = createTweet user.Username tweet
      match createTweetResult with
      | Ok _ -> return! OK "posted" ctx
      | Error err -> return! INTERNAL_ERROR err ctx
    | Error err -> return! BAD_REQUEST err ctx
  | None -> return! BAD_REQUEST "tweet not available" ctx
}
let Tweet createTweet getTweets = 
  choose [
    path "/tweets" >=> POST >=> secured (FORBIDDEN "please login to tweet") (handlePostTweet createTweet)
    pathScan "/tweets/%s" (handleGetUserTweets getTweets)]
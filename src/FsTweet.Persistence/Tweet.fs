module FsTweet.Persistence.Tweet
open System.Collections.Generic
open FsTweet.Domain.User
open FsTweet.Domain.Tweet
open ResultExtensions
open FsTweet.Persistence.User
open System
let private tweets = new Dictionary<Username, Post list>()

let createPost username tweet = asyncResult {
  let! userFindResult = getUserByUsername username
  match userFindResult with
  | None -> return Error "invalid user id"
  | Some user ->
    let post = { Tweet = tweet; Username = username; Time = DateTimeOffset.Now }
    match tweets.TryGetValue username with
    | true, posts ->       
      tweets.[username] <- post :: tweets.[username]
      return Ok post
    | _ -> 
      tweets.[username] <- [post]
      return Ok post
}

let getTweets username = 
  match tweets.TryGetValue username with
  | true, posts -> Ok posts |> async.Return
  | _ -> Ok [] |> async.Return
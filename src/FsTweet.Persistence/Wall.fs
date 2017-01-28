module FsTweet.Persistence.Wall

open System.Collections.Generic
open FsTweet.Domain.Tweet
open FsTweet.Domain.User
open User

let private wall = new Dictionary<Username, Post list>()

let addToUserWall post username =
  match wall.TryGetValue username with
  | true, _ -> 
    wall.[username] <- [post] @ wall.[username]
  | _ -> wall.[username] <- [post]
let addPost post usernames  =
  usernames
  |> List.iter (addToUserWall post)
  |> ok

let getWallPosts username =
  match wall.TryGetValue username with
  | true, posts ->  ok posts
  | _ -> ok []

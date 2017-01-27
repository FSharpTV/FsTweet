module FsTweet.Persistence.Follow
open System.Collections.Generic
open FsTweet.Domain.User
open System
open User

let followers = new Dictionary<Username, Username list>()
let following = new Dictionary<Username, Username list>()


let getFollowers username =
  match followers.TryGetValue username with
  | true, followers -> followers |> ok
  | _ -> ok [] 
let getFollowing username =
  match following.TryGetValue username with
  | true, following -> following |> ok
  | _ -> ok [] 

let isFollowing myUsername username =
  match following.TryGetValue myUsername with
  | true, xs -> List.contains username xs |> ok
  | _ -> false |> ok
let followUser myUsername username =
  match following.TryGetValue myUsername with
  | true, xs -> 
    match List.contains username xs with
    | true -> ()
    | _ -> 
      following.[myUsername] <- [username] @ following.[myUsername]
  | _ -> 
    following.Add(myUsername, [username])
  match followers.TryGetValue username with
  | true, xs -> 
    match List.contains myUsername xs with
    | true -> ()
    | _ -> 
      followers.[username] <- [myUsername] @ followers.[username]
  | _ -> 
    followers.Add(username, [myUsername])
  ok ()
  

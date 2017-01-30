module FsTweet.Domain.Tweet

open User
open System

type Tweet = private Tweet of string with
  member this.Value =
    let (Tweet tweet) = this
    tweet
  static member TryCreate (tweet : string) =
    match tweet with
    | null | ""  -> Error "tweet should be empty"
    | x when x.Length > 140 -> Error "tweet should not contain more than 140 characters"
    | x -> Tweet x |> Ok


type Post = {
  Username : Username
  Time: DateTimeOffset
  Tweet : Tweet
}
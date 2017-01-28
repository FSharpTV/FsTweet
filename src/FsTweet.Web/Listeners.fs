module FsTweet.Web.Listeners

open FsTweet.Domain.Tweet
open ResultExtensions

type WallMessage =
| TweetPosted of Post

let handleNewPost getFollowers addPost (post : Post) = asyncResult {
  let! followers = getFollowers post.Username
  return! addPost post followers
}

let onTweetListener getFollowers addPost = MailboxProcessor.Start(fun inbox ->
  let rec loop () = async {
    let! msg = inbox.Receive()
    match msg with
    | TweetPosted post -> 
      let! result = handleNewPost getFollowers addPost post
      match result with
      | Error err -> printfn "%A" err
      | Ok _ -> ()
      return! loop ()
  }
  loop () )

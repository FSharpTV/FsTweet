module FsTweet.Web.Wall

open FsTweet.Domain.User
open Suave.DotLiquid
let userWallPage = "user_wall.liquid"
let Userwall (user : User) =
  page userWallPage user.Username.Value
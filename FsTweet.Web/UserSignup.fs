module FsTweet.Web.UserSignup

open Suave
open Suave.Filters
open Suave.Operators
open Suave.DotLiquid

let UserSignup = 
  choose [
    path "/signup" >=> page "signup.html" ""
  ] 
module FsTweet.Web.SessionCombinators
open Suave.State.CookieStateStore
open Suave
open Suave.Operators
open Suave.Cookie
open Suave.Authentication

let sessionRelativeExpiry = 
  Session
  //MaxAge (System.TimeSpan.FromDays(7.))

let sessionSet failureF key value = 
  stateful sessionRelativeExpiry false
  >=> context (fun ctx ->
                match HttpContext.state ctx with
                | Some state -> state.set key value
                | _ -> failureF
              )

let sessionGet failureF key successF = 
  stateful sessionRelativeExpiry false 
  >=> context (fun ctx ->
                match HttpContext.state ctx with
                | Some store -> 
                  match store.get key with
                  | Some value -> successF value
                  | _ -> failureF
                | _ -> failureF
  )

let clearSession = 
  unsetPair SessionAuthCookie
    >=> unsetPair StateCookie
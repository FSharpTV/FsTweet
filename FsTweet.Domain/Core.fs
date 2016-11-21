﻿module FsTweet.Domain.Core

type Error =
| PersistenceError of string
| RequestError of string

type Result<'T,'E> with
  static member apply fR xR =
    match fR, xR with
    | Ok f, Ok x -> f x |> Ok
    | Error e1, Error e2 -> Error (e1 @ e2)
    | _, Error e -> Error e
    | Error e, _ -> Error e

module ResultOperators = 
  let inline (<*>) fR xR = Result.apply fR xR
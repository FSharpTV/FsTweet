module ResultExtensions

type Result<'T,'E> with
  static member apply fR xR =
    match fR, xR with
    | Ok f, Ok x -> f x |> Ok
    | Error e1, Error e2 -> Error (e1 @ e2)
    | _, Error e -> Error e
    | Error e, _ -> Error e

let mapAsyncOkResult f asyncResult = async {
 let! result = asyncResult
 match result with
 | Ok x -> return f x
 | Error err -> return Error err
}
let inline (<*>) fR xR = Result.apply fR xR
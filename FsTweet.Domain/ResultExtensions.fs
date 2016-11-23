module ResultExtensions

type Result<'T,'E> with
  static member Apply fR xR =
    match fR, xR with
    | Ok f, Ok x -> f x |> Ok
    | Error e1, Error e2 -> Error (e1 @ e2)
    | _, Error e -> Error e
    | Error e, _ -> Error e

  static member Map f xR =
    match xR with
    | Ok x -> Ok (f x)
    | Error e -> Error e 

let inline (<*>) fR xR = Result.Apply fR xR

let mapAsyncOkResult f asyncResult = async {
 let! result = asyncResult
 match result with
 | Ok x -> return f x
 | Error err -> return Error err
}

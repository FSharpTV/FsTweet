module ResultExtensions

type Result<'T,'E> with
  static member Apply fR xR =
    match fR, xR with
    | Ok f, Ok x -> f x |> Ok
    | Error e1, Error e2 -> Error (e1 @ e2)
    | _, Error e -> Error e
    | Error e, _ -> Error e

let inline (<*>) fR xR = Result.Apply fR xR

type AsyncResultBuilder () =    
    member this.Bind(x, f) =
        async {
            let! x' = x
            match x' with
            | Ok s -> return! f s
            | Error f -> return Error f }
    // 'a -> 'a
    member this.Return x = async {return x}
    member this.ReturnFrom x = x

 
let asyncResult = AsyncResultBuilder ()
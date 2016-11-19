module FsTweet.Domain.AsynResult

let puree<'a, 'b> = Ok >> async.Return

let apply fAR xAR = async{
  let! fR = fAR
  match fR with
  | Ok f ->
    let! xA = xAR
    match xA with
    | Ok x -> return! f x
    | Error errs -> return Error errs
  | Error errs -> return Error errs
}

let inline (<*>) fAR xAR = apply fAR xAR
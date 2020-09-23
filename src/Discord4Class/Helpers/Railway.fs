module Discord4Class.Helpers.Railway

let (>>=) train action =
    Result.bind action train

let (=<<) action train =
    Result.bind action train

let switch action train =
    Ok (action train)

let onError =
    Result.mapError

let getResult =
    function
    | Ok x    -> x
    | Error x -> x

let errorBe v = function
    | Ok x -> x
    | Error _ -> v

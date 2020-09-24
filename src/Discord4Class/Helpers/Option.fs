namespace Discord4Class.Helpers

module Option =

    let NoneBe a = function
        | Some v -> v
        | None -> a

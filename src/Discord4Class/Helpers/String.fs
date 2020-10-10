namespace Discord4Class.Helpers

module String =

    let isNumeric =
        String.forall (fun c ->
            let diff = (int) c - 48
            0 <= diff && diff < 10
        )

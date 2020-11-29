namespace Discord4Class.Helpers

open System

module Random =

    let randomPermute arr =
        let rand = Random()
        arr
        |> Array.iteri (fun i _ ->
            let rnd = rand.Next(0, arr.Length)
            let tmp = arr.[i]
            arr.[i] <- arr.[rnd]
            arr.[rnd] <- tmp )
        arr

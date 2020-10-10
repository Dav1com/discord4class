namespace Discord4Class.Helpers

open System

module Random =

    let randomIndexes hi =
        let rand = Random()
        let output = [|0..hi-1|]
        output
        |> Array.iteri (fun i _ ->
            let rnd = rand.Next(0, hi);
            let tmp = output.[i]
            output.[i] <- output.[rnd]
            output.[rnd] <- tmp
        )
        output

namespace Discord4Class.Repositories

open MongoDB.Driver
open System.Collections.Generic
open Discord4Class.Db

module RateLimits =

    [<Literal>]
    let private collectionName = "RateLimits"

    type GetRateLimitError =
        | IdNotFound
        | RateLimitNotFound

    type RateLimit =
        { Count: byte
          Reset: uint64 } // unix timestamp

    [<CLIMutable>]
    type RTDocument =
        { Id: uint64
          Values: IDictionary<string, seq<RateLimit>> }

    type RateLimits =
        { Id: uint64
          Values: Map<string, RateLimit[]> }

        static member private rtToDocument rt =
            { Id = rt.Id
              Values =
                rt.Values
                |> Map.map (fun key value -> value :> seq<_>)
                :> IDictionary<_, _> }
            : RTDocument

        static member private documentToRt (rt: RTDocument) =
            { Id = rt.Id
              Values =
                rt.Values
                |> Seq.map (fun kv -> (kv.Key, kv.Value |> Array.ofSeq))
                |> Map.ofSeq }

        static member Filter = Builders<RTDocument>.Filter
        static member Update = Builders<RTDocument>.Update
        static member Projection = Builders<RTDocument>.Projection
        static member Operation =
            Operation(collectionName, RT.rtToDocument, RT.documentToRt, (fun rt -> rt.Id))

        static member GetRateLimit db guildId commandName =
            RT.Projection.Expression(fun rtd -> rtd.Values.[commandName])
            |> RT.Operation.FindByIdProjection db guildId
            |> Async.RunSynchronously
            |> Seq.toList
            |> function
                | [] -> Error IdNotFound
                | [null] -> Error RateLimitNotFound
                | seqRt -> Ok seqRt.[0]

        static member UpdateAddToCounters commandName adds =
            // someArray.[-1] on LINQ expression seems not to work with F#
            // arrays
            RT.Update.Combine [
                for i in adds do
                    StringFieldDefinition<RTDocument, byte>
                        $"Values.%s{commandName}.%i{i}.Count"
                    |> fun f -> Builders.Update.Inc(f, 1uy) ]

        static member UpdateReset commandName index reset =
            let field =
                StringFieldDefinition<RTDocument, RateLimit>
                    $"Values.%s{commandName}.%i{index}"
            Builders.Update.Set(field, {Count = 1uy; Reset = reset})

        static member ResetAndAddToCounters db guildId commandName resets adds =
            match (resets, adds) with
            | ([], []) -> async.Zero()
            | (resets, adds) ->
                RT.Update.Combine [
                    for (i, reset) in resets do
                        RT.UpdateReset commandName i reset
                    if adds <> [] then
                        RT.UpdateAddToCounters commandName adds ]
                |> RT.Operation.UpdateOneById db guildId
                |> Async.Ignore

    and RT = RateLimits

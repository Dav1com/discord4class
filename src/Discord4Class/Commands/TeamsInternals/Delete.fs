namespace Discord4Class.Commands.TeamsInternals

open DSharpPlus.Entities
open Discord4Class.Commands.TeamsInternals.Predicates

[<AutoOpen>]
module Delete =
    let deleteChannels config (guild : DiscordGuild) =
        guild.Channels
        |> Seq.map (fun kv -> kv.Value)
        |> Seq.filter (isTeamChannel config)
        |> fun x -> printfn "TEST: %A" x; x
        |> Seq.map (fun ch ->
            ch.DeleteAsync()
            |> Async.AwaitTask
        )
        |> Async.Parallel |> Async.RunSynchronously |> ignore

    let deleteRoles (roles : DiscordRole array) =
        roles
        |> Array.map (fun role ->
            role.DeleteAsync()
            |> Async.AwaitTask
        )
        |> Async.Parallel |> Async.RunSynchronously |> ignore

namespace Discord4Class.Commands.TeamsInternals

open DSharpPlus
open DSharpPlus.Entities
open Discord4Class.Commands.TeamsInternals.Predicates

[<AutoOpen>]
module Delete =

    let deleteChannels config (guild: DiscordGuild) =
        guild.GetChannelsAsync()
        |> Async.AwaitTask |> Async.RunSynchronously
        |> Seq.filter (isTeamChannel config)
        |> Seq.sortBy (fun ch ->
            match ch.Type with
            | ChannelType.Category -> 1
            | _ -> 0 )
        |> Seq.map (fun ch -> async {
            try
                ch.DeleteAsync()
                |> Async.AwaitTask |> Async.RunSynchronously
            with | _ -> () } )
        |> Async.Sequential |> Async.RunSynchronously |> ignore

    let deleteRoles (roles: Map<_,DiscordRole>) =
        roles
        |> Map.toSeq
        |> Seq.map (fun (_, role) ->
            role.DeleteAsync()
            |> Async.AwaitTask )
        |> Async.Parallel |> Async.RunSynchronously |> ignore

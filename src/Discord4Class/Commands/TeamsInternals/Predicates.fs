namespace Discord4Class.Commands.TeamsInternals

open DSharpPlus.Entities
open Discord4Class.Config.Types

[<AutoOpen>]
module Predicates =

    let isTeamRole config (role : DiscordRole) =
        (
            config.Guild.Lang.TeamsNumberIsRight = "1" &&
            role.Name.StartsWith config.Guild.Lang.Team
        ) ||
        (
            config.Guild.Lang.TeamsNumberIsRight = "0" &&
            role.Name.EndsWith config.Guild.Lang.Team
        )

    let isTeamChannel config (channel : DiscordChannel) =
        (
            config.Guild.Lang.TeamsNumberIsRight = "1" &&
            channel.Name.ToLower().StartsWith (config.Guild.Lang.Team.ToLower())
        ) ||
        (
            config.Guild.Lang.TeamsNumberIsRight = "0" &&
            channel.Name.ToLower().EndsWith (config.Guild.Lang.Team.ToLower())
        )

    let existsTeams config (e : DiscordGuild) =
        e.Roles
        |> Seq.map (fun kv -> kv.Value)
        |> Seq.tryFind (isTeamRole config)
        |> function
            | Some _ -> true
            | None -> false

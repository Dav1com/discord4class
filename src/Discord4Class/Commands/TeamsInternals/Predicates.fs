namespace Discord4Class.Commands.TeamsInternals

open DSharpPlus.Entities
open Discord4Class.Config.Types

[<AutoOpen>]
module Predicates =

    let isTeamRole guild (role: DiscordRole) =
        ( guild.Lang.TeamsNumberIsRight = "1" &&
          role.Name.StartsWith guild.Lang.Team )
        ||
        ( guild.Lang.TeamsNumberIsRight = "0" &&
          role.Name.EndsWith guild.Lang.Team )

    let isTeamChannel guild (channel: DiscordChannel) =
        ( guild.Lang.TeamsNumberIsRight = "1" &&
          channel.Name.ToLower().StartsWith (guild.Lang.Team.ToLower()) )
        ||
        ( guild.Lang.TeamsNumberIsRight = "0" &&
          channel.Name.ToLower().EndsWith (guild.Lang.Team.ToLower()) )

    let existsTeams config (e: DiscordGuild) =
        e.Roles
        |> Seq.tryFind (fun kv -> isTeamRole config kv.Value)
        |> fun o -> o.IsSome

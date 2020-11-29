namespace Discord4Class.Helpers

open DSharpPlus.Entities

module Roles =

    let getNonManagedRoles (guild: DiscordGuild) =
        guild.Roles
        |> Seq.filter (fun kv -> kv.Value.IsManaged)
        |> Seq.toList

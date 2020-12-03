namespace Discord4Class.Commands.TeamsInternals

open DSharpPlus.Entities

[<AutoOpen>]
module Results =

    type GroupingError =
        | UserHasMultipleTeams of DiscordMember

    type GroupingChannelResult =
        | OkChannel of string * DiscordChannel
        | ChannelNotFound of string

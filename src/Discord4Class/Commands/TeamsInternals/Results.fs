namespace Discord4Class.Commands.TeamsInternals

open DSharpPlus.Entities

[<AutoOpen>]
module Results =
    type GroupingResult =
        | Ok of string * DiscordMember array
        | UserHasMultipleTeams of string
        | MissingTeamVoiceChannel of string
        | UserHasDuplicateTeam of string
        | UserHasNoGroup

    type GroupingChannelResult =
        | OkChannel of string * DiscordChannel
        | ChannelNotFound of string

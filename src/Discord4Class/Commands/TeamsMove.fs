namespace Discord4Class.Commands

open DSharpPlus
open DSharpPlus.EventArgs
open Discord4Class.Exceptions
open Discord4Class.Helpers.Messages
open Discord4Class.Config.Types
open Discord4Class.Commands.TeamsInternals
open Discord4Class.CommandsManager

module TeamsMove =

    let main app guild client args memb (e: MessageCreateEventArgs) = async {
        match getTeamsMembers guild e.Guild false true with
        | Ok teams when teams.Count = 0 ->
            guild.Lang.TeamsMoveNoMembers guild.CommandPrefix
            |> sendMessage e.Channel |> ignore
            addReaction e.Message app.Emojis.No
        | Ok teams ->
            match getTeamsChannels guild ChannelType.Voice e.Guild with
            | channels when channels.Count = 0 ->
                guild.Lang.TeamsMoveNoChannels guild.CommandPrefix
                |> sendMessage e.Channel |> ignore
                addReaction e.Message app.Emojis.No
            | channels ->
                addReaction e.Message app.Emojis.Doing
                moveMembers (e.Guild.Channels.[guild.ClassVoice.Value])
                    channels teams
                changeReaction e.Message app.Emojis.Yes
        | Error _ -> raise TeamsMoveReturnUnexpectedErrorException
    }

    let command =
        { BaseCommand with
            Names = [ "move" ]
            Description = fun gc ->
                gc.Lang.TeamsMoveDescription gc.CommandPrefix gc.Lang.TeamsMoveUsage
            Permissions = Teacher
            RequiredSettings = Settings.ClassVoice
            IsHeavy = true
            RequiresSmallGuild = true
            RateLimits = [
                { Allowed = 3uy
                  Interval = 30UL}
                { Allowed = 30uy
                  Interval = 30UL } ]
            Function = main }

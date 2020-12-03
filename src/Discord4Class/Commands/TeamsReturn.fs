namespace Discord4Class.Commands

open DSharpPlus
open DSharpPlus.EventArgs
open Discord4Class.Exceptions
open Discord4Class.Helpers.Messages
open Discord4Class.Config.Types
open Discord4Class.Commands.TeamsInternals
open Discord4Class.CommandsManager

module TeamsReturn =

    let main app guild client args memb (e: MessageCreateEventArgs) = async {
        match getTeamsMembers guild e.Guild false true with
        | Ok teams when teams.Count = 0 ->
            guild.Lang.TeamsReturnNoMembers guild.CommandPrefix
            |> sendMessage e.Channel |> ignore
            addReaction e.Message app.Emojis.No
        | Ok teams ->
            match getTeamsChannels guild ChannelType.Voice e.Guild with
            | channels when channels.Count = 0 ->
                guild.Lang.TeamsReturnNoChannels guild.CommandPrefix
                |> sendMessage e.Channel |> ignore
                addReaction e.Message app.Emojis.No
            | channels ->
                addReaction e.Message app.Emojis.Doing
                returnMembers (e.Guild.Channels.[guild.ClassVoice.Value])
                    channels teams
                changeReaction e.Message app.Emojis.Yes
        | Error _ -> raise TeamsMoveReturnUnexpectedErrorException
    }

    let command =
        { BaseCommand with
            Names = [ "return" ]
            Description = fun gc ->
                gc.Lang.TeamsReturnDescription gc.CommandPrefix
                    gc.CommandPrefix gc.Lang.TeamsReturnUsage
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

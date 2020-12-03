namespace Discord4Class.Commands

open DSharpPlus.EventArgs
open Discord4Class.Helpers.Messages
open Discord4Class.Config.Types
open Discord4Class.CommandsManager

module Teams =

    let rateLimits = [
        { Allowed = 3uy
          Interval = 30UL} ]

    let description gc = gc.Lang.TeamsDescription gc.Lang.TeamsUsage gc.CommandPrefix

    let main app guild client args memb (e: MessageCreateEventArgs) = async {
        match args with
        | _ :: _ ->
            guild.Lang.TeamsInvalidSubcommand guild.CommandPrefix
            |> sendMessage e.Channel |> ignore
        | _ ->
            guild.Lang.TeamsMissingSubcommand guild.CommandPrefix
            |> sendMessage e.Channel |> ignore
    }

    let command =
        { BaseCommand with
            Names = [ "teams" ]
            Description = fun gc ->
                gc.Lang.TeamsDescription gc.CommandPrefix gc.Lang.TeamsUsage
                    gc.CommandPrefix
            Permissions = Teacher
            RequiredSettings = Settings.TeacherRole
            RateLimits = [
                { Allowed = 3uy
                  Interval = 30UL} ]
            Function = main }

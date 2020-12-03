namespace Discord4Class.Commands

open DSharpPlus.EventArgs
open Discord4Class.Helpers.Messages
open Discord4Class.Config.Types
open Discord4Class.CommandsManager

module QuestionTeacher =

    let main app guild clt args memb (e: MessageCreateEventArgs) = async {
        match args with
        | [] -> guild.Lang.QuestionTeacherMissingSubcommand guild.CommandPrefix
        | _ -> guild.Lang.QuestionTeacherInvalidSubcommand guild.CommandPrefix
        |> sendMessage e.Channel |> ignore
    }

    let command =
        { BaseCommand with
              Names = [ "q" ]
              Description = fun gc ->
                gc.Lang.QuestionTeacherDescription
                    gc.CommandPrefix gc.Lang.QuestionTeacherUsage
              Permissions = Teacher
              MaxArgs = 1
              RateLimits = [
                { Allowed = 1uy
                  Interval = 30UL} ]
              Function = main }

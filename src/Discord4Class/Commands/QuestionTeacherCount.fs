namespace Discord4Class.Commands

open DSharpPlus.EventArgs
open Discord4Class.Helpers.Messages
open Discord4Class.Repositories.Questions
open Discord4Class.Config.Types
open Discord4Class.CommandsManager

module QuestionTeacherCount =

    let main app guild clt args memb (e: MessageCreateEventArgs) = async {
        Qs.GetCounter app.Db e.Guild.Id
        |> Async.RunSynchronously
        |> Seq.tryItem 0
        |> Option.defaultValue 0
        |> guild.Lang.QuestionTeacherCount
        |> sendMessage e.Channel
        |> ignore
    }

    let command =
        { BaseCommand with
            Names = [ "count" ]
            Description = fun gc ->
                gc.Lang.QuestionTeacherCountDescription
                    gc.CommandPrefix gc.Lang.QuestionTeacherCountUsage
            Permissions = Teacher
            RateLimits = [
                { Allowed = 3uy
                  Interval = 30UL} ]
            Function = main }

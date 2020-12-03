namespace Discord4Class.Commands

open DSharpPlus.EventArgs
open Discord4Class.Exceptions
open Discord4Class.Constants
open Discord4Class.Helpers.Messages
open Discord4Class.Repositories.Questions
open Discord4Class.Config.Types
open Discord4Class.CommandsManager

module Question =

    [<Literal>]
    let private maxQuestions = 50

    let main app guild clt (args: string list) memb (e: MessageCreateEventArgs) = async {
        match args with
        | [] ->
            addReaction e.Message app.Emojis.No
        | [question] when question.Length > EmbedDescriptionMaxLength ->
            addReaction e.Message app.Emojis.No
        | [question] ->
            Qs.Projection.Expression(fun qs -> qs.Count)
            |> Qs.Operation.FindByIdProjection app.Db e.Guild.Id
            |> Async.RunSynchronously
            |> Seq.tryItem 0
            |> function
                | None ->
                    Qs.Operation.InsertOne app.Db
                        { Id = e.Guild.Id
                          Count = 1
                          Questions = [
                              { ChId = e.Channel.Id
                                Id = e.Message.Id } ] }
                    |> Async.RunSynchronously
                    addReaction e.Message app.Emojis.Sended
                | Some count ->
                    if count > maxQuestions then
                        addReaction e.Message app.Emojis.No
                    else
                        Qs.PushQuestion app.Db e.Guild.Id
                            { ChId = e.Channel.Id
                              Id = e.Message.Id }
                        |> Async.RunSynchronously |> ignore
                        addReaction e.Message app.Emojis.Sended
        | _ -> raise BadArgumentPhrasingException
    }

    let command =
        { BaseCommand with
            Names = [ "q"; "q-student" ]
            Description = fun gc ->
                gc.Lang.QuestionDescription gc.CommandPrefix gc.Lang.QuestionUsage
            RateLimits = [
                { Allowed = 5uy
                  Interval = 10UL }
                { Allowed = 20uy
                  Interval = 60UL } ]
            Function = main }

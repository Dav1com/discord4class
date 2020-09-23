namespace Discord4Class.EventManagers

open System.Threading.Tasks
open DSharpPlus.CommandsNext.Exceptions
open DSharpPlus.EventArgs
open Discord4Class.Helpers.Railway
open Discord4Class.Config.Types
//open Discord4Class.Database.Driver
open Discord4Class.Commands.Exception
open Discord4Class.BotCommands

module MessageCreated =

    type private CallType =
        | ByMention
        | ByPrefix
        | NoCmd

    let private detectCallType config (e : MessageCreateEventArgs) =
        if e.Message.Content.StartsWith config.Bot.CommandPrefix then
            ByPrefix
        elif config.Bot.CommandByMention && e.Message.Content.StartsWith (e.Client.CurrentUser.Mention.Insert(2, "!")) then
            ByMention
        else
            NoCmd

    let private getCommand config (e : MessageCreateEventArgs) = function
        | ByPrefix ->
            (e.Message.Content.Substring config.Bot.CommandPrefix.Length)
                .Trim()
            |> Ok
        | ByMention ->
            (e.Message.Content.Substring (e.Client.CurrentUser.Mention.Length+1))
                .Trim()
            |> Ok
        | NoCmd -> Error ()

    let exec config (e : MessageCreateEventArgs) =
        try
            if e.Channel.IsPrivate then
                Task.CompletedTask
            elif not e.Author.IsCurrent then

                detectCallType config e
                |> getCommand config e
                >>= switch (fun cmd -> Map.tryFind cmd BotCommands)
                >>= switch (function
                    | Some t -> t
                    | None -> raise (CommandNotFoundException e.Message.Content)
                )
                >>= switch (fun f ->
                    (f config e)
                    |> Async.StartAsTask
                    :> Task
                )
                |> errorBe Task.CompletedTask

            else
                Task.CompletedTask
        with
            | :? CommandNotFoundException as ex -> cmdNotFound config e ex
            | ex -> cmdErrorUnknown config e ex

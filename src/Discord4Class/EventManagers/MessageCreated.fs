namespace Discord4Class.EventManagers

open System.Threading.Tasks
open DSharpPlus.EventArgs
open Discord4Class.Config.Types
open Discord4Class.Config.Loader
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
            (e.Message.Content.Substring config.Guild.CommandPrefix.Length)
                .Trim().Split " "
            |> Array.head
            |> Some
        | ByMention ->
            (e.Message.Content.Substring (e.Client.CurrentUser.Mention.Length+1))
                .Trim().Split " "
            |> Array.head
            |> Some
        | NoCmd -> None

    let exec config (e : MessageCreateEventArgs) =
        try
            if e.Channel.IsPrivate then
                Task.CompletedTask
            elif not e.Author.IsCurrent then
                let guildConf = loadGuildConfiguration config config.App.DbDatabase e.Guild.Id

                detectCallType guildConf e
                |> getCommand guildConf e
                |> function
                    | Some cmd ->
                        cmd
                        |> BotCommands.TryFind
                        |> function
                            | Some f -> f guildConf e
                            | None -> cmdNotFound cmd guildConf e
                    | None ->
                        Task.CompletedTask

            else
                Task.CompletedTask
        with
            | ex -> cmdErrorUnknown config e ex

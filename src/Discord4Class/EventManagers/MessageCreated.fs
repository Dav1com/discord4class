namespace Discord4Class.EventManagers

open System
open System.Threading.Tasks
open DSharpPlus.EventArgs
open Discord4Class.Config.Types
open Discord4Class.Config.Loader
open Discord4Class.Commands.Welcome
open Discord4Class.Commands.Exception
open Discord4Class.Commands.DmResponse
open Discord4Class.BotCommands

module MessageCreated =

    type private CallType =
        | ByMention
        | ByPrefix
        | NoCmd

    type private Command =
      { Name : string
        Args : string }

    let private detectCallType config (e : MessageCreateEventArgs) =
        if e.Message.Content.StartsWith config.Guild.CommandPrefix then
            ByPrefix
        elif config.Bot.CommandByMention && e.Message.Content.StartsWith (e.Client.CurrentUser.Mention.Insert(2, "!")) then
            ByMention
        else
            NoCmd

    let private getCommand config (e : MessageCreateEventArgs) = function
        | ByPrefix ->
            (e.Message.Content.Substring config.Guild.CommandPrefix.Length)
                .Trim()
            |> fun s ->
                s.IndexOf " "
                |> function
                    | -1 ->
                        {
                            Name = s
                            Args = ""
                        }
                    | i ->
                        {
                            Name = s.[..i-1].ToLower()
                            Args = s.[i+1..]
                        }
            |> Some
        | ByMention ->
            (e.Message.Content.Substring (e.Client.CurrentUser.Mention.Length+1))
                .Trim()
            |> fun s ->
                s.IndexOf " "
                |> function
                    | -1 ->
                        {
                            Name = s
                            Args = ""
                        }
                    | i ->
                        {
                            Name = s.[..i-1].ToLower()
                            Args = s.[i+1..]
                        }
            |> Some
        | NoCmd -> None

    let exec config (e : MessageCreateEventArgs) =
        async {
            try
                if e.Channel.IsPrivate then
                    sendDmResponse config e
                elif not e.Author.IsCurrent then
                    let guildConf = loadGuildConfiguration config config.App.DbDatabase e.Guild.Id

                    detectCallType guildConf e
                    |> getCommand guildConf e
                    |> function
                        | Some cmd ->
                            cmd.Name
                            |> BotCommands.TryFind
                            |> function
                                | Some f -> f guildConf cmd.Args e
                                | None -> cmdNotFound cmd.Name guildConf e
                        | None ->
                            async.Zero()
                elif e.Message.Content.Length = 0 then
                    sendWelcome config e
                else
                    async.Zero()
                |> Async.RunSynchronously
            with
                | ex -> cmdErrorUnknown config e ex |> Async.RunSynchronously
        } |> Async.StartAsTask :> Task

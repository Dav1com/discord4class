namespace Discord4Class.EventManagers

open System.Threading.Tasks
open DSharpPlus
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

    let private detectCallType config guildConf (client : DiscordClient) (e : MessageCreateEventArgs) =
        if e.Message.Content.StartsWith guildConf.CommandPrefix then
            ByPrefix
        elif config.Bot.CommandByMention && e.Message.Content.StartsWith (client.CurrentUser.Mention.Insert(2, "!")) then
            ByMention
        else
            NoCmd

    let private getCommand config guildConf (client : DiscordClient) (e : MessageCreateEventArgs) = function
        | ByPrefix ->
            (e.Message.Content.Substring guildConf.CommandPrefix.Length)
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
            (e.Message.Content.Substring (client.CurrentUser.Mention.Length+1))
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

    let exec config client (e : MessageCreateEventArgs) =
        async {
            try
                if e.Channel.IsPrivate then
                    sendDmResponse config.App config.Bot.DefaultLang e
                elif not e.Author.IsCurrent then
                    let guildConf = loadGuildConfiguration config config.App.Db e.Guild.Id

                    detectCallType config guildConf client e
                    |> getCommand config guildConf client e
                    |> function
                        | Some cmd ->
                            cmd.Name
                            |> BotCommands.TryFind
                            |> function
                                | Some f -> f config.App guildConf client cmd.Args e
                                | None -> cmdNotFound cmd.Name guildConf e
                        | None ->
                            async.Zero()
                elif e.Message.Content.Length = 0 then
                    sendWelcome config.App config.Bot.DefaultLang config.Bot.CommandPrefix client e
                else
                    async.Zero()
                |> Async.RunSynchronously
            with
                | ex ->
                    cmdErrorUnknown config.App config.Bot.DefaultLang e ex
                    |> Async.RunSynchronously
        } |> Async.StartAsTask :> Task

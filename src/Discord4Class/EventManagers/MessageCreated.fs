namespace Discord4Class.EventManagers

open System
open DSharpPlus
open DSharpPlus.EventArgs
open Discord4Class.Constants
open Discord4Class.Helpers.Railway
open Discord4Class.Helpers.Messages
open Discord4Class.Config.Types
open Discord4Class.Config.Loader
open Discord4Class.CommandsManager.Exception
open Discord4Class.Commands.Help
open Discord4Class.BotCommands

module MessageCreated =

    type private CallType =
        | ByMention
        | ByPrefix
        | NoCmd

    type private Command =
        { Name : string
          Args : string }

    let private detectCallType config guildConf (client: DiscordClient) (msg: string) =
        if msg.StartsWith guildConf.CommandPrefix then ByPrefix
        elif config.Bot.CommandByMention &&
             msg.StartsWith (client.CurrentUser.Mention.Insert(2, "!")) then
            ByMention
        else NoCmd

    let private getCommandStr guildConf (client: DiscordClient) (msg: string) = function
        | ByPrefix ->
            (msg.Substring guildConf.CommandPrefix.Length)
                .TrimStart()
            |> Some
        | ByMention ->
            (msg.Substring (client.CurrentUser.Mention.Length+1))
                .TrimStart()
            |> Some
        | NoCmd -> None

    let private sendDmResponse app defLang (e: MessageCreateEventArgs) = async {
        app.AllLangs.[defLang].ResponseToDm AppName app.DocsURL
        |> sendMessage e.Channel |> ignore
    }

    let main config client (e: MessageCreateEventArgs) = async {
        try
            if e.Channel.IsPrivate && not e.Author.IsBot then
                if checkDmRateLimits config.App e.Author.Id then
                    dmHelp Commands config.App config.Bot.CommandPrefix
                        config.App.AllLangs.[config.Bot.DefaultLang] e.Channel
                        e.Author
            elif not (e.Author.IsBot || e.Author.IsSystem = Nullable true) then
                let guildConf = loadGuildConfiguration config config.App.Db e.Guild.Id
                detectCallType config guildConf client
                    e.Message.Content
                |> getCommandStr guildConf client e.Message.Content
                |> function
                    | Some cmdStr ->
                        let memb =
                            e.Guild.GetMemberAsync e.Author.Id
                            |> Async.AwaitTask |> Async.RunSynchronously
                        Commands.RunCommandAsync config.App guildConf
                            client memb e cmdStr
                        |> Result.mapError (fun err ->
                            match err.ToMessage guildConf with
                            | Some msg ->
                                sendMessageAsync e.Channel msg
                                |> Async.Ignore
                            | None -> async.Zero() )
                        |> getResult
                        |> Async.RunSynchronously
                    | None -> ()
        with
        | ex ->
            cmdErrorUnknown config.App.AllLangs.[config.Bot.DefaultLang]
                e ex
            |> Async.RunSynchronously
    }

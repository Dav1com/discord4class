namespace Discord4Class

open System
open System.Threading.Tasks
open Microsoft.Extensions.Logging
open Emzi0767.Utilities
open DSharpPlus
open DSharpPlus.EventArgs
open DSharpPlus.Interactivity
open Discord4Class.Config
open Discord4Class.Config.Types
open Discord4Class.EventManagers

module Bot =

    let getDiscordConfig config =
        let dConf = DiscordConfiguration()
        dConf.set_AutoReconnect true

        match config.Preferences.LogLevel with
        | InnerTypes.Off -> LogLevel.Critical
        | InnerTypes.Normal -> LogLevel.Information
        | InnerTypes.Debug -> LogLevel.Debug
        |> dConf.set_MinimumLogLevel

        dConf.set_Token config.Bot.BotToken
        dConf.set_TokenType TokenType.Bot
        dConf

    let closeBot (discord : DiscordClient) _ _ =
        discord.DisconnectAsync ()
        |> Async.AwaitTask
        |> Async.RunSynchronously

    let runBot config = async {

        let discord = new DiscordClient( getDiscordConfig config )

        let iconf = InteractivityConfiguration()
        iconf.set_Timeout <| TimeSpan.FromSeconds 30.0

        discord.UseInteractivity(iconf) |> ignore

        // Commands
        AsyncEventHandler<_, _>(MessageCreated.exec config)
        |> discord.add_MessageCreated
        // Clean DB
        AsyncEventHandler<_, _>(GuildDeleted.exec config)
        |> discord.add_GuildDeleted
        // Mutes non teacher members on muted channels
        AsyncEventHandler<_, _>(VoiceStateUpdated.exec config)
        |> discord.add_VoiceStateUpdated
        // Updates DB
        AsyncEventHandler<_, _>(ChannelDeleted.exec config)
        |> discord.add_ChannelDeleted
        // Updates DB
        AsyncEventHandler<_, _>(GuildRoleDeletes.exec config)
        |> discord.add_GuildRoleDeleted

        do! discord.ConnectAsync() |> Async.AwaitTask
        Console.add_CancelKeyPress (new ConsoleCancelEventHandler (closeBot discord) )
        do! Task.Delay -1 |> Async.AwaitTask
    }

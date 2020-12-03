namespace Discord4Class

open System
open System.Threading.Tasks
open Microsoft.Extensions.Logging
open Emzi0767.Utilities
open DSharpPlus
open DSharpPlus.Interactivity.Extensions
open DSharpPlus.Interactivity
open Discord4Class.Constants
open Discord4Class.Config
open Discord4Class.Config.Loader
open Discord4Class.EventManagers

module Bot =

    let private funAsTask f c e =
        async{ f c e |> Async.Start } |> Async.StartAsTask :> Task

    let private getDiscordConfig (iConfig: InnerTypes.IniConfig) =
        let dConf = DiscordConfiguration()
        dConf.set_AutoReconnect true
        dConf.set_Intents (Nullable <|
              DiscordIntents.DirectMessages
            + DiscordIntents.Guilds
            + DiscordIntents.GuildMessages
            + DiscordIntents.GuildMessageReactions
            + DiscordIntents.GuildVoiceStates      // mute, hands, and attendance
            + DiscordIntents.GuildMembers
            + DiscordIntents.GuildPresences )      // get guild member list cache)

        match iConfig.Preferences.LogLevel with
        | InnerTypes.Critical -> LogLevel.Critical
        | InnerTypes.Informational -> LogLevel.Information
        | InnerTypes.Debug -> LogLevel.Debug
        |> dConf.set_MinimumLogLevel

        dConf.set_LargeThreshold LargeGuildThreshold
        dConf.set_Token iConfig.Bot.BotToken
        dConf.set_TokenType TokenType.Bot
        dConf

    let private closeBot (discord: DiscordClient) _ _ =
        discord.DisconnectAsync()
        |> Async.AwaitTask |> Async.RunSynchronously

    let private initBot config =
        ReadyResumed.initDatabase config 1

    let runBot iniConfig = async {
        let discord = new DiscordClient(getDiscordConfig iniConfig)

        let iconf = InteractivityConfiguration()
        iconf.set_Timeout <| TimeSpan.FromSeconds 30.0
        discord.UseInteractivity(iconf) |> ignore

        let config = buildFullConfig discord iniConfig

        initBot config
        // Init Database
        AsyncEventHandler<_, _>(funAsTask <| ReadyResumed.main config)
        |> discord.add_Resumed
        // Commands
        AsyncEventHandler<_, _>(funAsTask <| MessageCreated.main config)
        |> discord.add_MessageCreated
        // Clean DB
        AsyncEventHandler<_, _>(funAsTask <| GuildDeleted.main config)
        |> discord.add_GuildDeleted
        // Clean DB
        // Mutes non teacher members on muted channels
        AsyncEventHandler<_, _>(funAsTask <| VoiceStateUpdated.main config)
        |> discord.add_VoiceStateUpdated
        // Updates DB
        AsyncEventHandler<_, _>(funAsTask <| ChannelDeleted.main config)
        |> discord.add_ChannelDeleted
        // Updates DB
        AsyncEventHandler<_, _>(funAsTask <| GuildRoleDeleted.main config)
        |> discord.add_GuildRoleDeleted
        // Updates client cache
        AsyncEventHandler<_, _>(funAsTask <| SocketErrored.main config)
        |> discord.add_SocketErrored

        Console.add_CancelKeyPress(new ConsoleCancelEventHandler(closeBot discord))
        do! discord.ConnectAsync() |> Async.AwaitTask
        do! Task.Delay -1 |> Async.AwaitTask
    }

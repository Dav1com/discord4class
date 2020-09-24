namespace Discord4Class

open System
open System.Threading.Tasks
open DSharpPlus
open DSharpPlus.EventArgs
open Discord4Class.Config
open Discord4Class.Config.Types
open Discord4Class.EventManagers

module Bot =

    let Log s (e : DebugLogMessageEventArgs) =
        printfn "[%s] [%s] [%s] %s" (e.Timestamp.ToString("yyyy-MM-dd HH:mm:ss")) (e.Level.ToString()) e.Application e.Message

    let getDiscordConfig config =
        let dConf = DiscordConfiguration()
        dConf.set_AutoReconnect true

        match config.Preferences.LogLevel with
        | InnerTypes.Off -> LogLevel.Critical
        | InnerTypes.Normal -> LogLevel.Info
        | InnerTypes.Debug -> LogLevel.Debug
        |> dConf.set_LogLevel

        dConf.set_Token config.Bot.BotToken
        dConf.set_TokenType TokenType.Bot
        dConf

    let closeBot (discord : DiscordClient) _ _ =
        discord.DisconnectAsync ()
        |> Async.AwaitTask
        |> Async.RunSynchronously

    let runBot config = async {

        let discord = new DiscordClient( getDiscordConfig config )
        discord.DebugLogger.LogMessageReceived.AddHandler(EventHandler<DebugLogMessageEventArgs>(Log))

        AsyncEventHandler<MessageCreateEventArgs>(MessageCreated.exec config)
        |> discord.add_MessageCreated
        //TODO: discord.add_GuildCreated // send welcome message
        //TODO: discord.add_GuildRoleCreated //asks for quick actions
        //TODO: discord.add_GuildRoleDeleted //check roles integrity and updates
        //TODO: discord.add_GuildRoleUpdated //check roles integrity and updates
        //TODO: discord.add_ChannelDeleted //check channels integrity and updates
        //TODO: discord.add_ChannelUpdated //check channels integrity and updates
        //TODO: discord.add_VoiceStateUpdated //assitence service

        do! discord.ConnectAsync() |> Async.AwaitTask
        Console.add_CancelKeyPress (new ConsoleCancelEventHandler (closeBot discord) )
        do! Task.Delay -1 |> Async.AwaitTask
    }

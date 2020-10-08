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
        iconf.set_Timeout <| TimeSpan.FromSeconds 10.0
        //iconf.PaginationBehaviour <- PaginationBehaviour.Ignore

        let interactivity = discord.UseInteractivity(iconf)

        // Commands
        AsyncEventHandler<DiscordClient, MessageCreateEventArgs>(MessageCreated.exec config)
        |> discord.add_MessageCreated
        // Clean database
        AsyncEventHandler<DiscordClient, GuildDeleteEventArgs>(GuildDeleted.exec config)
        |> discord.add_GuildDeleted
        //TODO: discord.add_GuildRoleCreated //asks for quick actions
        //TODO: discord.add_ChannelDeleted //check channels integrity and updates
        //TODO: discord.add_VoiceStateUpdated //assistence service
        //TODO: discord.add_MessageReactionAdded // Teachers mark as solved questions

        do! discord.ConnectAsync() |> Async.AwaitTask
        Console.add_CancelKeyPress (new ConsoleCancelEventHandler (closeBot discord) )
        do! Task.Delay -1 |> Async.AwaitTask
    }

namespace Discord4Class.Commands

open DSharpPlus
open DSharpPlus.Entities
open DSharpPlus.EventArgs
open Discord4Class.Exceptions
open Discord4Class.Helpers.Messages
open Discord4Class.Helpers.Time
open Discord4Class.Lang.Types
open Discord4Class.Config.Types
open Discord4Class.CommandsManager.Manager
open Discord4Class.Repositories.RateLimits
open Discord4Class.CommandsManager.Types

module Help =

    let rateLimits = [
        { Allowed = 5uy
          Interval = 10UL }
        { Allowed = 20uy
          Interval = 60UL } ]

    let dmRateLimits = [
        { Allowed = 1uy
          Interval = 1800UL } ]

    let collectCommandNames separator (BotCommands cmds) =
        (Seq.fold (fun acc cmd -> acc + separator + cmd.Names.[0]) "" cmds).[1..]

    let baseHelp (commands: BotCommands) lang (prefix: string) app =
        let cmdNames =
            commands.ComandNamesByPermission
            |> Map.ofSeq
        [ DiscordEmbedBuilder()
            .WithTitle(lang.HelpGettingStarted)
            .AddField(lang.HelpLanguage, lang.HelpLanguageDescription)
            .AddField(lang.HelpConfiguration, lang.HelpConfigurationDescription)
            .AddField(lang.HelpReadingTheHelp, lang.HelpReadingTheHelpDescription)
            .AddField(lang.HelpSupportServer, lang.HelpSupportServerDescription app.SupportServer)
            .WithColor(DiscordColor.Green)
            .Build()
          DiscordEmbedBuilder()
            .WithTitle(lang.HelpEmbedTitle)
            .WithDescription(lang.HelpEmbedDescription prefix)
            .AddField(lang.Everyone, String.concat "\n" cmdNames.[NoPerms], true)
            .AddField(lang.ManageGuild, String.concat "\n" cmdNames.[GuildPrivileged], true)
            .AddField(lang.Teacher, String.concat "\n" cmdNames.[Teacher], false)
            .WithColor(DiscordColor.Blue)
            .Build()
          DiscordEmbedBuilder()
            .WithTitle(lang.HelpJoinServerTitle)
            .WithDescription(app.JoinGuildURL)
            .WithColor(DiscordColor.Teal)
            .Build() ]

    let help (cmds: BotCommands) app guild (client: DiscordClient) args (memb: DiscordMember) (e: MessageCreateEventArgs) = async {
        match args with
        | [cmd] ->
            cmds.GetCommandNoChecks guild client.CurrentApplication.Owners memb
                e.Author e.Channel cmd
            |> function
                | Ok (command, _) ->
                    match command.Command.Description guild with
                    | "" -> guild.Lang.HelpNotAvailable
                    | msg -> msg
                | Error _ -> guild.Lang.HelpCommandNotFound guild.CommandPrefix
            |> sendMessage e.Channel |> ignore
        | _ -> raise BadCommandConfigException
    }

    [<Literal>]
    let private dmCommandName = "help#U"

    let checkDmRateLimits app authorId =
        match RT.GetRateLimit app.Db authorId dmCommandName with
        | Ok rts ->
            let (RateLimitWaitUntil (waitUntil, resets, adds)) =
                CommandRateLimit.GetWaitUntil rts dmRateLimits
            RT.ResetAndAddToCounters app.Db authorId dmCommandName resets adds
            |> Async.Ignore |> Async.Start
            waitUntil = 0UL
        | Error IdNotFound ->
            CommandRateLimit.Insert app.Db authorId dmCommandName dmRateLimits
            |> Async.Start
            true
        | Error RateLimitNotFound ->
            CommandRateLimit.UpdateAddName app.Db authorId dmCommandName dmRateLimits
            |> Async.Start
            true

    let dmHelp cmds app prefix lang (channel: DiscordChannel) (author: DiscordUser) =
        baseHelp cmds lang prefix app
        |> List.iter (sendEmbed channel >> ignore)

    let dmHelpCommand cmds app guild client args (memb: DiscordMember) (e: MessageCreateEventArgs) = async {
        let! channel = memb.CreateDmChannelAsync() |> Async.AwaitTask
        dmHelp cmds app guild.CommandPrefix guild.Lang channel e.Author
    }

    let private description gc =
        gc.Lang.HelpDescription gc.CommandPrefix gc.Lang.HelpUsage

    let dmCommand botCommands =
        { BaseCommand with
            Names = [ "help" ]
            Description = description
            DisplayOnCommandList = false
            When = fun _ args -> args.Trim().Length = 0
            RateLimits = dmRateLimits
            RateLimitType = UserRateLimit
            Function = dmHelpCommand botCommands }

    let command botCommands =
        { BaseCommand with
            Names = [ "help" ]
            Description = description
            RateLimits = rateLimits
            Function = help botCommands }

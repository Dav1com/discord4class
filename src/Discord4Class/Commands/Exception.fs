namespace Discord4Class.Commands

open System
open System.Threading.Tasks
open DSharpPlus
open DSharpPlus.Entities
open DSharpPlus.CommandsNext
open DSharpPlus.CommandsNext.Exceptions
open DSharpPlus.EventArgs
open Discord4Class.Helpers.Permission
open Discord4Class.Config.Types

module Exception =

    [<Literal>]
    let Perms = Permission.NoPerm

    [<Literal>]
    let private EmbedColor = 0xFF0000

    let private guildErrorEmbed config (client : DiscordUser) message =
        DiscordEmbedBuilder()
            .WithColor( DiscordColor(EmbedColor) )
            .WithAuthor(config.Guild.Lang.ErrorEmbedAuthor)
            .WithFooter(config.Bot.BotName, client.AvatarUrl)
            .WithTimestamp( Nullable<DateTimeOffset>(DateTimeOffset.Now) )
            .WithDescription(message)
            .Build()

    let cmdNotFound config (msg : MessageCreateEventArgs) (e : CommandNotFoundException) =
        printfn "CmdException on command: %s" e.Command
        let embed =
            config.Guild.Lang.ErrorCmdNotFound msg.Author.Mention msg.Message.Content
            |> guildErrorEmbed config msg.Client.CurrentUser
        msg.Channel.SendMessageAsync("", false, embed) :> Task

    let cmdErrorUnknown config (msg : MessageCreateEventArgs) (ex : exn) =
        let embed =
            config.Guild.Lang.ErrorCmdUnknown msg.Author.Mention (ex.GetType().ToString()) ex.Message
            |> guildErrorEmbed config msg.Client.CurrentUser
        msg.Channel.SendMessageAsync("", false, embed) :> Task

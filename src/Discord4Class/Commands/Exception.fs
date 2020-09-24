namespace Discord4Class.Commands

open System
open System.Threading.Tasks
open DSharpPlus.Entities
open DSharpPlus.EventArgs
open Discord4Class.Helpers.Permission
open Discord4Class.Helpers.Embed
open Discord4Class.Config.Types

module Exception =

    [<Literal>]
    let Perms = Permission.NoPerm

    [<Literal>]
    let private EmbedColor = 0xFF0000

    let cmdNotFound cmd config (msg : MessageCreateEventArgs) =
        let embed =
            config.Guild.Lang.ErrorCmdNotFound cmd
            |> newEmbed EmbedColor config.Guild.Lang.ErrorEmbedAuthor
        msg.Channel.SendMessageAsync(msg.Author.Mention, false, embed)
        :> Task

    let cmdErrorUnknown config (msg : MessageCreateEventArgs) (ex : exn) =
        let embed =
            config.Guild.Lang.ErrorCmdUnknown (ex.GetType().ToString()) ex.Message
            |> newEmbed EmbedColor config.Guild.Lang.ErrorEmbedAuthor
        msg.Channel.SendMessageAsync(msg.Author.Mention, false, embed)
        :> Task

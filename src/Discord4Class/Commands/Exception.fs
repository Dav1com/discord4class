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
        async {
            let embed =
                config.Guild.Lang.ErrorCmdNotFound cmd
                |> newEmbed EmbedColor config.Guild.Lang.ErrorEmbedAuthor
            msg.Channel.SendMessageAsync(msg.Author.Mention, false, embed)
            |> Async.AwaitTask
            |> Async.RunSynchronously
            |> ignore
        } |> Async.StartAsTask :> Task

    let cmdErrorUnknown config (msg : MessageCreateEventArgs) (ex : exn) =
        async {
            let embed =
                config.Guild.Lang.ErrorCmdUnknown (ex.GetType().ToString()) ex.Message
                |> newEmbed EmbedColor config.Guild.Lang.ErrorEmbedAuthor
            msg.Channel.SendMessageAsync(msg.Author.Mention, false, embed)
            |> Async.AwaitTask
            |> Async.RunSynchronously
            |> ignore
        } |> Async.StartAsTask :> Task

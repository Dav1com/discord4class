namespace Discord4Class.Commands

open DSharpPlus.EventArgs
open Discord4Class.Helpers.Embed
open Discord4Class.Lang.Types
open Discord4Class.Config.Types

module Exception =

    [<Literal>]
    let private EmbedColor = 0xFF0000

    let cmdNotFound cmd guild (msg : MessageCreateEventArgs) = async {
        let embed =
            guild.Lang.ErrorCmdNotFound cmd
            |> newEmbed EmbedColor guild.Lang.ErrorEmbedAuthor
        msg.Channel.SendMessageAsync(msg.Author.Mention, false, embed)
        |> Async.AwaitTask |> Async.RunSynchronously |> ignore
    }

    let cmdErrorUnknown app defaultLang (msg : MessageCreateEventArgs) (ex : exn) = async {
        let lang = app.AllLangs.[defaultLang]
        let embed =
            lang.ErrorCmdUnknown (ex.GetType().ToString()) ex.Message
            |> newEmbed EmbedColor lang.ErrorEmbedAuthor
        msg.Channel.SendMessageAsync(msg.Author.Mention, false, embed)
        |> Async.AwaitTask |> Async.RunSynchronously |> ignore
    }

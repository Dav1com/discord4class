namespace Discord4Class.CommandsManager

open DSharpPlus.Entities
open DSharpPlus.EventArgs
open Discord4Class.Helpers.Embed
open Discord4Class.Lang.Types
open Discord4Class.Config.Types

module Exception =

    let private embedColor = DiscordColor.Red

    let cmdNotFound cmd guild (msg: MessageCreateEventArgs) = async {
        let embed =
            guild.Lang.ErrorCmdNotFound cmd
            |> newEmbed embedColor guild.Lang.ErrorEmbedAuthor
        msg.Channel.SendMessageAsync(msg.Author.Mention, false, embed)
        |> Async.AwaitTask |> Async.RunSynchronously |> ignore
    }

    let cmdErrorUnknown lang (msg: MessageCreateEventArgs) (ex : exn) = async {
        try
            let embed =
                lang.ErrorCmdUnknown (ex.GetType().ToString()) ex.Message
                |> newEmbed embedColor lang.ErrorEmbedAuthor
            msg.Channel.SendMessageAsync(msg.Author.Mention, false, embed)
            |> Async.AwaitTask |> Async.RunSynchronously |> ignore
        with
        | e ->
            eprintfn "Exception while sending exception message: %s: %s" (e.GetType().ToString()) e.Message
    }

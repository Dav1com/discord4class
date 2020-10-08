namespace Discord4Class.Commands

open System.Threading.Tasks
open DSharpPlus.EventArgs
open Discord4Class.Config.Types

module DmResponse =

    [<Literal>]
    let BotName = "Discord4Class"

    let sendDmResponse config (e : MessageCreateEventArgs) = async {
        config.Guild.Lang.ResponseToDm BotName config.App.DocsURL
        |> fun s -> e.Channel.SendMessageAsync(s)
        |> Async.AwaitTask
        |> Async.RunSynchronously
        |> ignore
    }

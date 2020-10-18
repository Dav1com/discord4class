namespace Discord4Class.Commands

open DSharpPlus.EventArgs
open Discord4Class.Helpers.Messages
open Discord4Class.Config.Types

module DmResponse =

    [<Literal>]
    let BotName = "Discord4Class"

    let sendDmResponse app defLang (e : MessageCreateEventArgs) = async {
        app.AllLangs.[defLang].ResponseToDm BotName app.DocsURL
        |> sendMessage e.Channel |> ignore
    }

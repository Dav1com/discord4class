namespace Discord4Class.Commands

open DSharpPlus
open DSharpPlus.EventArgs
open Discord4Class.Helpers.Messages
open Discord4Class.Config.Types

module Welcome =

    let sendWelcome app defaultLang defaultPrefix (client : DiscordClient) (e : MessageCreateEventArgs) = async {
        app.AllLangs.[defaultLang].JoinGuildMessage client.CurrentUser.Username
            defaultPrefix app.DocsURL
        |> sendMessage e.Channel |> ignore
    }

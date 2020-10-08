namespace Discord4Class.Commands

open System.Threading.Tasks
open DSharpPlus
open DSharpPlus.EventArgs
open Discord4Class.Config.Types

module Welcome =

    let sendWelcome config (client : DiscordClient) (e : MessageCreateEventArgs) = async {
        config.Guild.Lang.JoinGuildMessage client.CurrentUser.Username config.Guild.CommandPrefix config.App.DocsURL
        |> fun s -> e.Channel.SendMessageAsync(s)
        |> Async.AwaitTask
        |> Async.RunSynchronously
        |> ignore
    }

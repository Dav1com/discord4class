namespace Discord4Class.Commands

open System.Threading.Tasks
open DSharpPlus.EventArgs
open Discord4Class.Config.Types

module Welcome =

    let sendWelcome config (e : MessageCreateEventArgs) =
        async {
            config.Guild.Lang.JoinGuildMessage e.Client.CurrentUser.Username config.Guild.CommandPrefix config.App.DocsURL
            |> fun s -> e.Channel.SendMessageAsync(s)
            |> Async.AwaitTask
            |> Async.RunSynchronously
            |> ignore
        } |> Async.StartAsTask :> Task

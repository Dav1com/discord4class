namespace Discord4Class.Commands

open System.Threading.Tasks
open DSharpPlus
open DSharpPlus.EventArgs
open Discord4Class.Config.Types

module Ping =

    let exec config (e : MessageCreateEventArgs) =
        async {
            return!
                (e.Client :?> DiscordClient).Ping
                |> config.Guild.Lang.PingSuccess
                |> (fun x -> e.Channel.SendMessageAsync(x) )
                |> Async.AwaitTask
        } |> Async.StartAsTask :> Task

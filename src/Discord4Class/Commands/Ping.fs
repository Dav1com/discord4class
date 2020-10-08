namespace Discord4Class.Commands

open System.Threading.Tasks
open DSharpPlus
open DSharpPlus.EventArgs
open Discord4Class.Config.Types

module Ping =

    let exec config (client : DiscordClient) _ (e : MessageCreateEventArgs) = async {
        client.Ping
        |> config.Guild.Lang.PingSuccess
        |> (fun x -> e.Channel.SendMessageAsync(x) )
        |> Async.AwaitTask |> Async.RunSynchronously |> ignore
    }

namespace Discord4Class.Commands

open System
open DSharpPlus.EventArgs
open Discord4Class.Config.Types

module Ping =

    let exec config (e : MessageCreateEventArgs) = async {
        let now = DateTime.UtcNow

        do! e.Channel.TriggerTypingAsync() |> Async.AwaitTask

        do!
            now.Subtract(e.Message.Timestamp.UtcDateTime).Milliseconds
            |> config.Guild.Lang.PingSuccess
            |> (fun x -> e.Channel.SendMessageAsync(x) )
            |> Async.AwaitTask
            |> Async.Ignore
    }

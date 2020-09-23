namespace Discord4Class

open DSharpPlus.EventArgs
open Discord4Class.Config.Types
open Discord4Class.Commands

module BotCommands =

    let (BotCommands : Map<string, Config -> MessageCreateEventArgs -> Async<unit>>) =
        [
            ("ping", Ping.exec)
        ]
        |> Map.ofSeq

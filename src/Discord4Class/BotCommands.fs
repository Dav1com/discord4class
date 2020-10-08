namespace Discord4Class

open DSharpPlus
open DSharpPlus.EventArgs
open Discord4Class.Config.Types
open Discord4Class.Commands

module BotCommands =

    let (BotCommands : Map<string, Config -> DiscordClient -> string -> MessageCreateEventArgs -> unit Async>) =
        [
            ("ping", Ping.exec)
            ("lang", Lang.exec)
            ("language", Lang.exec)
            ("prefix", Prefix.exec)
            ("init", Init.exec)
            ("destroy", Destroy.exec)
            ("config", Config.exec)
        ]
        |> Map.ofSeq

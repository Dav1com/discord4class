namespace Discord4Class

open System.Threading.Tasks
open DSharpPlus.EventArgs
open Discord4Class.Config.Types
open Discord4Class.Commands

module BotCommands =

    let (BotCommands : Map<string, Config -> MessageCreateEventArgs -> Task>) =
        [
            ("ping", Ping.exec)
            ("lang", Lang.exec)
            ("language", Lang.exec)
            ("prefix", Prefix.exec)
            ("init", Init.exec)
            ("destroy", Destroy.exec)
        ]
        |> Map.ofSeq

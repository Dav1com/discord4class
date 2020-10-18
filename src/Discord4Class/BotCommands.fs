namespace Discord4Class

open DSharpPlus
open DSharpPlus.EventArgs
open Discord4Class.Config.Types
open Discord4Class.Commands

module BotCommands =

    let (BotCommands : Map<string, AppConfig -> GuildConfig -> DiscordClient -> string -> MessageCreateEventArgs -> unit Async>) =
        [
            ("ping", Ping.exec)
            ("lang", Lang.exec)
            ("language", Lang.exec)
            ("prefix", Prefix.exec)
            ("init", Init.exec)
            ("destroy", Destroy.exec)
            ("config", Config.exec)
            ("q", Question.exec)
            ("question", Question.exec)
            ("teams", Teams.exec)
            ("mute", Mute.exec)
            ("math", LatexMath.exec)
        ]
        |> Map.ofSeq

namespace Discord4Class.Commands

open System.Threading.Tasks
open DSharpPlus
open DSharpPlus.EventArgs
open Discord4Class.Config.Types

module Ping =

    let exec config (e : MessageCreateEventArgs) =
        // Weird enought, e.Client.CurrentClient is not a DiscordClient (it doesn't have the Ping property)
        // I found no other way to extract the ping from e
        (e.Client :?> DiscordClient).Ping
        |> config.Guild.Lang.PingSuccess
        |> (fun x -> e.Channel.SendMessageAsync(x) )
        :> Task

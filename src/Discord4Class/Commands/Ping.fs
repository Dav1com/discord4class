namespace Discord4Class.Commands

open System.Threading.Tasks
open DSharpPlus
open DSharpPlus.EventArgs
open Discord4Class.Helpers.Messages
open Discord4Class.Config.Types

module Ping =

    let exec app guild (client : DiscordClient) _ (e : MessageCreateEventArgs) = async {
        client.Ping
        |> guild.Lang.PingSuccess
        |> sendMessage e.Channel |> ignore
    }

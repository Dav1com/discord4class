namespace Discord4Class.Commands

open DSharpPlus
open DSharpPlus.EventArgs
open Discord4Class.Helpers.Messages
open Discord4Class.Config.Types
open Discord4Class.CommandsManager

module Ping =

    let main app guild (client: DiscordClient) _ memb (e: MessageCreateEventArgs) = async {
        client.Ping
        |> guild.Lang.PingSuccess
        |> sendMessage e.Channel |> ignore
    }

    let command =
        { BaseCommand with
            Names = [ "ping" ]
            Description = fun gc ->
                gc.Lang.PingDescription gc.CommandPrefix gc.Lang.PingUsage
            RateLimits = [
                { Allowed = 2uy
                  Interval = 10UL } ]
            Function = main }

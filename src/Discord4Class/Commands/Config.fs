namespace Discord4Class.Commands

open DSharpPlus.EventArgs
open Discord4Class.Helpers.Messages
open Discord4Class.Config.Types
open Discord4Class.CommandsManager

module Config =

    let main app guild client args memb (e: MessageCreateEventArgs) = async {
        match args with
        | _ :: _ ->
            guild.Lang.ConfigInvalidSubcommand guild.CommandPrefix
            |> sendMessage e.Channel |> ignore
        | _ ->
            guild.Lang.ConfigMissingSubcommand guild.CommandPrefix
            |> sendMessage e.Channel |> ignore
    }

    let command =
        { BaseCommand with
            Names = [ "config" ]
            Description = fun gc ->
                gc.Lang.ConfigDescription
                    gc.CommandPrefix gc.Lang.ConfigUsage gc.Lang.ConfigNames
            Permissions = GuildPrivileged
            RateLimits = [
                { Allowed = 3uy
                  Interval = 30UL } ]
            Function = main
            MaxArgs = 1 }

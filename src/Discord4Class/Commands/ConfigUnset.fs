namespace Discord4Class.Commands

open System
open DSharpPlus.EventArgs
open Discord4Class.Helpers.Messages
open Discord4Class.Repositories.GuildData
open Discord4Class.Config.Types
open Discord4Class.CommandsManager

module ConfigUnset =

    let main app guild client args memb (e: MessageCreateEventArgs) = async {
        match args with
        | configName :: _ when guild.IsConfigOnDb ->
            match Map.tryFind configName GD.configUpdaters with
            | Some update ->
                GD.Operation.UpdateOneById app.Db e.Guild.Id
                    (update <| Nullable<_>())
                |> Async.RunSynchronously |> ignore
                addReaction e.Message app.Emojis.Yes
            | None ->
                addReaction e.Message app.Emojis.No
                guild.Lang.ConfigInvalidName
                |> sendMessage e.Channel |> ignore
        | _ :: _ ->
            addReaction e.Message app.Emojis.No
            guild.Lang.ConfigUnsetGuildNoData
            |> sendMessage e.Channel |> ignore
        | _ ->
            guild.Lang.ConfigUnsetMissingName guild.CommandPrefix
            |> sendMessage e.Channel |> ignore
    }

    let command =
        { BaseCommand with
            Names = [ "unset" ]
            Description = fun gc ->
                gc.Lang.ConfigUnsetDescription gc.CommandPrefix
                    gc.Lang.ConfigUnsetUsage gc.Lang.ConfigNames
            Permissions = GuildPrivileged
            MaxArgs = 1
            RateLimits = [
                { Allowed = 4uy
                  Interval = 30UL } ]
            Function = main }

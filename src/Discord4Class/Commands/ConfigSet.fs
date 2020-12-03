namespace Discord4Class.Commands

open System
open DSharpPlus
open DSharpPlus.EventArgs
open Discord4Class.Helpers.Messages
open Discord4Class.Repositories.GuildData
open Discord4Class.Config.Types
open Discord4Class.CommandsManager

module ConfigSet =

    let main app (guild: GuildConfig) client args memb (e: MessageCreateEventArgs) = async {
        match args with
        | configName :: value :: _ ->
            match (Map.tryFind configName GD.configUpdaters, guild.ExtractConfig configName) with
            | (Some update, Some v) ->
                addReaction e.Message app.Emojis.Doing
                match v with
                | TextChannelId _ ->
                    if e.MentionedChannels.Count = 0 then None
                    else Some e.MentionedChannels.[0].Id
                | VoiceChannelId _ ->
                    e.Guild.Channels
                    |> Seq.tryFind (fun ch ->
                        ch.Value.Type = ChannelType.Voice
                        && ch.Value.Name = value )
                    |> Option.map (fun ch -> ch.Value.Id)
                | RoleId _ ->
                    if e.MentionedRoles.Count = 0 then None
                    else Some e.MentionedRoles.[0].Id
                |> function
                    | Some id ->
                        GD.InsertOrUpdate app.Db guild.IsConfigOnDb
                            (GD.configInserts.[configName] e.Guild.Id (Some id))
                            (update <| Nullable<_> id)
                        |> Async.RunSynchronously
                        changeReaction e.Message app.Emojis.Yes
                    | None ->
                        changeReaction e.Message app.Emojis.No
                        guild.Lang.ConfigSetInvalidValue configName
                        |> sendMessage e.Channel |> ignore
            | _ ->
                addReaction e.Message app.Emojis.No
                guild.Lang.ConfigInvalidName
                |> sendMessage e.Channel |> ignore
        | [_] ->
            guild.Lang.ConfigSetMissingNewValue guild.CommandPrefix
            |> sendMessage e.Channel |> ignore
        | _ ->
            guild.Lang.ConfigSetMissingName guild.CommandPrefix
            |> sendMessage e.Channel |> ignore
    }

    let command =
        { BaseCommand with
            Names = [ "set" ]
            Description = fun gc ->
                gc.Lang.ConfigSetDescription gc.CommandPrefix
                    gc.Lang.ConfigSetUsage gc.Lang.ConfigNames
            Permissions = GuildPrivileged
            MaxArgs = 2
            RateLimits = [
                { Allowed = 4uy
                  Interval = 10UL }
                { Allowed = 30uy
                  Interval = 600UL } ]
            Function = main }

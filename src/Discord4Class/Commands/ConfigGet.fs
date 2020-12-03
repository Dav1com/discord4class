namespace Discord4Class.Commands

open DSharpPlus.EventArgs
open Discord4Class.Helpers.Messages
open Discord4Class.Config.Types
open Discord4Class.Repositories.GuildData
open Discord4Class.CommandsManager

module ConfigGet =

    let main app (guild: GuildConfig) client args memb (e: MessageCreateEventArgs) = async {
        match args with
        | configName :: _ ->
            match guild.ExtractConfig configName with
            | Some (TextChannelId None) | Some (VoiceChannelId None) | Some (RoleId None) ->
                guild.Lang.ConfigGetValueNull configName
                |> sendMessage e.Channel |> ignore
                addReaction e.Message app.Emojis.No
            | Some (TextChannelId (Some id)) | Some (VoiceChannelId (Some id)) ->
                if e.Guild.Channels.ContainsKey id then
                    e.Guild.Channels.[id].Mention
                else guild.Lang.DeletedChannel
                |> guild.Lang.ConfigGetValue
                |> sendMessage e.Channel |> ignore
            | Some (RoleId (Some id)) ->
                if e.Guild.Roles.ContainsKey id then
                    e.Guild.Roles.[id].Mention
                else guild.Lang.DeletedRole
                |> guild.Lang.ConfigGetValue
                |> sendMessage e.Channel |> ignore
            | None ->
                guild.Lang.ConfigInvalidName
                |> sendMessage e.Channel |> ignore
                addReaction e.Message app.Emojis.No
        | _ ->
            guild.Lang.ConfigGetMissingName guild.CommandPrefix
            |> sendMessage e.Channel |> ignore
    }

    let command =
        { BaseCommand with
            Names = [ "get" ]
            Description = fun gc ->
                gc.Lang.ConfigGetDescription gc.CommandPrefix
                    gc.Lang.ConfigGetUsage gc.Lang.ConfigNames
            Permissions = GuildPrivileged
            MaxArgs = 1
            RateLimits = [
                { Allowed = 4uy
                  Interval = 30UL } ]
            Function = main }

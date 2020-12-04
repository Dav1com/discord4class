namespace Discord4Class.Commands

open DSharpPlus
open DSharpPlus.EventArgs
open Discord4Class.Constants
open Discord4Class.Helpers.Permission
open Discord4Class.Helpers.Messages
open Discord4Class.Repositories.GuildData
open Discord4Class.Config.Types
open Discord4Class.CommandsManager

module Prefix =

    let main app guild (client: DiscordClient) args memb (e: MessageCreateEventArgs) = async {
        match args with
        | (newPrefix: string) :: _ when checkPermissions memb e.Channel GuildPrivilegedPerm ->
            match newPrefix with
            | s when s = guild.CommandPrefix -> guild.Lang.PrefixNoChange
            | s when s.Length > PrefixMaxSize ->
                guild.Lang.PrefixTooLong PrefixMaxSize
            | s ->
                GD.Update.Set((fun gd -> gd.CommandPrefix), s)
                |> GD.InsertOrUpdate app.Db guild.IsConfigOnDb
                    { GD.Base with
                        Id = e.Guild.Id
                        CommandPrefix = Some s }
                |> Async.RunSynchronously

                guild.Lang.PrefixSuccess newPrefix
        | _ :: _ ->
            guild.Lang.PrefixNoPermission guild.Lang.ManageGuild
        | [] ->
            "  1) `" + guild.CommandPrefix + "`" +
            if app.CommandByMention then "\n  2) " + client.CurrentUser.Mention
            else ""
            |> guild.Lang.PrefixActualValue
        |> sendMessage e.Channel |> ignore
    }

    let command =
        { BaseCommand with
            Names = [ "prefix" ]
            Description = fun gc ->
                gc.Lang.PrefixDescription gc.CommandPrefix gc.Lang.PrefixUsage
            Permissions = NoPerms
            MaxArgs = 1
            RateLimits = [
                { Allowed = 2uy
                  Interval = 10UL } ]
            Function = main }

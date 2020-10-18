namespace Discord4Class.Commands

open DSharpPlus
open DSharpPlus.EventArgs
open Discord4Class.Helpers.Messages
open Discord4Class.Helpers.Permission
open Discord4Class.Repositories.GuildConfiguration
open Discord4Class.Config.Types

module Prefix =

    [<Literal>]
    let RequiredPerms = Permissions.Administrator
    [<Literal>]
    let PrefixMaxSize = 2

    let exec app guild _ newPrefix (e : MessageCreateEventArgs) = async {
        if checkPermissions e RequiredPerms then
            newPrefix
            |> function
                | s when s = guild.CommandPrefix ->
                    guild.Lang.PrefixNoChange
                | s when s = "" ->
                    guild.Lang.PrefixMissingArg guild.CommandPrefix
                | s when s.Length > PrefixMaxSize ->
                    guild.Lang.PrefixTooLong PrefixMaxSize
                | s ->
                    { GC.Base with
                        _id = e.Guild.Id
                        CommandPrefix = Some s }
                    |> GC.InsertUpdate app.Db guild.IsConfigOnDb
                    |> Async.RunSynchronously

                    guild.Lang.PrefixSuccess newPrefix
            |> sendMessage e.Channel |> ignore
    }

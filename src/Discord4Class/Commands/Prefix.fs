namespace Discord4Class.Commands

open System.Threading.Tasks
open DSharpPlus
open DSharpPlus.EventArgs
open Discord4Class.Helpers.Permission
open Discord4Class.Config.Types
open Discord4Class.Repositories.GuildConfiguration

module Prefix =

    [<Literal>]
    let RequiredPerms = Permissions.Administrator
    [<Literal>]
    let PrefixMaxSize = 2

    let exec (config : Config) newPrefix (e : MessageCreateEventArgs) =
        async {
            if checkPermissions e RequiredPerms then
                newPrefix
                |> function
                    | s when s = config.Guild.CommandPrefix ->
                        config.Guild.Lang.PrefixNoChange
                        |> fun s -> e.Channel.SendMessageAsync(s)
                        |> Async.AwaitTask |> Async.Ignore
                    | s when s = "" ->
                        config.Guild.Lang.PrefixMissingArg config.Guild.CommandPrefix
                        |> fun s -> e.Channel.SendMessageAsync(s)
                        |> Async.AwaitTask |> Async.Ignore
                    | s when s.Length > PrefixMaxSize ->
                        config.Guild.Lang.PrefixTooLong PrefixMaxSize
                        |> fun s -> e.Channel.SendMessageAsync(s)
                        |> Async.AwaitTask |> Async.Ignore
                    | s ->
                        { GC.Base with
                            _id = e.Guild.Id
                            CommandPrefix = Some s }
                        |> GC.InsertUpdate config.App.DbDatabase config.Guild.IsConfigOnDb
                        |> Async.RunSynchronously

                        config.Guild.Lang.PrefixSuccess newPrefix
                        |> fun s -> e.Channel.SendMessageAsync(s)
                        |> Async.AwaitTask |> Async.Ignore
                |> Async.RunSynchronously
        } |> Async.StartAsTask :> Task

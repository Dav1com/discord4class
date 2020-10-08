namespace Discord4Class.Commands

open System.Threading.Tasks
open DSharpPlus.EventArgs
open Discord4Class.Config.Types
open Discord4Class.Repositories.GuildConfiguration

module Prefix =

    [<Literal>]
    let PrefixMaxSize = 2

    let exec (config : Config) newPrefix (e : MessageCreateEventArgs) =
        async {

            newPrefix
            |> function
                | s when s = config.Guild.CommandPrefix ->
                    config.Guild.Lang.PrefixNoChange
                    |> fun s -> e.Channel.SendMessageAsync(s)
                    |> Async.AwaitTask |> Async.Ignore
                | s when s.ToLower().Contains "prefix" ->
                    config.Guild.Lang.PrefixMissingArg config.Guild.CommandPrefix PrefixMaxSize
                    |> fun s -> e.Channel.SendMessageAsync(s)
                    |> Async.AwaitTask |> Async.Ignore
                | s when s.Length > PrefixMaxSize ->
                    config.Guild.Lang.PrefixTooLong PrefixMaxSize
                    |> fun s -> e.Channel.SendMessageAsync(s)
                    |> Async.AwaitTask |> Async.Ignore
                | s ->
                    if config.Guild.IsConfigOnDb then
                        let filter = GC.Filter.And [
                            GC.Filter.Eq((fun g -> g._id), e.Guild.Id)
                        ]
                        let update = GC.Update.Set((fun g -> g.CommandPrefix), s)
                        [
                            GC.UpdateOne config.App.DbDatabase filter update
                            |> Async.Ignore
                        ]
                    else
                        [
                            GC.Insert config.App.DbDatabase {
                                _id = e.Guild.Id
                                CommandPrefix = newPrefix
                                Language = config.Bot.DefaultLang
                                Channels = None
                            }
                        ]
                    |> List.append [
                        config.Guild.Lang.PrefixSuccess newPrefix
                        |> fun s -> e.Channel.SendMessageAsync(s)
                        |> Async.AwaitTask
                        |> Async.Ignore
                    ]
                    |> Async.Parallel
                    |> Async.Ignore
            |> Async.RunSynchronously
        } |> Async.StartAsTask :> Task

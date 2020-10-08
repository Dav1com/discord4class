namespace Discord4Class.Commands

open System.Globalization
open System.Threading.Tasks
open DSharpPlus.EventArgs
open Discord4Class.Repositories.GuildConfiguration
open Discord4Class.Config.Types

module Lang =

    let exec config (newLang : string) (e : MessageCreateEventArgs) =
        async {
            newLang.ToLower()
            |> config.Lang.TryFind
            |> function
                | Some l ->
                    if config.Guild.IsConfigOnDb then
                        let filter =
                            GC.Filter.And [
                                GC.Filter.Eq((fun x -> x._id), e.Guild.Id)
                            ]
                        let update = GC.Update.Set((fun x -> x.Language), newLang)
                        [
                            GC.UpdateOne config.App.DbDatabase filter update
                            |> Async.Ignore
                        ]
                    else
                        [
                            GC.Insert config.App.DbDatabase {
                                _id = e.Guild.Id
                                CommandPrefix = config.Bot.CommandPrefix
                                Language = newLang
                                Channels = None }
                            |> Async.Ignore
                        ]
                    |> List.append
                        [
                            l.LangSuccess (CultureInfo newLang).NativeName
                            |> fun s -> e.Channel.SendMessageAsync(s)
                            |> Async.AwaitTask
                            |> Async.Ignore
                        ]
                    |> Async.Parallel
                    |> Async.Ignore
                | None ->
                    if newLang.Contains "lang" then
                        config.Guild.Lang.LangMissingArg config.Guild.CommandPrefix config.App.DocsURL
                        |> fun s -> e.Channel.SendMessageAsync(s)
                        |> Async.AwaitTask |> Async.Ignore
                    else
                        config.Guild.Lang.LangNotFound config.App.DocsURL
                        |> fun s -> e.Channel.SendMessageAsync(s)
                        |> Async.AwaitTask |> Async.Ignore
            |> Async.RunSynchronously
        } |> Async.StartAsTask :> Task

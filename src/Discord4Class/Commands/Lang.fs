namespace Discord4Class.Commands

open System.Threading.Tasks
open DSharpPlus.EventArgs
open Discord4Class.Repositories.GuildConfiguration
open Discord4Class.Config.Types

module Lang =

    let exec config (e : MessageCreateEventArgs) =
        let newLang =
            (e.Message.Content.Split " "
            |> Array.last)
                .ToLower()
        newLang
        |> config.Lang.TryFind
        |> function
            | Some l ->
                if not config.Guild.IsConfigOnDb then
                    [
                        GC.Insert config.App.DbDatabase {
                            _id = e.Guild.Id
                            CommandPrefix = config.Bot.CommandPrefix
                            Language = newLang }
                        |> Async.Ignore
                    ]
                else
                    let filter =
                        GC.Filter.And [
                            GC.Filter.Eq((fun x -> x._id), e.Guild.Id)
                        ]
                    let update = GC.Update.Set((fun x -> x.Language), newLang)
                    [
                        GC.UpdateOne config.App.DbDatabase filter update
                        |> Async.Ignore
                    ]
                |> List.append
                    [
                        l.LangSuccess
                        |> fun s -> e.Channel.SendMessageAsync(s, false)
                        |> Async.AwaitTask
                        |> Async.Ignore
                    ]
                |> Async.Parallel
                |> Async.StartAsTask
                :> Task
            | None ->
                if newLang = "lang" || newLang = config.Guild.CommandPrefix + "lang" then
                    config.Guild.Lang.LangMissingArg config.Guild.CommandPrefix config.App.DocsURL
                    |> fun s -> e.Channel.SendMessageAsync(s, false)
                else
                    config.Guild.Lang.LangNotFound config.App.DocsURL
                    |> fun s -> e.Channel.SendMessageAsync(s, false)
                :> Task

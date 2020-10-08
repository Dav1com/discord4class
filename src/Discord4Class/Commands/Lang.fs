namespace Discord4Class.Commands

open System.Globalization
open System.Threading.Tasks
open DSharpPlus
open DSharpPlus.EventArgs
open Discord4Class.Helpers.Permission
open Discord4Class.Repositories.GuildConfiguration
open Discord4Class.Config.Types

module Lang =

    [<Literal>]
    let RequiredPerms = Permissions.Administrator

    let exec config _ (newLang : string) (e : MessageCreateEventArgs) = async {
        if checkPermissions e RequiredPerms then
            newLang.ToLower()
            |> config.Lang.TryFind
            |> function
                | Some l ->
                    { GC.Base with
                        _id = e.Guild.Id
                        Language = Some newLang }
                    |> GC.InsertUpdate config.App.DbDatabase config.Guild.IsConfigOnDb
                    |> Async.RunSynchronously

                    l.LangSuccess (CultureInfo newLang).NativeName
                    |> fun s -> e.Channel.SendMessageAsync(s)
                    |> Async.AwaitTask |> Async.Ignore
                | None ->
                    if newLang = "" then
                        config.Guild.Lang.LangMissingArg config.Guild.CommandPrefix
                        |> fun s -> e.Channel.SendMessageAsync(s)
                        |> Async.AwaitTask |> Async.Ignore
                    else
                        config.Guild.Lang.LangNotFound
                        |> fun s -> e.Channel.SendMessageAsync(s)
                        |> Async.AwaitTask |> Async.Ignore
            |> Async.RunSynchronously
    }

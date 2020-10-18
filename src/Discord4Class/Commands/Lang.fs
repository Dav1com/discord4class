namespace Discord4Class.Commands

open System.Globalization
open DSharpPlus
open DSharpPlus.EventArgs
open Discord4Class.Helpers.Messages
open Discord4Class.Helpers.Permission
open Discord4Class.Repositories.GuildConfiguration
open Discord4Class.Config.Types

module Lang =

    [<Literal>]
    let RequiredPerms = Permissions.Administrator

    let exec app guild _ (newLang : string) (e : MessageCreateEventArgs) = async {
        if checkPermissions e RequiredPerms then
            newLang.ToLower()
            |> app.AllLangs.TryFind
            |> function
                | Some l ->
                    { GC.Base with
                        _id = e.Guild.Id
                        Language = Some newLang }
                    |> GC.InsertUpdate app.Db guild.IsConfigOnDb
                    |> Async.RunSynchronously

                    l.LangSuccess (CultureInfo newLang).NativeName
                | None ->
                    if newLang = "" then
                        guild.Lang.LangMissingArg guild.CommandPrefix
                    else
                        guild.Lang.LangNotFound
            |> sendMessage e.Channel |> ignore
    }

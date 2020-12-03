namespace Discord4Class.Commands

open System.Globalization
open DSharpPlus.EventArgs
open Discord4Class.Helpers.Messages
open Discord4Class.Repositories.GuildData
open Discord4Class.Config.Types
open Discord4Class.CommandsManager

module Lang =

    let main app guild _ (args: string list) memb (e: MessageCreateEventArgs) = async {
        match args with
        | newLang :: _ ->
            newLang.ToLower()
            |> app.AllLangs.TryFind
            |> function
                | Some l ->
                    GD.Update.Set((fun gd -> gd.Language), newLang)
                    |> GD.InsertOrUpdate app.Db guild.IsConfigOnDb
                        { GD.Base with
                            Id = e.Guild.Id
                            Language = Some newLang }
                    |> Async.RunSynchronously

                    l.LangSuccess (CultureInfo newLang).NativeName
                | None -> guild.Lang.LangNotFound
        | _ -> guild.Lang.LangActualValue (CultureInfo guild.LangName).NativeName
        |> sendMessage e.Channel |> ignore
    }

    let command =
        { BaseCommand with
            Names = [ "lang" ]
            Description = fun gc ->
                gc.Lang.LangDescription gc.CommandPrefix gc.Lang.LangUsage
            Permissions = GuildPrivileged
            MaxArgs = 1
            RateLimits = [
                { Allowed = 3uy
                  Interval = 10UL } ]
            Function = main }

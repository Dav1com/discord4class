namespace Discord4Class.Config

open System
open System.Text.RegularExpressions
open Microsoft.Extensions.Configuration
open FSharp.Reflection
open FsConfig
open DSharpPlus
open DSharpPlus.Entities
open Discord4Class.Helpers.String
open Discord4Class.Helpers.Railway
open Discord4Class.Constants
open Discord4Class.Db
open Discord4Class.Repositories.GuildData
open Discord4Class.Lang.Loader
open Discord4Class.Config.InnerTypes
open Discord4Class.Config.Types

module Loader =

    type ConfigurationError =
        | ConfigMissing of fieldName: string
        | InvalidValue of fieldName: string * value: string
        | InvalidEmoji of fieldName: string * value: string
        | EmojiNotFound of fieldName: string  * value: string
        | GuildEmojiNotFound of fieldName: string * value: string
        | PrefixTooLong of maxLength: int
        | PrefixTooShort of minLength: int
        | NotSupportedType of fieldName: string

        member this.Fail () =
            match this with
            | ConfigMissing fn -> failwithf "Configuration %s not found" fn
            | InvalidValue (fn, value) ->
                failwithf "Configuration %s has invalid value: '%s'" fn value
            | InvalidEmoji (fn, value) ->
                failwithf "The value '%s' is not a proper value for %s, \
                           expeting an emoji" value fn
            | EmojiNotFound (fn, value) ->
                failwithf "In field %s, the emoji '%s' doesn't exists" fn value
            | GuildEmojiNotFound (fn, value) ->
                failwithf "The emoji %s in the field %s was not found in \
                           the provided guild" value fn
            | PrefixTooShort min ->
                failwithf "The provided command prefix is too short"
            | PrefixTooLong max ->
                failwithf "The provided command prefix is too long, \
                           max length is %i" max
            | NotSupportedType msg -> failwith msg

    let private checkEmojis emojis =
        let eFields = FSharpValue.GetRecordFields(emojis)
        eFields
        |> Array.tryFindIndex (fun o ->
            let str = (o :?> string)
            not ((str.Length > 2 && isSnakeCase str.[1..str.Length - 2])
                 || (isNumeric str && str.Length <= 20
                    && (sprintf "%20s" str) <= (string UInt64.MaxValue)) ) )
        |> function
            | Some i ->
                InvalidEmoji
                    ( FSharpType.GetRecordFields(typeof<Emojis>).[i].Name,
                      eFields.[i] :?> string )
                |> Error
            | None -> Ok ()

    let private checkPrefix (prefix: string) =
        if prefix.Length <= PrefixMaxSize then Ok ()
        elif prefix.Length = 0 then Error (PrefixTooShort 1)
        else Error (PrefixTooLong PrefixMaxSize)

    let private checkLang (lang: string) =
        if (Regex "([a-zA-Z]{2}-[a-zA-Z]{2})").Match(lang).Success
            && Globalization.CultureInfo(lang).LCID <> 4096
        then Ok ()
        else Error (InvalidValue ("DefaultLang", lang))

    let private extractConfig = function
        | Ok (c: IniConfig) ->
            checkEmojis c.Emojis
            >>= fun _ -> checkPrefix c.Bot.CommandPrefix
            >>= fun _ -> checkLang c.Bot.DefaultLang
            >>= fun _ -> Ok c
        | Error (NotFound envVarName) -> Error (ConfigMissing envVarName)
        | Error (BadValue (envVarName, value)) ->
            Error (InvalidValue (envVarName, value))
        | Error (NotSupported msg) -> Error (NotSupportedType msg)

    let loadConfiguration (inifile: string) =
        let c =
            ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddIniFile(inifile)
                .Build()

        let ac = AppConfig c
        ac.Get<IniConfig>()
        |> extractConfig

    let private parseEmojis (client: DiscordClient) emojiGuildId iniEmojis =
        let guildEmojis =
            match emojiGuildId with
            | Some i ->
                client.GetGuildAsync i
                |> Async.AwaitTask |> Async.RunSynchronously
                |> function
                    | null -> None
                    | guild ->
                        guild.Emojis
                        |> Seq.map (|KeyValue|)
                        |> Map.ofSeq
                        |> Some
            | None -> None
        FSharpValue.GetRecordFields iniEmojis
        |> Array.map (fun s ->
            match s :?> string with
            | s when s.[0] = ':' -> DiscordEmoji.FromName(client, s)
            | s ->
                match guildEmojis with
                | Some emojis ->
                    Map.tryFind (uint64 s) emojis
                    |> function
                        | Some emoji -> emoji
                        | None ->
                            failwithf "Guild %u doen't have the emoji of id %u"
                                emojiGuildId.Value (uint64 s)
                | None ->
                    failwith "When using custom emoji, you must provide \
                              the guild id where those emojis are, and the \
                              bot must be member of that guild"
            :> obj )
        |> fun arr -> FSharpValue.MakeRecord(typeof<Emojis>, arr) :?> Emojis

    let buildFullConfig (client: DiscordClient) (iniConfig: InnerTypes.IniConfig) =
        { Bot = iniConfig.Bot
          Persistence = iniConfig.Persistence
          Preferences = iniConfig.Preferences
          App =
            { DocsURL = iniConfig.Misc.DocsUrl
              JoinGuildURL = iniConfig.Misc.InviteToGuildUrl
              SupportServer = iniConfig.Misc.SupportGuild
              Emojis = parseEmojis client iniConfig.Misc.CustomEmojiGuild iniConfig.Emojis
              AllLangs = loadLangFiles LangPath
              Db = openConnection iniConfig.Persistence.DbUri
              CommandByMention = iniConfig.Bot.CommandByMention } }

    let loadGuildConfiguration c db guildId =
        let gc = GD.Operation.FindById db guildId |> Async.RunSynchronously
        match gc with
        | Some g ->
            let (lang, langName) =
                match g.Language with
                | Some l ->
                    c.App.AllLangs
                    |> Map.tryFind l
                    |> function
                        | Some lan -> (lan, l)
                        | None ->
                            ( c.App.AllLangs.[c.Bot.DefaultLang],
                              c.Bot.DefaultLang )
                | None ->
                    (c.App.AllLangs.[c.Bot.DefaultLang], c.Bot.DefaultLang)
            let prefix =
                match g.CommandPrefix with
                | Some p -> p
                | None -> c.Bot.CommandPrefix
            { Lang = lang
              LangName = langName
              CommandPrefix = prefix
              IsConfigOnDb = true
              TeachersText = IdValue.ToUint64 g.TeachersText
              ClassVoice = IdValue.ToUint64 g.ClassVoice
              TeacherRole = IdValue.ToUint64 g.TeacherRole
              DoingHeavyTask = g.DoingHeavyTask }
        | None ->
            { Lang = c.App.AllLangs.[c.Bot.DefaultLang]
              LangName = c.Bot.DefaultLang
              CommandPrefix = c.Bot.CommandPrefix
              IsConfigOnDb = false
              TeachersText = None
              ClassVoice = None
              TeacherRole = None
              DoingHeavyTask = false }

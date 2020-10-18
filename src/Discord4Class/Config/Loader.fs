namespace Discord4Class.Config

open System.IO
open Microsoft.Extensions.Configuration
open FsConfig
open Discord4Class.Db
open Discord4Class.Repositories.GuildConfiguration
open Discord4Class.Lang.Loader
open Discord4Class.Config.InnerTypes
open Discord4Class.Config.Types

module Loader =

    let extractConfigOrFail (config : Result<IniConfig,_>) =
        match config with
        | Ok c ->
            let langPath = "res/lang/"
            let fullLang = loadLangFiles langPath
            {
                Bot = c.Bot
                Persistence = c.Persistence
                Preferences = c.Preferences
                App = {
                    DocsURL = c.Misc.DocsUrl
                    JoinGuildURL = c.Misc.DocsUrl
                    AllLangs = fullLang
                    Db = openConnection c.Persistence.DbUri
                    Emojis = c.Emojis
                }
            }
        | Error err ->
            match err with
            | NotFound envVarName ->
                failwithf "Configuration %s not found" envVarName
            | BadValue (envVarName, value) ->
                failwithf "Configuration %s has invalid value: '%s'" envVarName value
            | NotSupported msg ->
                failwith msg

    let loadConfiguration (inifile : string) =
        let c =
            ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddIniFile(inifile)
                .Build()

        let ac = AppConfig c
        ac.Get<IniConfig> ()
        |> extractConfigOrFail

    let loadGuildConfiguration (c : Config) db (gui : uint64) =
        let gc =
            GC.Filter.And [
                GC.Filter.Eq((fun x -> x._id), gui)
            ]
            |> GC.GetOne db
            |> Async.RunSynchronously
        match gc with
        | Some g ->
            let lang =
                match g.Language with
                | Some l -> c.App.AllLangs.[l]
                | None -> c.App.AllLangs.[c.Bot.DefaultLang]
            let prefix =
                match g.CommandPrefix with
                | Some p -> p
                | None -> c.Bot.CommandPrefix
            {
                Lang = lang
                CommandPrefix = prefix
                IsConfigOnDb = true
                TeachersText = g.TeachersText
                ClassVoice = g.ClassVoice
                TeacherRole = g.TeacherRole
            }
        | None ->
            {
                Lang = c.App.AllLangs.[c.Bot.DefaultLang]
                CommandPrefix = c.Bot.CommandPrefix
                IsConfigOnDb = false
                TeachersText = None
                ClassVoice = None
                TeacherRole = None
            }

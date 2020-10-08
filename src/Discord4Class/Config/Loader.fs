namespace Discord4Class.Config

open System.IO
open System.Reflection
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
            let assembly = Assembly.GetExecutingAssembly().GetName()
            let langPath = "res/lang/"
            let fullLang = loadLangFiles langPath
            let defLang = c.Bot.DefaultLang
            {
                Bot = c.Bot
                Persistence = c.Persistence
                Preferences = c.Preferences
                App = {
                    AppName = assembly.Name
                    AppVersion = assembly.Version.ToString()
                    DocsURL = "<PlaceHolder>"
                    JoinGuildURL = "<PlaceHolder>"
                    DbDatabase = openConnection c.Persistence.DbUri
                }
                Lang = fullLang
                Guild = {
                    Lang = fullLang.[defLang]
                    CommandPrefix = c.Bot.CommandPrefix
                    IsConfigOnDb = false
                    TeachersText = None
                    ClassVoice = None
                    TeacherRole = None
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
                | Some l -> c.Lang.[l]
                | None -> c.Lang.[c.Bot.DefaultLang]
            let prefix =
                match g.CommandPrefix with
                | Some p -> p
                | None -> c.Bot.CommandPrefix
            { c with
                Guild = {
                    Lang = lang
                    CommandPrefix = prefix
                    IsConfigOnDb = true
                    TeachersText = g.TeachersText
                    ClassVoice = g.ClassVoice
                    TeacherRole = g.TeacherRole
                }
            }
        | None ->
            { c with
                Guild = {
                    Lang = c.Lang.[c.Bot.DefaultLang]
                    CommandPrefix = c.Bot.CommandPrefix
                    IsConfigOnDb = false
                    TeachersText = None
                    ClassVoice = None
                    TeacherRole = None
                }
            }

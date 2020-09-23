namespace Discord4Class.Config

open System.IO
open System.Reflection
open Microsoft.Extensions.Configuration
open FsConfig
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
                    LangFilesPath = langPath
                }
                Lang = fullLang
                Guild = {
                    LangLocale = defLang
                    Lang = fullLang.[defLang]
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

namespace Discord4Class.Config

open FsConfig
open MongoDB.Driver
open Discord4Class.Lang.Types

// this module should not be directly opened,
// except inside of Discord4Class.Config namespace
module InnerTypes =
    type BotMode =
        | Public
        | Private

    type BotConf =
      { BotToken : string
        [<DefaultValue("Discord4Class")>] BotName : string
        [<DefaultValue("Public")>] Mode : BotMode
        [<DefaultValue("true")>]   Multilangual : bool
        [<DefaultValue("en-us")>]  DefaultLang : string
        [<DefaultValue("!")>]      CommandPrefix : string
        [<DefaultValue("true")>]   CommandByMention : bool
        [<DefaultValue("Admin")>]  AdminRole : string }

    type DbDriverConfig =
        | MongoDB
//TODO: | SQLite

    type Persistence =
      { DbDriver : DbDriverConfig
        DbUrl : string
        [<DefaultValue("")>] DbUser : string
        [<DefaultValue("")>] DbPass : string}

    type LogLevelConf =
        | Off
        | Normal
        | Debug

    type Preferences =
      { AutoStartBot : bool
        LogLevel : LogLevelConf }

    type IniConfig =
      { Bot : BotConf
        Persistence : Persistence
        Preferences : Preferences }

    type AppConfig =
      { AppName : string // located in .fsproj
        AppVersion : string // Located in .fsproj
        DocsURL : string // TODO: Move to app.config
        JoinGuildURL : string // TODO: Move to app.config
        DbDatabase : IMongoDatabase }

    type GuildConfig =
      { LangLocale : string
        Lang : LangBuilders }

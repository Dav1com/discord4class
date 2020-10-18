namespace Discord4Class.Config

open MongoDB.Driver
open Discord4Class.Lang.Types
open Discord4Class.Config.InnerTypes

module Types =

    type AppConfig =
      { DocsURL : string
        JoinGuildURL : string
        Emojis : Emoji
        AllLangs : Map<string, LangBuilders>
        Db : IMongoDatabase }

    type GuildConfig =
      { Lang : LangBuilders
        CommandPrefix : string
        IsConfigOnDb : bool
        TeachersText : uint64 option
        ClassVoice : uint64 option
        TeacherRole : uint64 option }

    type Config =
      { Bot : BotConf
        Persistence : Persistence
        Preferences : Preferences
        App : AppConfig }

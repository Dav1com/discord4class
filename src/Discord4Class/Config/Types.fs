namespace Discord4Class.Config

open DSharpPlus.Entities
open MongoDB.Driver
open Discord4Class.Lang.Types
open Discord4Class.Config.InnerTypes

module Types =

    type AppConfig =
        { DocsURL: string
          JoinGuildURL: string
          SupportServer: string
          Emojis: Emojis
          AllLangs: Map<string, LangBuilders>
          Db: IMongoDatabase
          CommandByMention: bool }

    type GuildConfig =
        { Lang: LangBuilders
          LangName: string
          CommandPrefix: string
          IsConfigOnDb: bool
          TeachersText: uint64 option
          ClassVoice: uint64 option
          TeacherRole: uint64 option
          DoingHeavyTask: bool }

    type Config =
        { Bot: BotConf
          Persistence: Persistence
          Preferences: Preferences
          App: AppConfig }

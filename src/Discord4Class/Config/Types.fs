namespace Discord4Class.Config

open Discord4Class.Lang.Types
open Discord4Class.Config.InnerTypes

module Types =
    type Config =
          { Bot : BotConf
            Persistence : Persistence
            Preferences : Preferences
            App : AppConfig
            Lang : Map<string, LangBuilders>
            Guild : GuildConfig }

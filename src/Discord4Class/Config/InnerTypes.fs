namespace Discord4Class.Config

open FsConfig

module InnerTypes =
    type BotMode =
        | Public
        | Private

    type BotConf =
      { BotToken : string
        [<DefaultValue("en-us")>]  DefaultLang : string
        [<DefaultValue("!")>]      CommandPrefix : string
        [<DefaultValue("true")>]   CommandByMention : bool }

    type Persistence =
      { DbUri : string }

    type LogLevelConf =
        | Off
        | Normal
        | Debug

    type Preferences =
      { LogLevel : LogLevelConf }

    type Emoji =
      { Doing : string
        Yes : string
        No : string
        Invalid : string
        Sended : string
        Mute : string
        UnMute : string }

    type Misc =
      { [<DefaultValue("")>] DocsUrl : string
        [<DefaultValue("")>] InviteToGuildUrl : string }

    type IniConfig =
      { Bot : BotConf
        Persistence : Persistence
        Preferences : Preferences
        Emojis : Emoji
        Misc : Misc }

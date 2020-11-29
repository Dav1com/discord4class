namespace Discord4Class.Config

open DSharpPlus.Entities
open FsConfig

module InnerTypes =

    type BotMode =
        | Public
        | Private

    type BotConf =
        { BotToken: string
          [<DefaultValue("en-us")>] DefaultLang: string
          [<DefaultValue("!")>]     CommandPrefix: string
          [<DefaultValue("true")>]  CommandByMention: bool }

    type Persistence = { DbUri: string }

    type LogLevelConf =
        | Off
        | Normal
        | Debug

    type Preferences = { LogLevel: LogLevelConf }

    type IniEmojis =
        { Doing: string
          Yes: string
          No: string
          Invalid: string
          Sended: string
          Mute: string
          UnMute: string }

    type Emojis =
        { Doing: DiscordEmoji
          Yes: DiscordEmoji
          No: DiscordEmoji
          Invalid: DiscordEmoji
          Sended: DiscordEmoji
          Mute: DiscordEmoji
          UnMute: DiscordEmoji }

    type Misc =
        { [<DefaultValue("")>] DocsUrl: string
          [<DefaultValue("")>] InviteToGuildUrl: string
          [<DefaultValue("")>] SupportGuild: string
          CustomEmojiGuild: uint64 option }

    type IniConfig =
        { Bot: BotConf
          Persistence: Persistence
          Preferences: Preferences
          Emojis: IniEmojis
          Misc: Misc }

namespace Discord4Class.Config

open DSharpPlus.Entities
open MongoDB.Driver
open Discord4Class.Lang.Types
open Discord4Class.Config.InnerTypes
open Discord4Class.Repositories.GuildData

module Types =

    type IdValue =
        | TextChannelId of uint64 option
        | VoiceChannelId of uint64 option
        | RoleId of uint64 option

        member this.Value =
            match this with
            | TextChannelId i -> i
            | VoiceChannelId i -> i
            | RoleId i -> i

        static member ToUint64 = Option.map (fun (v: IdValue) -> v.Value)

        static member GetValue (iv: IdValue) = iv.Value

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

        member this.ExtractConfig name =
            match name with
            | "teachers-text" ->
                Some (TextChannelId this.TeachersText)
            | "class-voice" ->
                Some (VoiceChannelId this.ClassVoice)
            | "teacher-role" ->
                Some (RoleId this.TeacherRole)
            | _ -> None

    type Config =
        { Bot: BotConf
          Persistence: Persistence
          Preferences: Preferences
          App: AppConfig }

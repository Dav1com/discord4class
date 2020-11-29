namespace Discord4Class.Repositories

open System
open MongoDB.Driver
open Discord4Class.Db

module GuildData =

    [<Literal>]
    let private collectionName = "GuildConfiguration"

    [<CLIMutable>]
    type GDDocument =
        { Id: uint64
          CommandPrefix: string
          Language: string
          TeachersText: Nullable<uint64>
          ClassVoice: Nullable<uint64>
          TeacherRole: Nullable<uint64>
          DoingHeavyTask: bool }

    type IdValue =
        | TextChannelId of uint64
        | VoiceChannelId of uint64
        | RoleId of uint64

        member this.Value =
            match this with
            | TextChannelId i -> i
            | VoiceChannelId i -> i
            | RoleId i -> i

        static member ToUint64 = Option.map (fun (v: IdValue) -> v.Value)

        static member GetValue (iv: IdValue) = iv.Value

    type GuildData =
        { Id: uint64 //GuildId
          CommandPrefix: string option
          Language: string option
          TeachersText: IdValue option
          ClassVoice: IdValue option
          TeacherRole: IdValue option
          //RateLimits: RateLimits
          DoingHeavyTask: bool } // unix timestamp

        static member private gdToDocument t =
            { Id = t.Id
              CommandPrefix = Option.defaultValue null t.CommandPrefix
              Language = Option.defaultValue null t.Language
              TeachersText =
                Option.map IdValue.GetValue t.TeachersText
                |> Option.toNullable
              ClassVoice =
                Option.map IdValue.GetValue t.ClassVoice
                |> Option.toNullable
              TeacherRole =
                Option.map IdValue.GetValue t.TeacherRole
                |> Option.toNullable
              DoingHeavyTask = t.DoingHeavyTask }: GDDocument

        static member private documentToGD (inner: GDDocument) =
            { Id = inner.Id
              CommandPrefix = Option.ofObj inner.CommandPrefix
              Language = Option.ofObj inner.Language
              TeachersText =
                Option.ofNullable (inner.TeachersText)
                |> Option.map TextChannelId
              ClassVoice =
                Option.ofNullable (inner.ClassVoice)
                |> Option.map VoiceChannelId
              TeacherRole =
                Option.ofNullable inner.TeacherRole
                |> Option.map RoleId
              DoingHeavyTask = inner.DoingHeavyTask }

        static member Filter = Builders<GDDocument>.Filter
        static member Update = Builders<GDDocument>.Update
        static member Operation =
            Operation(collectionName, GD.gdToDocument,
                GD.documentToGD, fun gd -> gd.Id)
        static member Base =
            { Id = 0UL
              CommandPrefix = None
              Language = None
              TeachersText = None
              ClassVoice = None
              TeacherRole = None
              DoingHeavyTask = false }

        static member InsertOrUpdate db isUpdate object update =
            if isUpdate then
                GD.Operation.UpdateOneById db object.Id update
                |> Async.Ignore
            else
                GD.Operation.InsertOne db object

        static member configUpdaters =
            [ ("teachers-text",
                fun v -> GD.Update.Set((fun gd -> gd.TeachersText), v))
              ("class-voice",
                fun v -> GD.Update.Set((fun gd -> gd.ClassVoice), v))
              ("teacher-role",
                fun v -> GD.Update.Set((fun gd -> gd.TeacherRole), v)) ]
            |> Map.ofList

        static member configExtractors =
            [ ("teachers-text", fun gd -> gd.TeachersText)
              ("class-voice", fun gd -> gd.ClassVoice)
              ("teacher-role", fun gd -> gd.TeacherRole) ]
            |> Map.ofList

        static member configIdTypes =
            [ ("teachers-text", TextChannelId 0UL)
              ("class-voice", VoiceChannelId 0UL)
              ("teacher-role", RoleId 0UL) ]
            |> Map.ofList

        static member configInserts =
            [ ("teachers-text", fun id v -> { GD.Base with Id = id; TeachersText = v })
              ("class-voice", fun id v -> { GD.Base with Id = id; ClassVoice = v })
              ("teacher-role", fun id v -> { GD.Base with Id = id; TeacherRole = v }) ]
            |> Map.ofList

    and GD = GuildData


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

    type GuildData =
        { Id: uint64 //GuildId
          CommandPrefix: string option
          Language: string option
          TeachersText: uint64 option
          ClassVoice: uint64 option
          TeacherRole: uint64 option
          //RateLimits: RateLimits
          DoingHeavyTask: bool } // unix timestamp

        static member private gdToDocument t =
            { Id = t.Id
              CommandPrefix = Option.defaultValue null t.CommandPrefix
              Language = Option.defaultValue null t.Language
              TeachersText = Option.toNullable t.TeachersText
              ClassVoice = Option.toNullable t.ClassVoice
              TeacherRole = Option.toNullable t.TeacherRole
              DoingHeavyTask = t.DoingHeavyTask }: GDDocument

        static member private documentToGD (inner: GDDocument) =
            { Id = inner.Id
              CommandPrefix = Option.ofObj inner.CommandPrefix
              Language = Option.ofObj inner.Language
              TeachersText = Option.ofNullable inner.TeachersText
              ClassVoice = Option.ofNullable inner.ClassVoice
              TeacherRole = Option.ofNullable inner.TeacherRole
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

        static member configInserts =
            [ ("teachers-text", fun id v -> { GD.Base with Id = id; TeachersText = v })
              ("class-voice", fun id v -> { GD.Base with Id = id; ClassVoice = v })
              ("teacher-role", fun id v -> { GD.Base with Id = id; TeacherRole = v }) ]
            |> Map.ofList

    and GD = GuildData


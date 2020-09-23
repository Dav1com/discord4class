namespace Discord4Class.Repositories

open System
open MongoDB.Driver
open MongoDB.Bson

module GuildConfiguration =

    [<Literal>]
    let private CollectionName = "GuildConfiguration"

    type GuildConfiguration =
      { GuildId : uint64
        CommandPrefix : String option
        Language : String option
        ClassTextChannel : uint64
        ClassVoiceChannel : uint64
        TeachersTextChannel : uint64
        TeachersVoiceChannel : uint64
        AdminRole : uint64
        TeacherRole : uint64
        StudentRole : uint64
        GroupPersistence : bool
        IsEveryoneStudent : bool }

        static member Get (db : IMongoDatabase) (filter : FilterDefinitionBuilder<GuildConfiguration>) =
            ()

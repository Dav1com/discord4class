namespace Discord4Class.Repositories

open MongoDB.Driver
open Discord4Class.Helpers.Exceptions
open Discord4Class.Config.InnerTypes

module GuildConfiguration =

    [<Literal>]
    let private CollectionName = "GuildConfiguration"

    // when adding fields, remember to handle it at UpdateById
    type GuildConfiguration =
      { _id : uint64 //GuildId
        CommandPrefix : string option
        Language : string option
        TeachersText : uint64 option
        ClassVoice : uint64 option
        TeacherRole : uint64 option }

        static member Filter = Builders<GuildConfiguration>.Filter
        static member Update = Builders<GuildConfiguration>.Update
        static member Base = {
            _id = 0UL
            CommandPrefix = None
            Language = None
            TeachersText = None
            ClassVoice = None
            TeacherRole = None
        }

        static member GetOne (db : IMongoDatabase) (filter : FilterDefinition<GuildConfiguration>) =
            let result =
                db.GetCollection<GuildConfiguration>(CollectionName)
                    .Find<GuildConfiguration>(filter)
            if result.CountDocuments() = 0L then
                async {return None}
            else
                async {return Some <| result.First()}

        static member GetMany (db : IMongoDatabase) (filter : FilterDefinition<GuildConfiguration>) = async {
            return
                db.GetCollection<GuildConfiguration>(CollectionName)
                    .Find<GuildConfiguration>(filter).ToList()
                |> List.ofSeq
        }

        static member Insert (db : IMongoDatabase) (obj : GuildConfiguration) =
            db.GetCollection<GuildConfiguration>(CollectionName)
                .InsertOneAsync(obj)
            |> Async.AwaitTask

        static member InsertMany (db : IMongoDatabase) (ls : GuildConfiguration list) =
            db.GetCollection<GuildConfiguration>(CollectionName)
                .InsertManyAsync(ls)
            |> Async.AwaitTask

        static member UpdateOne (db : IMongoDatabase) (filter : FilterDefinition<GuildConfiguration>) (update : UpdateDefinition<GuildConfiguration>) =
            db.GetCollection<GuildConfiguration>(CollectionName)
                .UpdateOneAsync(filter, update)
            |> Async.AwaitTask

        static member UpdateById (db : IMongoDatabase) (obj : GuildConfiguration) =
            let filter = GC.Filter.And [ GC.Filter.Eq((fun gc -> gc._id), obj._id) ]
            match obj with
            | {CommandPrefix = Some s} ->
                GC.Update.Set((fun gc -> gc.CommandPrefix), Some s)
            | {Language = Some s} ->
                GC.Update.Set((fun gc -> gc.Language), Some s)
            | {TeachersText = Some i} ->
                GC.Update.Set((fun gc -> gc.TeachersText), Some i)
            | {ClassVoice = Some i} ->
                GC.Update.Set((fun gc -> gc.ClassVoice), Some i)
            | {TeacherRole = Some i} ->
                GC.Update.Set((fun gc -> gc.TeacherRole), Some i)
            | _ -> raise ExceptionInsertUpdateNothing
            |> GC.UpdateOne db filter

        static member DeleteOne (db : IMongoDatabase) (filter : FilterDefinition<GuildConfiguration>) =
            db.GetCollection<GuildConfiguration>(CollectionName)
                .DeleteOneAsync filter
            |> Async.AwaitTask

        static member InsertUpdate (db : IMongoDatabase) isUpdate (obj : GuildConfiguration) =
            if isUpdate then
                GC.UpdateById db obj |> Async.Ignore
            else
                GC.Insert db obj

    and GC = GuildConfiguration


namespace Discord4Class.Repositories

open MongoDB.Driver
open Discord4Class.Config.InnerTypes

module GuildConfiguration =

    [<Literal>]
    let private CollectionName = "GuildConfiguration"

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

        static member DeleteOne (db : IMongoDatabase) (filter : FilterDefinition<GuildConfiguration>) =
            db.GetCollection<GuildConfiguration>(CollectionName)
                .DeleteOneAsync filter
            |> Async.AwaitTask

    type GC = GuildConfiguration


namespace Discord4Class.Repositories

open System
open MongoDB.Driver
open MongoDB.Bson

module GuildConfiguration =

    [<Literal>]
    let private CollectionName = "GuildConfiguration"

    type GuildConfiguration =
      { _id : uint64 //GuildId
        CommandPrefix : string
        Language : string }

        static member Filter = Builders<GuildConfiguration>.Filter
        static member Update = Builders<GuildConfiguration>.Update

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

    type GC = GuildConfiguration


namespace Discord4Class.Repositories

open System
open System.Collections.Generic
open MongoDB.Driver
open Discord4Class.Helpers.Exceptions

module MutedChannels =

    [<Literal>]
    let private CollectionName = "MutedChannels"

    type MutedChannels =
      { _id : uint64
        Muted : uint64 list }

        static member Filter = Builders<MutedChannels>.Filter
        static member Update = Builders<MutedChannels>.Update
        static member Base = {
            _id = 0UL
            Muted = []
        }

        static member GetOne (db : IMongoDatabase) (filter : FilterDefinition<_>) =
            let result =
                db.GetCollection<MutedChannels>(CollectionName)
                    .Find<MC>(filter)
            if result.CountDocuments() = 0L then
                async {return None}
            else
                async {return Some <| result.First()}

        static member Insert (db : IMongoDatabase) obj =
            db.GetCollection<MutedChannels>(CollectionName)
                .InsertOneAsync(obj)
            |> Async.AwaitTask

        static member UpdateOne (db : IMongoDatabase) (filter : FilterDefinition<_>) (update : UpdateDefinition<MutedChannels>) =
            db.GetCollection<MutedChannels>(CollectionName)
                .UpdateOneAsync(filter, update)
            |> Async.AwaitTask

        static member DeleteOne (db : IMongoDatabase) (filter : FilterDefinition<_>) =
            db.GetCollection<MutedChannels>(CollectionName)
                .DeleteOneAsync filter
            |> Async.AwaitTask

        static member IsMuted (db : IMongoDatabase) guildId channelId = async { return
            MC.GetOne db (MC.Filter.And [MC.Filter.Eq((fun mc -> mc._id), guildId)])
            |> Async.RunSynchronously
            |> function
                | Some mc ->
                    mc.Muted
                    |> List.contains channelId
                | None -> false
        }

        static member Push db guildId channelId =
            let filter = MC.Filter.And [MC.Filter.Eq((fun mc -> mc._id), guildId)]
            MC.Update.Push((fun mc -> mc.Muted :> IEnumerable<_>), channelId)
            |> MC.UpdateOne db filter

        static member Pull db guildId channelId =
            let filter = MC.Filter.And [MC.Filter.Eq((fun mc -> mc._id), guildId)]
            MC.Update.Pull((fun mc -> mc.Muted :> IEnumerable<_>), channelId)
            |> MC.UpdateOne db filter

    and MC = MutedChannels



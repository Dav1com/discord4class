namespace Discord4Class.Repositories

open System
open MongoDB.Driver
open Discord4Class.Db

module Questions =

    [<Literal>]
    let private collectionName = "Questions"

    type Question =
        { ChId: uint64 // Channel Id
          Id: uint64 } // Message Id

    [<CLIMutable>]
    type QsDocument =
        { Id: uint64
          Count: int
          Questions: seq<Question> }

    type Questions =
        { Id: uint64
          Count: int
          Questions: Question list }

        static member private tToDocument qs =
            { Id = qs.Id
              Count = qs.Count
              Questions = qs.Questions }: QsDocument

        static member private documentToT (qsDoc: QsDocument) =
            { Id = qsDoc.Id
              Count = qsDoc.Count
              Questions = Seq.toList qsDoc.Questions }

        static member Filter = Builders<QsDocument>.Filter
        static member Update = Builders<QsDocument>.Update
        static member Projection = Builders<QsDocument>.Projection
        static member Operation =
            Operation(collectionName, Qs.tToDocument, Qs.documentToT,
                (fun qsd -> qsd.Id))

        static member GetCounter db guildId =
            Qs.Projection.Expression(fun qs -> qs.Count)
            |> Qs.Operation.FindByIdProjection db guildId

        static member PushQuestion db guildId q =
            Qs.Update.Combine [
                Qs.Update.Inc((fun qs -> qs.Count), 1)
                Qs.Update.Push((fun qs -> qs.Questions), q) ]
            |> Qs.Operation.UpdateOneById db guildId

        static member PopQuestion (db: IMongoDatabase) guildId =
            let filter =
                Qs.Filter.And [
                    Qs.Filter.Eq((fun qs -> qs.Id), guildId)
                    Qs.Filter.Gt((fun qs -> qs.Count), 0) ]
            let update =
                Qs.Update.Combine [
                    Qs.Update.Inc((fun qs -> qs.Count), -1)
                    Qs.Update.PopFirst(fun qs -> qs.Questions :> obj) ]
            let options = FindOneAndUpdateOptions<QsDocument,Question option>()
            options.Projection <-
                Qs.Projection.Expression(fun qs ->
                    Seq.tryItem 0 qs.Questions )
            db.GetCollection(collectionName).FindOneAndUpdateAsync<Question option>(
                filter, update, options)
            |> Async.AwaitTask

    and Qs = Questions


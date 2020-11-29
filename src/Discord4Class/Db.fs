namespace Discord4Class

open System
open System.Linq.Expressions
open MongoDB.Driver

module Db =

    [<Literal>]
    let private dbName = "Discord4Class"

    let openConnection (uri: string) =
        MongoClient(uri).GetDatabase dbName

    type Operation<'T, 'TDocument, 'TId>(collName, tToInner, innerToT, idExtractor) =
        let collectionName: string = collName
        let tToInner: 'T -> 'TDocument = tToInner
        let innerToInner: 'TDocument -> 'T = innerToT
        let idExtractor: Expression<Func<'TDocument,'TId>> = idExtractor

        member _.Collection (db: IMongoDatabase) = db.GetCollection<'TDocument>(collectionName)

        member this.InsertMany db coll =
            this.Collection(db).InsertManyAsync(Seq.map tToInner coll)
            |> Async.AwaitTask

        member this.InsertOne db document =
            this.Collection(db).InsertOneAsync(tToInner document)
            |> Async.AwaitTask

        member this.Find db (filter: FilterDefinition<_>) = async { return
            this.Collection(db).Find(filter).ToEnumerable()
            |> Seq.map innerToT }

        member this.FindOne db (filter: FilterDefinition<_>) = async { return
            this.Collection(db).Find(filter).Limit(Nullable<_> 1)
            |> fun r ->
                match r.CountDocuments() with
                | 1L -> Some (innerToT <| r.First())
                | _ -> None }

        member this.FindById db id = this.FindOne db (Builders<_>.Filter.Eq(idExtractor, id))

        member this.FindProjection db (filter: FilterDefinition<_>) (projection: ProjectionDefinition<_,'a>) = async { return
            this.Collection(db).Find(filter).Project(projection).ToEnumerable()
            :> seq<_> }

        member this.FindByIdProjection db id (projection: ProjectionDefinition<_,'a>) = async { return
            this.Collection(db).Find(Builders<_>.Filter.Eq(idExtractor, id))
                .Project(projection).ToEnumerable()
            :> seq<_> }

        member this.UpdateMany db filter update =
            this.Collection(db).UpdateManyAsync(filter, update) |> Async.AwaitTask

        member this.UpdateOne db filter update =
            this.Collection(db).UpdateOneAsync(filter, update) |> Async.AwaitTask

        member this.UpdateOneById db id update =
            this.UpdateOne db (Builders<_>.Filter.Eq(idExtractor, id)) update

        member this.DeleteMany db filter =
            this.Collection(db).DeleteManyAsync(filter) |> Async.AwaitTask

        member this.DeleteOne db filter =
            this.Collection(db).DeleteOneAsync(filter) |> Async.AwaitTask

        member this.DeleteOneById db id =
            this.DeleteOne db (Builders<_>.Filter.Eq(idExtractor, id))

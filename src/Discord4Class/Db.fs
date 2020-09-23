namespace Discord4Class

open System
open MongoDB.Driver
open MongoDB.Bson
open Discord4Class.DbDriver.Types

module Bd =

    [<Literal>]
    let DbName = "Discord4Class"

    let openConnection url user pass db =
        let connectionString =
            sprintf "mongodb://%s%s%s"
                (if not (String.IsNullOrEmpty user) then user + ":" else "" )
                (if not (String.IsNullOrEmpty pass) then pass else "" )
                (if not (String.IsNullOrEmpty user) then "@url" else url)

        MongoClient(connectionString).GetDatabase db


namespace Discord4Class

open System
open MongoDB.Driver

module Db =

    [<Literal>]
    let DbName = "Discord4Class"

    let openConnection (uri : string) =
        MongoClient(uri).GetDatabase DbName

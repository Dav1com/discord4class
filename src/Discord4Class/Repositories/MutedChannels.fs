namespace Discord4Class.Repositories

open System
open MongoDB.Driver
open Discord4Class.Db

module MutedChannels =

    [<Literal>]
    let private collectionName = "MutedChannels"

    [<CLIMutable>]
    type MCDocument =
        { Id: uint64
          Muted: seq<uint64> }

    type MutedChannels =
        { Id: uint64
          Muted: uint64 list }

        static member private tToDocument t =
            { Id = t.Id
              Muted = Seq.ofList t.Muted }: MCDocument

        static member private documentToT (inner: MCDocument) =
            { Id = inner.Id
              Muted = List.ofSeq inner.Muted }

        static member Filter = Builders<MCDocument>.Filter
        static member Update = Builders<MCDocument>.Update
        static member Operation =
            Operation(collectionName, MC.tToDocument, MC.documentToT,
                fun mc -> mc.Id)
        static member Base =
            { Id = 0UL
              Muted = [] }

        static member IsMuted db guildId channelId = async { return
            MC.Filter.And
                [ MC.Filter.Eq((fun mc -> mc.Id), guildId)
                  MC.Filter.AnyEq((fun mc -> mc.Muted), channelId) ]
            |> MC.Operation.FindOne db |> Async.RunSynchronously
            |> Option.isSome }

        static member Pull db guildId channelId =
            MC.Update.Pull((fun mc -> mc.Muted), channelId)
            |> MC.Operation.UpdateOneById db guildId

        static member Push db guildId channelId =
            MC.Update.Push((fun mc -> mc.Muted), channelId)
            |> MC.Operation.UpdateOneById db guildId

    and MC = MutedChannels

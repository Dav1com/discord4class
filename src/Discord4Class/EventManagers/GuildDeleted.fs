namespace Discord4Class.EventManagers

open System.Threading.Tasks
open MongoDB
open DSharpPlus.EventArgs
open Discord4Class.Config.Types
open Discord4Class.Repositories.GuildConfiguration

module GuildDeleted =

    let exec config (e : GuildDeleteEventArgs) =
        async {
            [
                GC.DeleteOne config.App.DbDatabase (GC.Filter.And [
                    GC.Filter.Eq((fun g -> g._id), e.Guild.Id)
                ])
            ]
            |> Async.Parallel
            |> Async.RunSynchronously
            |> ignore
        } |> Async.StartAsTask :> Task

namespace Discord4Class.EventManagers

open System.Threading.Tasks
open MongoDB
open DSharpPlus.EventArgs
open Discord4Class.Config.Types
open Discord4Class.Repositories.GuildData
open Discord4Class.Repositories.RateLimits
open Discord4Class.Repositories.Questions

module GuildDeleted =

    let main config _ (e: GuildDeleteEventArgs) = async {
        [ GD.Operation.DeleteOneById config.App.Db e.Guild.Id
          RT.Operation.DeleteOneById config.App.Db e.Guild.Id
          Qs.Operation.DeleteOneById config.App.Db e.Guild.Id ]
        |> Async.Parallel |> Async.RunSynchronously
        |> ignore
    }

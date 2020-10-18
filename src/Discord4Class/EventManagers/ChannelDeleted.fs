namespace Discord4Class.EventManagers

open System.Threading.Tasks
open DSharpPlus.EventArgs
open Discord4Class.Repositories.GuildConfiguration
open Discord4Class.Repositories.MutedChannels
open Discord4Class.Config.Types

module ChannelDeleted =

    let exec config client (e : ChannelDeleteEventArgs) =
        async {
            let gcFilter = GC.Filter.And [GC.Filter.Eq((fun gc -> gc._id), e.Guild.Id)]
            gcFilter
            |> GC.GetOne config.App.Db
            |> Async.RunSynchronously
            |> function
            | Some {ClassVoice = Some ch } when ch = e.Channel.Id ->
                GC.Update.Set((fun gc -> gc.ClassVoice), None)
                |> GC.UpdateOne config.App.Db gcFilter
                |> Async.RunSynchronously |> ignore
            | Some {TeachersText = Some ch} when ch = e.Channel.Id ->
                GC.Update.Set((fun gc -> gc.TeachersText), None)
                |> GC.UpdateOne config.App.Db gcFilter
                |> Async.RunSynchronously |> ignore
            | _ ->
                let mcFilter = MC.Filter.And [MC.Filter.Eq((fun mc -> mc._id), e.Guild.Id)]
                mcFilter
                |> MC.GetOne config.App.Db
                |> Async.RunSynchronously
                |> function
                | Some _ ->
                    MC.Pull config.App.Db e.Guild.Id e.Channel.Id
                    |> Async.RunSynchronously
                    |> ignore
                | _ -> ()
        } |> Async.StartAsTask :> Task

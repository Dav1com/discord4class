namespace Discord4Class.EventManagers

open System
open System.Linq.Expressions
open System.Threading.Tasks
open DSharpPlus.EventArgs
open Discord4Class.Helpers.Permission
open Discord4Class.Config.Types
open Discord4Class.Repositories.GuildData
open Discord4Class.Repositories.MutedChannels

module VoiceStateUpdated =

    let main config client (e: VoiceStateUpdateEventArgs) = async {
        let memb = e.Guild.Members.[e.User.Id]
        GD.Operation.FindById config.App.Db e.Guild.Id
        |> Async.RunSynchronously
        |> function
        | Some { TeacherRole = Some teacherRole } ->
            if not <| checkIsTeacher teacherRole memb then
                MC.Operation.FindById config.App.Db e.Guild.Id
                |> Async.RunSynchronously
                |> function
                | Some { Muted = muted } ->
                    match e.After with
                    | null when e.Before.IsServerMuted ->
                        memb.SetMuteAsync false
                        |> Async.AwaitTask |> Async.RunSynchronously
                    | vs ->
                        match List.contains vs.Channel.Id muted with
                        | true when not vs.IsServerMuted ->
                            memb.SetMuteAsync true
                            |> Async.AwaitTask |> Async.RunSynchronously
                        | false when vs.IsServerMuted ->
                            memb.SetMuteAsync false
                            |> Async.AwaitTask |> Async.RunSynchronously
                        | _ -> ()
                | _ -> ()
        | _ -> ()
    }

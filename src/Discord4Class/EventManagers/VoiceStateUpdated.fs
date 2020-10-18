namespace Discord4Class.EventManagers

open System
open System.Linq.Expressions
open System.Threading.Tasks
open DSharpPlus.EventArgs
open Discord4Class.Helpers.Permission
open Discord4Class.Config.Types
open Discord4Class.Repositories.GuildConfiguration
open Discord4Class.Repositories.MutedChannels

module VoiceStateUpdated =

    let exec config client (e : VoiceStateUpdateEventArgs) =
        async {
            let memb = e.Guild.Members.[e.User.Id]
            GC.Filter.And [GC.Filter.Eq((fun gc -> gc._id), e.Guild.Id)]
            |> GC.GetOne config.App.Db
            |> Async.RunSynchronously
            |> function
            | Some {TeacherRole = Some teacherRole} ->
                if not <| checkIsTeacher memb teacherRole then
                    MC.Filter.And [
                        MC.Filter.Eq((fun mc -> mc._id), e.Guild.Id)
                        MC.Filter.SizeGte((fun mc -> mc.Muted :> obj), 0)
                    ]
                    |> MC.GetOne config.App.Db
                    |> Async.RunSynchronously
                    |> function
                    | Some {Muted = muted} ->
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
        } |> Async.StartAsTask :> Task

namespace Discord4Class.EventManagers

open System.Threading.Tasks
open DSharpPlus.EventArgs
open Discord4Class.Repositories.GuildConfiguration
open Discord4Class.Config.Types

module GuildRoleDeletes =

    let exec config client (e : GuildRoleDeleteEventArgs) =
        async {
            GC.Filter.And [
                GC.Filter.Eq((fun gc -> gc._id), e.Guild.Id)
                GC.Filter.Eq((fun gc -> gc.TeacherRole), Some e.Role.Id)
            ]
            |> GC.GetOne config.App.Db
            |> Async.RunSynchronously
            |> function
            | Some {TeacherRole = Some teacherRole} ->
                let filter = GC.Filter.And [GC.Filter.Eq((fun gc -> gc._id), e.Guild.Id)]
                GC.Update.Set((fun gc -> gc.TeacherRole), None)
                |> GC.UpdateOne config.App.Db filter
                |> Async.RunSynchronously |> ignore
            | _ -> ()
        } |> Async.StartAsTask :> Task

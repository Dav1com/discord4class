namespace Discord4Class.EventManagers

open System
open DSharpPlus.EventArgs
open Discord4Class.Repositories.GuildData
open Discord4Class.Config.Types

module GuildRoleDeleted =

    let main config client (e: GuildRoleDeleteEventArgs) = async {
        GD.Filter.And
            [ GD.Filter.Eq((fun gd -> gd.Id), e.Guild.Id)
              GD.Filter.Eq((fun gd -> gd.TeacherRole), Nullable<_> e.Role.Id) ]
        |> GD.Operation.FindOne config.App.Db
        |> Async.RunSynchronously
        |> function
            | Some { TeacherRole = Some _ } ->
                GD.Update.Set((fun gd -> gd.TeacherRole), Nullable<_>())
                |> GD.Operation.UpdateOneById config.App.Db e.Guild.Id
                |> Async.RunSynchronously |> ignore
            | _ -> ()
    }

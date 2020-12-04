namespace Discord4Class.EventManagers

open System
open DSharpPlus.EventArgs
open Discord4Class.Repositories.GuildData
open Discord4Class.Repositories.MutedChannels
open Discord4Class.Config.Types

module ChannelDeleted =

    let main config client (e: ChannelDeleteEventArgs) = async {
        GD.Operation.FindById config.App.Db e.Guild.Id
        |> Async.RunSynchronously
        |> function
        | Some { ClassVoice = Some ch } when ch = e.Channel.Id ->
            GD.Update.Set((fun gd -> gd.ClassVoice), Nullable())
            |> GD.Operation.UpdateOneById config.App.Db e.Guild.Id
            |> Async.RunSynchronously |> ignore
        | Some { TeachersText = Some ch } when ch = e.Channel.Id ->
            GD.Update.Set((fun gd -> gd.TeachersText), Nullable())
            |> GD.Operation.UpdateOneById config.App.Db e.Guild.Id
            |> Async.RunSynchronously |> ignore
        | _ ->
            MC.Operation.FindById config.App.Db e.Guild.Id
            |> Async.RunSynchronously
            |> Option.map (fun _ ->
                MC.Pull config.App.Db e.Guild.Id e.Channel.Id
                |> Async.RunSynchronously )
            |> ignore
    }

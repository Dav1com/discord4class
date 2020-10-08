namespace Discord4Class.Helpers

open DSharpPlus.EventArgs

module Permission =

    let checkPermissions (e : MessageCreateEventArgs) requiredPerms =
        let memb =
            e.Guild.GetMemberAsync e.Author.Id
            |> Async.AwaitTask |> Async.RunSynchronously
        let permissions = e.Channel.PermissionsFor memb
        (permissions &&& requiredPerms = requiredPerms || memb = e.Guild.Owner)

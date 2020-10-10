namespace Discord4Class.Helpers

open DSharpPlus
open DSharpPlus.EventArgs

module Permission =

    let checkPermissions (e : MessageCreateEventArgs) requiredPerms =
        let memb =
            e.Guild.GetMemberAsync e.Author.Id
            |> Async.AwaitTask |> Async.RunSynchronously
        let permissions = e.Channel.PermissionsFor memb
        (permissions &&& requiredPerms = requiredPerms || memb = e.Guild.Owner)

    let minPermsText =
        Permissions.AccessChannels +
        Permissions.SendMessages +
        Permissions.ReadMessageHistory +
        Permissions.AttachFiles +
        Permissions.AddReactions +
        Permissions.EmbedLinks

    let minPermsVoice =
        Permissions.AccessChannels +
        Permissions.UseVoice +
        Permissions.Speak +
        Permissions.Stream +
        Permissions.UseVoiceDetection

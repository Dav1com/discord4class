namespace Discord4Class.Helpers

open DSharpPlus
open DSharpPlus.Entities

module Permission =

    let checkPermissions memb (channel: DiscordChannel) requiredPerms =
        let permissions = channel.PermissionsFor memb
        (permissions &&& requiredPerms = requiredPerms)

    let checkIsTeacher teacherRole (memb: DiscordMember) =
        memb.Roles
        |> Seq.tryFind (fun role -> role.Id = teacherRole)
        |> Option.isSome

    let minPermsText =
          Permissions.AccessChannels
        + Permissions.SendMessages
        + Permissions.ReadMessageHistory
        + Permissions.AttachFiles
        + Permissions.AddReactions
        + Permissions.EmbedLinks

    let minPermsVoice =
          Permissions.AccessChannels
        + Permissions.UseVoice
        + Permissions.Speak
        + Permissions.Stream
        + Permissions.UseVoiceDetection

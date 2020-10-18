namespace Discord4Class.Commands.TeamsInternals

open DSharpPlus.Entities

[<AutoOpen>]
module Students =
    let getStudents onlineOnly (teacherRole : DiscordRole) (classVoice : DiscordChannel) (e : DiscordGuild) =
        e.Members
        |> Seq.map (fun x -> x.Value)
        |> Seq.filter (fun memb ->
            not memb.IsBot &&
            (
                not onlineOnly ||
                (
                    not (isNull memb.VoiceState) &&
                    memb.VoiceState.Channel.Id = classVoice.Id
                )
            ) &&
            memb.Roles
            |> Array.ofSeq
            |> Array.tryFind (fun role -> role.Id = teacherRole.Id)
            |> function
                | Some _ -> false
                | None -> true
        )
        |> Array.ofSeq

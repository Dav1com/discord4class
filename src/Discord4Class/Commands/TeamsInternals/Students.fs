namespace Discord4Class.Commands.TeamsInternals

open DSharpPlus.Entities

[<AutoOpen>]
module Students =

    let getStudents onlineOnly (teacherRole: DiscordRole) (classVoice: DiscordChannel) (guild: DiscordGuild) =
        guild.Members
        |> Seq.filter (fun kv ->
            let memb = kv.Value
            not memb.IsBot &&
            ( not onlineOnly ||
              ( not (isNull memb.VoiceState) &&
                memb.VoiceState.Channel.Id = classVoice.Id )
            ) &&
            memb.Roles
            |> List.ofSeq
            |> List.tryFind ((=) teacherRole)
            |> fun o -> o.IsNone )
        |> Seq.map (fun kv -> kv.Value)
        |> Seq.toArray

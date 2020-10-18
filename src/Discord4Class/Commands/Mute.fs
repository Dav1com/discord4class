namespace Discord4Class.Commands

open DSharpPlus.Entities
open DSharpPlus.EventArgs
open Discord4Class.Constants
open Discord4Class.Helpers.Permission
open Discord4Class.Helpers.Messages
open Discord4Class.Config.Types
open Discord4Class.Repositories.MutedChannels

module Mute =

    [<Literal>]
    let MaxMutedChannels = 10

    let private setMuteEveryone (guild : DiscordGuild) (channel : DiscordChannel) mute =
        guild.Members
        |> Seq.filter (fun memb ->
            match memb.Value.VoiceState with
            | null -> false
            | vs -> (vs.Channel.Id = channel.Id)
        )
        |> Seq.map (fun memb -> memb.Value.SetMuteAsync(mute) |> Async.AwaitTask )
        |> Async.Parallel |> Async.RunSynchronously |> ignore

    let exec app guild client _args (e : MessageCreateEventArgs) = async {
        let memb = e.Guild.Members.[e.Author.Id]
        if guild.TeacherRole.IsNone then
            guild.Lang.ErrorRoleNull "teacher-role"
                guild.CommandPrefix "teacher-role"
            |> sendMessage e.Channel |> ignore
        elif checkIsTeacher memb guild.TeacherRole.Value then
            match memb.VoiceState with
            | null ->
                "Not in channel"
                |> sendMessage e.Channel |> ignore
            | v ->
                let channelId = v.Channel.Id
                let filter = MC.Filter.And [MC.Filter.Eq((fun mc -> mc._id), e.Guild.Id)]
                filter
                |> MC.GetOne app.Db
                |> Async.RunSynchronously
                |> function
                | Some mc ->
                    mc.Muted
                    |> List.contains v.Channel.Id
                    |> function
                    | true ->
                        MC.Pull app.Db e.Guild.Id channelId
                        |> Async.RunSynchronously |> ignore
                        addReaction e.Message client app.Emojis.Mute
                    | false when mc.Muted.Length < MaxMutedChannels ->
                        MC.Push app.Db e.Guild.Id channelId
                        |> Async.RunSynchronously |> ignore
                        addReaction e.Message client app.Emojis.UnMute
                    | false ->
                        "Too many muted channels"
                        |> sendMessage e.Channel |> ignore
                | None ->
                    MC.Insert app.Db {
                        _id = e.Guild.Id
                        Muted = [channelId]
                    }
                    |> Async.RunSynchronously
                    "Insert Success"
                    |> sendMessage e.Channel |> ignore
    }

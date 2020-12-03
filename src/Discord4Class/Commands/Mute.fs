namespace Discord4Class.Commands

open DSharpPlus.Entities
open DSharpPlus.EventArgs
open Discord4Class.Helpers.Permission
open Discord4Class.Helpers.Messages
open Discord4Class.Config.Types
open Discord4Class.Repositories.MutedChannels
open Discord4Class.CommandsManager

module Mute =

    [<Literal>]
    let private maxMutedChannels = 10

    let private setMuteEveryone teacherRole (guild: DiscordGuild) (channel : DiscordChannel) mute =
        channel.Users
        |> Seq.filter (checkIsTeacher teacherRole >> not)
        |> Seq.map (fun memb -> memb.SetMuteAsync(mute) |> Async.AwaitTask )
        |> Async.Parallel |> Async.RunSynchronously |> ignore

    let main app guild client _args (memb: DiscordMember) (e: MessageCreateEventArgs) = async {
        match memb.VoiceState with
        | null ->
            "Not in channel"
            |> sendMessage e.Channel |> ignore
        | v ->
            let channelId = v.Channel.Id
            MC.Operation.FindById app.Db e.Guild.Id
            |> Async.RunSynchronously
            |> function
            | Some { Muted = muted } ->
                muted
                |> List.contains v.Channel.Id
                |> function
                | true ->
                    MC.Pull app.Db e.Guild.Id channelId
                    |> Async.RunSynchronously |> ignore
                    addReaction e.Message app.Emojis.UnMute
                | false when muted.Length < maxMutedChannels ->
                    MC.Push app.Db e.Guild.Id channelId
                    |> Async.RunSynchronously |> ignore
                    addReaction e.Message app.Emojis.Mute
                | false ->
                    "Too many muted channels"
                    |> sendMessage e.Channel |> ignore
            | None ->
                MC.Operation.InsertOne app.Db
                    { Id = e.Guild.Id
                      Muted = [ channelId ] }
                |> Async.RunSynchronously
                "Insert Success"
                |> sendMessage e.Channel |> ignore
    }

    let command =
        { BaseCommand with
            Names = [ "mute" ]
            Description = fun gc ->
                gc.Lang.MuteDescription maxMutedChannels
                    gc.CommandPrefix gc.Lang.MuteUsage
            Permissions = Teacher
            IsHeavy = true
            RateLimits = [
                { Allowed = 5uy
                  Interval = 30UL} ]
            Function = main }

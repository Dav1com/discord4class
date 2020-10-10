namespace Discord4Class.Commands.TeamsInternals

open DSharpPlus.Entities

[<AutoOpen>]
module Move =
    let moveMembers (classVoice : DiscordChannel) (channels : Map<string, DiscordChannel>) teams =
        teams
        |> Array.map (fun (name : string, team : DiscordMember array) ->
            let channel =
                channels
                |> Map.tryFind name
            team
            |> Array.map (fun memb ->
                match memb.VoiceState with
                | null -> async.Zero()
                | state ->
                    if state.Channel.Id = classVoice.Id then
                        match channel with
                        | Some ch ->
                            ch.PlaceMemberAsync memb
                            |> Async.AwaitTask
                        | None -> async.Zero()
                    else
                        async.Zero()
            )
            |> Async.Parallel
        )
        |> Async.Parallel |> Async.RunSynchronously
        |> ignore

    let returnMembers (classVoice : DiscordChannel) (channels : Map<string,DiscordChannel>) teams =
        teams
        |> Array.map (fun (name : string, team : DiscordMember array) ->
            let channel =
                channels
                |> Map.tryFind name
            team
            |> Array.map (fun memb ->
                match channel with
                | Some ch when not (isNull memb.VoiceState) && memb.VoiceState.Channel = ch ->
                    classVoice.PlaceMemberAsync memb
                    |> Async.AwaitTask
                | _ -> async.Zero()
            )
            |> Async.Parallel |> Async.Ignore
        )
        |> Async.Parallel |> Async.RunSynchronously |> ignore

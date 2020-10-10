namespace Discord4Class.Commands

open System
open DSharpPlus
open DSharpPlus.EventArgs
open Discord4Class.Helpers.String
open Discord4Class.Config.Types
open Discord4Class.Commands.TeamsInternals

module Teams =

    let exec config _client (args : string) (e : MessageCreateEventArgs) = async {
        if config.Guild.TeacherRole.IsNone then
            ()
        elif config.Guild.ClassVoice.IsNone then
            ()
        else
            (
                config.Guild.TeacherRole.Value
                |> e.Guild.GetRole,
                config.Guild.ClassVoice.Value
                |> e.Guild.GetChannel
            )
            |> function
            | (null,_) -> ()
            | (_,null) -> ()
            | (teacherRole, classVoice) ->
                let phrased =
                    args.Split " "
                    |> Array.filter (String.IsNullOrEmpty >> not)
                    |> Array.map (fun s -> s.Trim().ToLower())
                match phrased with
                | [|"move"|] ->
                    let channels = getTeamChannels config ChannelType.Voice e.Guild
                    getTeamRoles config e.Guild
                    |> getTeamMembers config e.Guild
                    |> List.choose (function
                        | Ok (a,b) -> Some (a,b)
                        | _ -> None
                    )
                    |> Array.ofList
                    |> moveMembers classVoice channels
                    config.Guild.Lang.TeamsMoved
                | [|"return"|] ->
                    let channels = getTeamChannels config ChannelType.Voice e.Guild
                    getTeamRoles config e.Guild
                    |> getTeamMembers config e.Guild
                    |> List.choose (function
                        | Ok (a,b) -> Some (a,b)
                        | _ -> None
                    )
                    |> Array.ofList
                    |> returnMembers classVoice channels
                    config.Guild.Lang.TeamsReturned
                | [|"destroy"|] ->
                    deleteChannels config e.Guild
                    getTeamRoles config e.Guild
                    |> deleteRoles
                    config.Guild.Lang.TeamsDestroyed
                | [|"manual"|] ->
                    let roles = getTeamRoles config e.Guild
                    let teams =
                        roles
                        |> getTeamMembers config e.Guild
                        |> List.choose (function
                            | Ok (a,b) -> Some (a,b)
                            | _ -> None
                        )
                        |> Array.ofList
                    createChannels teacherRole e.Guild teams roles
                    config.Guild.Lang.TeamsChannelsCreated
                | [|"size"; "0"|] | [|"online"; "size"; "0"|] ->
                    config.Guild.Lang.TeamsSizeZero
                | [|"size"; num|] | [|"online"; "size"; num|] ->
                    if not (isNumeric num) then
                        config.Guild.Lang.TeamsNonNumericArgument config.Guild.CommandPrefix
                    elif existsTeams config e.Guild then
                        config.Guild.Lang.TeamsAlreadyCreated config.Guild.CommandPrefix
                    else
                        let students = getStudents (phrased.Length = 3) teacherRole classVoice e.Guild
                        let size = int num
                        if size > students.Length then
                            config.Guild.Lang.TeamsSizeTooLarge
                        else
                            let teams = makeTeams true config students size
                            teams
                            |> createRoles e.Guild
                            |> createChannels teacherRole e.Guild teams
                            config.Guild.Lang.TeamsSuccess
                | [|"0"|] | [|"online"; "0"|] ->
                    config.Guild.Lang.TeamsNumberZero
                | [|num|] | [|"online"; num|] ->
                    if not (isNumeric num) then
                        config.Guild.Lang.TeamsNonNumericArgument config.Guild.CommandPrefix
                    elif existsTeams config e.Guild then
                        config.Guild.Lang.TeamsAlreadyCreated config.Guild.CommandPrefix
                    else
                        let teamsCount = int num;
                        let students = getStudents (phrased.Length = 2) teacherRole classVoice e.Guild
                        if teamsCount > students.Length then
                            config.Guild.Lang.TeamsNumberTooLarge
                        else
                            let teams = makeTeams false config students teamsCount
                            teams
                            |> createRoles e.Guild
                            |> createChannels teacherRole e.Guild teams
                            config.Guild.Lang.TeamsSuccess
                | _ ->
                    config.Guild.Lang.TeamsMissingArgument config.Guild.CommandPrefix
                |> fun s -> e.Channel.SendMessageAsync(s)
                |> Async.AwaitTask |> Async.RunSynchronously |> ignore
    }

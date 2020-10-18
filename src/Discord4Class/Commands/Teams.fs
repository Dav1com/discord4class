namespace Discord4Class.Commands

open System
open DSharpPlus
open DSharpPlus.Entities
open DSharpPlus.EventArgs
open Discord4Class.Constants
open Discord4Class.Helpers.Messages
open Discord4Class.Helpers.String
open Discord4Class.Helpers.Permission
open Discord4Class.Config.Types
open Discord4Class.Commands.TeamsInternals

module Teams =

    [<Literal>]
    let StudentsLimit = 1000
    [<Literal>]
    let TeamsLimit = 100

    let exec app guild client (args : string) (e : MessageCreateEventArgs) = async {
        let thisMemb = e.Guild.Members.[e.Author.Id]
        if guild.TeacherRole.IsNone then
            guild.Lang.ErrorRoleNull "teacher-role"
                guild.CommandPrefix "teacher-rol"
            |> sendMessage e.Channel |> ignore
        elif guild.ClassVoice.IsNone then
            guild.Lang.ErrorVoiceChannelNull "class-voice"
                guild.CommandPrefix "class-voice"
            |> sendMessage e.Channel |> ignore
        elif checkIsTeacher thisMemb guild.TeacherRole.Value then
            (
                guild.TeacherRole.Value
                |> e.Guild.GetRole,
                guild.ClassVoice.Value
                |> e.Guild.GetChannel
            )
            |> function
            | (null,_) ->
                guild.Lang.ErrorRoleDeleted "teacher-rol"
                    guild.CommandPrefix "teacher-rol"
                |> sendMessage e.Channel |> ignore
            | (_,null) ->
                guild.Lang.ErrorVoiceChannelDeleted "class-voice"
                    guild.CommandPrefix "class-voice"
                |> sendMessage e.Channel |> ignore
            | (teacherRole, classVoice) ->
                let phrased =
                    args.Split " "
                    |> Array.filter (String.IsNullOrEmpty >> not)
                    |> Array.map (fun s -> s.Trim().ToLower())
                match phrased with
                | [|"move"|] ->
                    addReaction e.Message client app.Emojis.Doing
                    let channels = getTeamChannels guild ChannelType.Voice e.Guild
                    getTeamRoles guild e.Guild
                    |> getTeamMembers guild e.Guild
                    |> List.choose (function
                        | Ok (a,b) -> Some (a,b)
                        | _ -> None
                    )
                    |> Array.ofList
                    |> moveMembers classVoice channels
                    exchangeReactions e.Message client app.Emojis.Doing app.Emojis.Yes
                | [|"return"|] ->
                    addReaction e.Message client app.Emojis.Doing
                    let channels = getTeamChannels guild ChannelType.Voice e.Guild
                    getTeamRoles guild e.Guild
                    |> getTeamMembers guild e.Guild
                    |> List.choose (function
                        | Ok (a,b) -> Some (a,b)
                        | _ -> None
                    )
                    |> Array.ofList
                    |> returnMembers classVoice channels
                    exchangeReactions e.Message client app.Emojis.Doing app.Emojis.Yes
                | [|"destroy"|] ->
                    addReaction e.Message client app.Emojis.Doing
                    deleteChannels guild e.Guild
                    getTeamRoles guild e.Guild
                    |> deleteRoles
                    exchangeReactions e.Message client app.Emojis.Doing app.Emojis.Yes
                | [|"manual"|] ->
                    addReaction e.Message client app.Emojis.Doing
                    let roles = getTeamRoles guild e.Guild
                    let teams =
                        roles
                        |> getTeamMembers guild e.Guild
                        |> List.choose (function
                            | Ok (a,b) -> Some (a,b)
                            | _ -> None
                        )
                        |> Array.ofList
                    createChannels teacherRole e.Guild teams roles
                    exchangeReactions e.Message client app.Emojis.Doing app.Emojis.Yes
                | [|"size"; "0"|] | [|"online"; "size"; "0"|] ->
                    guild.Lang.TeamsSizeZero
                    |> sendMessage e.Channel |> ignore
                | [|"size"; num|] | [|"online"; "size"; num|] ->
                    if not (isNumeric num) then
                        guild.Lang.TeamsNonNumericArgument guild.CommandPrefix
                        |> sendMessage e.Channel |> ignore
                    elif existsTeams guild e.Guild then
                        guild.Lang.TeamsAlreadyCreated guild.CommandPrefix
                        |> sendMessage e.Channel |> ignore
                    else
                        let students = getStudents (phrased.Length = 3) teacherRole classVoice e.Guild
                        let size = int num
                        if size > students.Length then
                            guild.Lang.TeamsSizeTooLarge students.Length
                            |> sendMessage e.Channel |> ignore
                        else
                            addReaction e.Message client app.Emojis.Doing
                            let teams = makeTeams true guild students size
                            teams
                            |> createRoles e.Guild
                            |> createChannels teacherRole e.Guild teams
                            exchangeReactions e.Message client app.Emojis.Doing app.Emojis.Yes
                | [|"0"|] | [|"online"; "0"|] ->
                    guild.Lang.TeamsNumberZero
                    |> sendMessage e.Channel |> ignore
                | [|num|] | [|"online"; num|] ->
                    if not (isNumeric num) then
                        guild.Lang.TeamsNonNumericArgument guild.CommandPrefix
                        |> sendMessage e.Channel |> ignore
                    elif existsTeams guild e.Guild then
                        guild.Lang.TeamsAlreadyCreated guild.CommandPrefix
                        |> sendMessage e.Channel |> ignore
                    else
                        let teamsCount = int num;
                        let students = getStudents (phrased.Length = 2) teacherRole classVoice e.Guild
                        if teamsCount > students.Length then
                            guild.Lang.TeamsNumberTooLarge students.Length
                            |> sendMessage e.Channel |> ignore
                        else
                            addReaction e.Message client app.Emojis.Doing
                            let teams = makeTeams false guild students teamsCount
                            teams
                            |> createRoles e.Guild
                            |> createChannels teacherRole e.Guild teams
                            exchangeReactions e.Message client app.Emojis.Doing app.Emojis.Yes
                | _ ->
                    guild.Lang.TeamsInvalidArguments guild.CommandPrefix
                    |> sendMessage e.Channel |> ignore
    }

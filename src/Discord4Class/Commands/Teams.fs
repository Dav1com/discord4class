namespace Discord4Class.Commands

open System
open DSharpPlus
open DSharpPlus.Entities
open DSharpPlus.EventArgs
open Discord4Class.Helpers.Random
open Discord4Class.Helpers.Railway
open Discord4Class.Helpers.Permission
open Discord4Class.Helpers.String
open Discord4Class.Config.Types

module Teams =

    type private GroupingResult =
        | Ok of string * DiscordMember array
        | UserHasMultipleTeams of string
        | MissingTeamVoiceChannel of string
        | UserHasDuplicateTeam of string
        | UserHasNoGroup

    type private GroupingChannelResult =
        | OkChannel of string * DiscordChannel
        | ChannelNotFound of string

    let private getStudents (teacherRole : DiscordRole) (e : DiscordGuild) =
        e.Members
        |> Seq.map (fun x -> x.Value)
        |> Seq.filter (fun memb ->
            not memb.IsBot &&
            memb.Roles
            |> Array.ofSeq
            |> Array.tryFind (fun role -> role.Id = teacherRole.Id)
            |> function
                | Some _ -> false
                | None -> true
        )
        |> Array.ofSeq

    let private isTeamRole config (role : DiscordRole) =
        (
            config.Guild.Lang.TeamsNumberIsRight = "1" &&
            role.Name.StartsWith config.Guild.Lang.Team
        ) ||
        (
            config.Guild.Lang.TeamsNumberIsRight = "0" &&
            role.Name.EndsWith config.Guild.Lang.Team
        )

    let private isTeamChannel config (channel : DiscordChannel) =
        (
            config.Guild.Lang.TeamsNumberIsRight = "1" &&
            channel.Name.ToLower().StartsWith (config.Guild.Lang.Team.ToLower())
        ) ||
        (
            config.Guild.Lang.TeamsNumberIsRight = "0" &&
            channel.Name.ToLower().EndsWith (config.Guild.Lang.Team.ToLower())
        )

    let private makeTeams bySize studentsNum num =
        let membersPerTeam =
            if bySize then num
            else studentsNum / num
        let teamsCount =
            if bySize then
                (studentsNum / num) +
                if studentsNum % num = 0 then 0
                else 1
            else num
        let mutable group = 1
        let mutable count = 0
        randomIndexes(studentsNum)
        |> Array.groupBy (fun _ ->
            if group >= teamsCount then
                group
            else
                if count < membersPerTeam then
                    count <- count + 1
                    group
                else
                    count <- 1
                    group <- group+1
                    group
        )

    let private getGroupRoles config (e : DiscordGuild) =
        e.Roles
        |> Seq.map (fun kv -> kv.Value)
        |> Seq.filter (isTeamRole config)
        |> Seq.distinctBy (fun role -> role.Name)
        |> Array.ofSeq

    let private existsTeams config (e : DiscordGuild) =
        e.Roles
        |> Seq.map (fun kv -> kv.Value)
        |> Seq.tryFind (isTeamRole config)
        |> function
            | Some _ -> true
            | None -> false

    let private getTeamMembers config (e : DiscordGuild) roles =
        e.Members
        |> Array.ofSeq
        |> Array.fold (fun acc kv ->
            let memb = kv.Value
            memb.Roles
            |> Array.ofSeq
            |> Array.filter (isTeamRole config)
            |> function
                | [||] -> (UserHasNoGroup)::acc
                | [|role|] ->
                    roles
                    |> Array.tryFind ((=) role)
                    |> function
                        | Some r ->
                            acc
                            |> List.tryFindIndex (function
                                | Ok (name, _) when name = role.Name -> true
                                | _ -> false
                            )
                            |> function
                                | Some i ->
                                    acc
                                    |> List.mapi (fun j result ->
                                        match j with
                                        | j when j = i ->
                                            match result with
                                            | Ok (name, arr) ->
                                                arr
                                                |> Array.append [|memb|]
                                                |> fun x -> Ok (name, x)
                                            | x -> x
                                        | _ -> result
                                    )
                                | None ->
                                    (Ok (role.Name, [|memb|]))::acc
                        | None -> (UserHasDuplicateTeam memb.Mention)::acc
                | _ -> (UserHasMultipleTeams memb.Mention)::acc
        ) []

    let private getTeamCategory config (e : DiscordGuild) (roles : DiscordRole array) =
        let channels =
            e.Channels
            |> Seq.map (fun kv -> kv.Value)
            |> Seq.filter (fun ch -> ch.Type = ChannelType.Category && isTeamChannel config ch)
        roles
        |> Array.map (fun role ->
            channels
            |> Seq.tryFind (fun ch -> ch.Name = role.Name)
            |> function
                | Some ch -> OkChannel (role.Name, ch)
                | None -> ChannelNotFound role.Name
        )

    let private createChannelsAndRoles config (students : DiscordMember array) (teacherRole : DiscordRole) (e : DiscordGuild) groups =
        groups
        |> Array.map (fun (i, group) -> async {
            let name = config.Guild.Lang.TeamsTemplate i
            let! category =
                e.CreateChannelCategoryAsync(name)
                |> Async.AwaitTask
            let! role =
                e.CreateRoleAsync(name)
                |> Async.AwaitTask
            [
                e.CreateChannelAsync(
                    name, ChannelType.Text, category,
                    overwrites = [
                        DiscordOverwriteBuilder()
                            .For(e.EveryoneRole)
                            .Deny(Permissions.All)
                        DiscordOverwriteBuilder()
                            .For(role)
                            .Allow(minPermsText)
                        DiscordOverwriteBuilder()
                            .For(teacherRole)
                            .Allow(Permissions.All)
                        DiscordOverwriteBuilder()
                            .For(e.CurrentMember)
                            .Allow(minPermsText + Permissions.ManageChannels)
                    ]
                ) |> Async.AwaitTask |> Async.Ignore
                e.CreateChannelAsync(
                    name, ChannelType.Voice, category,
                    overwrites = [
                        DiscordOverwriteBuilder()
                            .For(e.EveryoneRole)
                            .Deny(Permissions.All)
                        DiscordOverwriteBuilder()
                            .For(role)
                            .Allow(minPermsVoice)
                        DiscordOverwriteBuilder()
                            .For(teacherRole)
                            .Allow(Permissions.All)
                        DiscordOverwriteBuilder()
                            .For(e.CurrentMember)
                            .Allow(minPermsVoice + Permissions.ManageChannels)
                    ]
                ) |> Async.AwaitTask |> Async.Ignore
                group
                |> Array.map (fun j ->
                    students.[j].GrantRoleAsync role
                    |> Async.AwaitTask
                )
                |> Async.Parallel |> Async.Ignore
            ]
            |> Async.Parallel |> Async.RunSynchronously |> ignore
        })
        |> Async.Sequential |> Async.RunSynchronously |> ignore

    let private moveMembers groups (classVoice : DiscordChannel) =
        groups
        |> Array.map (fun (group : DiscordMember array, channel : DiscordChannel) ->
            group
            |> Array.map (fun memb ->
                match memb.VoiceState with
                | null -> async.Zero()
                | state ->
                    if state.Channel.Id = classVoice.Id then
                        channel.PlaceMemberAsync memb
                        |> Async.AwaitTask
                    else
                        async.Zero()
            )
            |> Async.Parallel
        )
        |> Async.Parallel |> Async.RunSynchronously
        |> ignore

    let exec config client (args : string) (e : MessageCreateEventArgs) = async {
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
                args.Trim().Split " "
                |> Array.filter (String.IsNullOrEmpty >> not)
                |> Array.map (fun s -> s.Trim())
                |> function
                | [|"move"|] ->
                    // check existance of groups and voice channels
                    // move members
                    ""
                | [|"return"|] ->
                    ""
                | [|"destroy"|] ->
                    // delete all channels
                    e.Guild.Channels
                    |> Seq.map (fun kv -> kv.Value)
                    |> Seq.filter (isTeamChannel config)
                    |> fun x -> printfn "TEST: %A" x; x
                    |> Seq.map (fun ch ->
                        ch.DeleteAsync()
                        |> Async.AwaitTask
                    )
                    |> Async.Parallel |> Async.RunSynchronously |> ignore
                    // delete all roles
                    getGroupRoles config e.Guild
                    |> Array.map (fun role ->
                        role.DeleteAsync()
                        |> Async.AwaitTask
                    )
                    |> Async.Parallel |> Async.RunSynchronously |> ignore
                    // message
                    config.Guild.Lang.TeamsDestroyed
                | [|"size"; "0"|] -> config.Guild.Lang.TeamsSizeZero
                | [|"size"; num|] ->
                    if not (isNumeric num) then
                        // Error, num is not numeric
                        config.Guild.Lang.TeamsNonNumericArgument config.Guild.CommandPrefix
                    elif existsTeams config e.Guild then
                        config.Guild.Lang.TeamsAlreadyCreated config.Guild.CommandPrefix
                    else
                        let students = getStudents teacherRole e.Guild
                        let size = int num
                        if size > students.Length then
                            // Error, size is too large
                            config.Guild.Lang.TeamsSizeTooLarge
                        else
                            // teams count
                            makeTeams true students.Length size
                            |> createChannelsAndRoles config students teacherRole e.Guild
                            config.Guild.Lang.TeamsSuccess
                | [|"0"|] -> config.Guild.Lang.TeamsNumberZero
                | [|num|] ->
                    if not (isNumeric num) then
                        // Error, num is not numeric
                        config.Guild.Lang.TeamsNonNumericArgument config.Guild.CommandPrefix
                    elif existsTeams config e.Guild then
                        config.Guild.Lang.TeamsAlreadyCreated config.Guild.CommandPrefix
                    else
                        let teamsCount = int num;
                        let students = getStudents teacherRole e.Guild
                        if teamsCount > students.Length then
                            config.Guild.Lang.TeamsNumberTooLarge
                        else
                            makeTeams false students.Length teamsCount
                            |> createChannelsAndRoles config students teacherRole e.Guild
                            config.Guild.Lang.TeamsSuccess
                | _ ->
                    config.Guild.Lang.TeamsMissingArgument config.Guild.CommandPrefix
                |> fun s -> e.Channel.SendMessageAsync(s)
                |> Async.AwaitTask |> Async.RunSynchronously |> ignore
    }

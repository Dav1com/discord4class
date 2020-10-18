namespace Discord4Class.Commands.TeamsInternals

open DSharpPlus
open DSharpPlus.Entities
open Discord4Class.Helpers.Random
open Discord4Class.Helpers.Permission
open Discord4Class.Config.Types

[<AutoOpen>]
module CreateTeams =
    let makeTeams bySize guild (students : DiscordMember array) num =
        let studentsNum = students.Length
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
        |> Array.map (fun i -> students.[i])
        |> Array.groupBy (fun _ ->
            if group >= teamsCount then
                guild.Lang.TeamsTemplate group
            else
                if count < membersPerTeam then
                    count <- count + 1
                    guild.Lang.TeamsTemplate group
                else
                    count <- 1
                    group <- group+1
                    guild.Lang.TeamsTemplate group
        )

    let createRoles (guild : DiscordGuild) (teams : (string * DiscordMember array) array) =
        teams
        |> Array.map (fun (name, team) -> async {
            let! role =
                guild.CreateRoleAsync(name)
                |> Async.AwaitTask
            team
            |> Array.map (fun memb ->
                memb.GrantRoleAsync role
                |> Async.AwaitTask
            )
            |> Async.Parallel |> Async.RunSynchronously |> ignore
            return role
        })
        |> Async.Parallel |> Async.RunSynchronously

    let createChannels (teacherRole : DiscordRole) (guild : DiscordGuild) (teams : (string * DiscordMember array) array) (roles : DiscordRole array) =
        teams
        |> Array.mapi (fun i (name, team) -> async {
            let! category =
                guild.CreateChannelCategoryAsync(name)
                |> Async.AwaitTask
            let role = roles.[i]
            [
                guild.CreateChannelAsync(
                    name, ChannelType.Text, category,
                    overwrites = [
                        DiscordOverwriteBuilder()
                            .For(guild.EveryoneRole)
                            .Deny(Permissions.All)
                        DiscordOverwriteBuilder()
                            .For(role)
                            .Allow(minPermsText)
                        DiscordOverwriteBuilder()
                            .For(teacherRole)
                            .Allow(Permissions.All)
                    ]
                ) |> Async.AwaitTask |> Async.Ignore
                guild.CreateChannelAsync(
                    name, ChannelType.Voice, category,
                    overwrites = [
                        DiscordOverwriteBuilder()
                            .For(guild.EveryoneRole)
                            .Deny(Permissions.All)
                        DiscordOverwriteBuilder()
                            .For(role)
                            .Allow(minPermsVoice)
                        DiscordOverwriteBuilder()
                            .For(teacherRole)
                            .Allow(Permissions.All)
                    ]
                ) |> Async.AwaitTask |> Async.Ignore
            ]
            |> Async.Parallel |> Async.RunSynchronously |> ignore
        })
        |> Async.Sequential |> Async.RunSynchronously |> ignore

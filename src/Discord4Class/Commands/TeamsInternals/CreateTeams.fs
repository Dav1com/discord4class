namespace Discord4Class.Commands.TeamsInternals

open DSharpPlus
open DSharpPlus.Entities
open Discord4Class.Helpers.Random
open Discord4Class.Helpers.Permission
open Discord4Class.Config.Types

[<AutoOpen>]
module CreateTeams =

    let private botPermissions =
          Permissions.ManageChannels
        + Permissions.AccessChannels
        + Permissions.SendMessages
        + Permissions.ManageMessages
        + Permissions.EmbedLinks
        + Permissions.AttachFiles
        + Permissions.ReadMessageHistory
        + Permissions.UseExternalEmojis
        + Permissions.AddReactions
        + Permissions.UseVoice
        + Permissions.MuteMembers
        + Permissions.MoveMembers

    let private extractTeamNumber guild (name: string) =
        if guild.Lang.TeamsNumberIsRight = "1" then
            let pos = name.LastIndexOf ' '
            name.[pos+1..]
        else
            let pos = name.IndexOf ' '
            name.[..pos-1]
        |> int

    let makeTeams guild (students: DiscordMember[]) teamsCount =
        randomPermute students
        |> Array.splitInto teamsCount
        |> Array.mapi (fun i membArr ->
            (guild.Lang.TeamsTemplate (i + 1), List.ofArray membArr) )
        |> Map.ofArray

    let createRoles (guild: DiscordGuild) (teams: Map<string, DiscordMember list>) =
        teams
        |> Map.toSeq
        |> Seq.map (fun (name, team) -> async {
            let! role =
                guild.CreateRoleAsync name
                |> Async.AwaitTask
            team
            |> List.map (fun memb ->
                memb.GrantRoleAsync role
                |> Async.AwaitTask )
            |> Async.Parallel |> Async.RunSynchronously |> ignore
            return (name, role) } )
        |> Async.Parallel |> Async.RunSynchronously
        |> Map.ofSeq

    let createChannels config (teacherRole: DiscordRole) (guild: DiscordGuild) (teams: Map<string, DiscordMember list>) (roles: Map<string,DiscordRole>) =
        teams
        |> Map.toSeq
        |> Seq.map (fun (name, team) -> async {
            let role = roles.[name]
            let! category =
                guild.CreateChannelCategoryAsync(name,
                    [ DiscordOverwriteBuilder()
                        .For(guild.EveryoneRole)
                        .Deny(Permissions.All)
                      DiscordOverwriteBuilder()
                        .For(role)
                        .Allow(minPermsText)
                      DiscordOverwriteBuilder()
                        .For(teacherRole)
                        .Allow(Permissions.All)
                      DiscordOverwriteBuilder()
                        .For(guild.CurrentMember)
                        .Allow(botPermissions) ] )
                |> Async.AwaitTask
            [ guild.CreateChannelAsync(
                name, ChannelType.Text, category )
              |> Async.AwaitTask |> Async.Ignore
              guild.CreateChannelAsync(
                name, ChannelType.Voice, category)
              |> Async.AwaitTask |> Async.Ignore ]
            |> Async.Parallel |> Async.RunSynchronously |> ignore } )
        |> Async.Sequential |> Async.RunSynchronously |> ignore

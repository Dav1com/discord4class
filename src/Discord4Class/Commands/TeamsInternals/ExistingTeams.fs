namespace Discord4Class.Commands.TeamsInternals

open DSharpPlus.Entities
open Discord4Class.Helpers.Railway
open Discord4Class.Commands.TeamsInternals.Predicates
open Discord4Class.Commands.TeamsInternals.Results

[<AutoOpen>]
module ExistingTeams =

    let getTeamRoles config (guild: DiscordGuild) =
        guild.Roles
        |> Seq.choose (fun kv ->
            if isTeamRole config kv.Value then Some (kv.Value.Name, kv.Value)
            else None )
        |> Seq.distinctBy (fun (name,_) -> name)
        |> Map.ofSeq

    let getTeamsMembers config (guild: DiscordGuild) ignoreErrors singleAddErrored =
        guild.Members
        |> Seq.fold (fun res kv ->
            res
            >>= fun acc ->
                let memb = kv.Value
                memb.Roles
                |> List.ofSeq
                |> List.filter (isTeamRole config)
                |> function
                    | [] -> Ok acc
                    | membRoles when ignoreErrors || membRoles.Length = 1 ->
                        membRoles
                        |> List.fold (fun (acc2: Map<string,DiscordMember list>) role ->
                            let name = role.Name
                            match Map.tryFind name acc with
                            | Some membs -> acc2.Add(name, memb :: membs)
                            | None -> acc2.Add(name, [memb]) ) acc
                        |> Ok
                    | membRoles when singleAddErrored ->
                        let name = membRoles.[0].Name
                        match Map.tryFind name acc with
                        | Some membs -> acc.Add(name, memb :: membs)
                        | None -> acc.Add(name, [memb])
                        |> Ok
                    | _ -> Error (UserHasMultipleTeams memb) )
            (Ok Map.empty)

    let getTeamsChannels config chType (guild: DiscordGuild) =
        guild.Channels
        |> Seq.choose (fun kv ->
            if kv.Value.Type = chType && isTeamChannel config kv.Value then
                Some (kv.Value.Name, kv.Value)
            else
                None )
        |> Map.ofSeq

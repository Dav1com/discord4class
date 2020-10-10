namespace Discord4Class.Commands.TeamsInternals

open DSharpPlus.Entities
open Discord4Class.Commands.TeamsInternals.Predicates
open Discord4Class.Commands.TeamsInternals.Results

[<AutoOpen>]
module ExistingTeams =
    let getTeamRoles config (e : DiscordGuild) =
        e.Roles
        |> Seq.map (fun kv -> kv.Value)
        |> Seq.filter (isTeamRole config)
        |> Seq.distinctBy (fun role -> role.Name)
        |> Array.ofSeq

    let getTeamMembers config (e : DiscordGuild) roles =
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

    let getTeamChannels config chType (guild : DiscordGuild) =
        guild.Channels
        |> Seq.choose (fun kv ->
            if kv.Value.Type = chType && isTeamChannel config kv.Value then
                Some (kv.Value.Name, kv.Value)
            else
                None
        )
        |> Map.ofSeq

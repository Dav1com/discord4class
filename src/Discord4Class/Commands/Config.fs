namespace Discord4Class.Commands

open FSharp.Reflection
open DSharpPlus
open DSharpPlus.EventArgs
open Discord4Class.Helpers.Messages
open Discord4Class.Helpers.Permission
open Discord4Class.Config.Types
open Discord4Class.Repositories.GuildConfiguration

module Config =

    [<Literal>]
    let RequiredPerms = Permissions.Administrator

    type private FiledType =
        | TextChannel
        | Role
        | VoiceChannel

    exception CommandConfigInvalidFieldException
    let private getFieldIndex objType name =
        FSharpType.GetRecordFields(objType)
        |> Array.tryFindIndex (fun attr -> attr.Name = name)
        |> function
        | Some i -> i
        | None -> raise CommandConfigInvalidFieldException

    let private configNames =
        [|
            ("teachers-text", (
                getFieldIndex typeof<GC> "TeachersText",
                getFieldIndex typeof<GuildConfig> "TeachersText",
                TextChannel ))
            ("class-voice", (
                getFieldIndex typeof<GC> "ClassVoice",
                getFieldIndex typeof<GuildConfig> "ClassVoice",
                VoiceChannel ))
            ("teacher-role", (
                getFieldIndex typeof<GC> "TeacherRole",
                getFieldIndex typeof<GuildConfig> "ClassVoice",
                Role ))
        |] |> Map.ofArray

    let private gcIdIndex = getFieldIndex typeof<GC> "_id"

    let private getValue fieldType value (e : MessageCreateEventArgs) =
        match fieldType with
        | TextChannel ->
            match List.ofSeq e.MentionedChannels with
            | ch::_ when ch.Type = ChannelType.Text ->
                Some ch.Id
            | _ -> None
        | Role ->
            match List.ofSeq e.MentionedRoles with
            | rl::_ -> Some rl.Id
            | _ -> None
        | VoiceChannel ->
            e.Guild.Channels.Values
            |> Seq.tryFind (fun c -> c.Name = value)
            |> function
            | Some ch when ch.Type = ChannelType.Voice ->
                Some ch.Id
            | _ -> None

    let private update app guild client name gcIndex configIndex fieldType value (e : MessageCreateEventArgs) =
        match value with
        | Some "unset" ->
            match FSharpValue.GetRecordFields(guild).[configIndex] :?> uint64 option with
            | Some _ ->
                let filter = GC.Filter.And [
                    GC.Filter.Eq((fun gc -> gc._id), e.Guild.Id)
                ]
                GC.Update.Set((fun gc -> FSharpValue.GetRecordFields(gc).[gcIndex] :?> uint64 option), None)
                |> GC.UpdateOne app.Db filter
                |> Async.RunSynchronously |> ignore
                exchangeReactions e.Message client app.Emojis.Doing app.Emojis.Yes
            | None ->
                exchangeReactions e.Message client app.Emojis.Doing app.Emojis.No
                guild.Lang.ConfigUnsetNull
                |> sendMessage e.Channel |> ignore
        | Some v ->
            getValue fieldType v e
            |> function
            | Some v ->
                let hold = FSharpValue.GetRecordFields(GC.Base)
                hold.[gcIndex] <- (Some v) :> obj
                hold.[gcIdIndex] <- e.Guild.Id :> obj
                FSharpValue.MakeRecord(typeof<GC>, hold) :?> GC
                |> GC.InsertUpdate app.Db guild.IsConfigOnDb
                |> Async.RunSynchronously
                exchangeReactions e.Message client app.Emojis.Doing app.Emojis.Yes
            | _ ->
                exchangeReactions e.Message client app.Emojis.Doing app.Emojis.No
                guild.Lang.ConfigInvalidNewValue name
                |> sendMessage e.Channel |> ignore
        | None ->
            match fieldType with
            | TextChannel | VoiceChannel ->
                match FSharpValue.GetRecordFields(guild).[configIndex] :?> uint64 option with
                | Some i ->
                    match e.Guild.GetChannel i with
                    | null -> guild.Lang.DeletedChannel
                    | channel -> channel.Mention
                    |> Some
                | None -> None
            | Role ->
                match FSharpValue.GetRecordFields(guild).[configIndex] :?> uint64 option with
                | Some i ->
                    match e.Guild.GetRole i with
                    | null -> guild.Lang.DeletedRole
                    | role -> role.Mention
                    |> Some
                | None -> None
            |> function
            | Some av -> guild.Lang.ConfigActualValue av
            | None -> guild.Lang.ConfigActualValueNull name
            |> sendMessage e.Channel |> ignore
            removeReaction e.Message client app.Emojis.Doing

    let exec app guild client (args : string) (e : MessageCreateEventArgs) = async {
        if checkPermissions e RequiredPerms then
            let phrased =
                args.Split(" ")
                |> Array.filter ((<>) "")
                |> List.ofArray
            match phrased with
            | name::tail ->
                addReaction e.Message client app.Emojis.Doing
                let value = List.tryHead tail
                configNames
                |> Map.tryFind name
                |> function
                | Some (gcIndex, configIndex, valueType) ->
                    update app guild client name gcIndex configIndex valueType value e
                | None ->
                    exchangeReactions e.Message client app.Emojis.Doing app.Emojis.No
                    guild.Lang.ConfigInvalidName
                    |> sendMessage e.Channel |> ignore
            | [] ->
                exchangeReactions e.Message client app.Emojis.Doing app.Emojis.No
                guild.Lang.ConfigMissingName guild.CommandPrefix
                |> sendMessage e.Channel |> ignore
    }

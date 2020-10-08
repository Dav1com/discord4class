namespace Discord4Class.Commands

open System.Threading.Tasks
open DSharpPlus
open DSharpPlus.Entities
open DSharpPlus.EventArgs
open Discord4Class.Helpers.Permission
open Discord4Class.Config.Types
open Discord4Class.Repositories.GuildConfiguration

module Config =

    [<Literal>]
    let RequiredPerms = Permissions.Administrator

    type private ValueType =
        | Channel of uint64 option
        | Role of uint64 option

    type private OperationResult =
        | Success
        | UnsetSuccess
        | UnsetNull
        | ShowValue of string * ValueType
        | InvalidNewValue of string

    let private configName args =
        match args with
        | "" -> None
        | s -> Some <| s.Split(" ").[0].ToLower()

    let private updateTeachersText config (args : string) (e : MessageCreateEventArgs) = async { return
        match e.Message.MentionedChannels |> Array.ofSeq with
        | _ when args.Contains "unset" ->
            match config.Guild.TeachersText with
            | Some _ ->
                let filter = GC.Filter.And [ GC.Filter.Eq ((fun gc -> gc._id), e.Guild.Id) ]
                GC.Update.Set((fun gc -> gc.TeachersText), None)
                |> GC.UpdateOne config.App.DbDatabase filter
                |> Async.RunSynchronously |> ignore
                UnsetSuccess
            | None -> UnsetNull
        | [||] -> ShowValue ("teachers-text", Channel config.Guild.TeachersText )
        | chs ->
            { GC.Base with
                _id = e.Guild.Id
                TeachersText = Some chs.[0].Id }
            |> GC.InsertUpdate config.App.DbDatabase config.Guild.IsConfigOnDb
            |> Async.RunSynchronously
            Success
    }

    let private updateClassVoice config (args : string) (e : MessageCreateEventArgs) = async { return
        match args.Split " " with
        | a when a.Length < 2 -> ShowValue ("class-voice", Channel config.Guild.ClassVoice)
        | a when a.[1] = "unset" ->
            match config.Guild.ClassVoice with
            | Some _ ->
                let filter = GC.Filter.And [ GC.Filter.Eq ((fun gc -> gc._id), e.Guild.Id) ]
                GC.Update.Set((fun gc -> gc.ClassVoice), None)
                |> GC.UpdateOne config.App.DbDatabase filter
                |> Async.RunSynchronously |> ignore
                UnsetSuccess
            | None -> UnsetNull
        | chs ->
            e.Guild.Channels
            |> Array.ofSeq
            |> Array.tryFind (fun ch -> ch.Name = chs.[1] && ch.Type = ChannelType.Voice)
            |> function
                | Some ch ->
                    { GC.Base with
                        _id = e.Guild.Id
                        ClassVoice = Some ch.Id }
                    |> GC.InsertUpdate config.App.DbDatabase config.Guild.IsConfigOnDb
                    |> Async.RunSynchronously
                    Success
                | None ->
                    InvalidNewValue "class-voice"
    }

    let private updateTeacherRole config (args : string) (e : MessageCreateEventArgs) = async { return
        match e.Message.MentionedRoles |> Array.ofSeq with
        | _ when args.Trim().EndsWith "unset" ->
            match config.Guild.TeacherRole with
            | Some _ ->
                let filter = GC.Filter.And [
                    GC.Filter.Eq((fun gc -> gc._id), e.Guild.Id)
                ]
                GC.Update.Set((fun gc -> gc.TeacherRole), None)
                |> GC.UpdateOne config.App.DbDatabase filter
                |> Async.RunSynchronously |> ignore
                UnsetSuccess
            | None -> UnsetNull
        | [||] -> ShowValue ("teacher-role", Role config.Guild.TeacherRole)
        | roles ->
            { GC.Base with
                _id = e.Guild.Id
                TeacherRole = Some roles.[0].Id }
            |> GC.InsertUpdate config.App.DbDatabase config.Guild.IsConfigOnDb
            |> Async.RunSynchronously
            Success
    }

    let private configNames =
        [|
            ("teachers-text", updateTeachersText)
            ("class-voice", updateClassVoice)
            ("teacher-role", updateTeacherRole)
        |] |> Map.ofArray

    let exec config args (e : MessageCreateEventArgs) = async {
        if checkPermissions e RequiredPerms then
            match configName args with
            | Some s ->
                configNames.TryFind s
                |> function
                    | Some f ->
                        f config args e
                        |> Async.RunSynchronously
                        |> function
                            | Success ->
                                config.Guild.Lang.ConfigSuccess
                            | InvalidNewValue s ->
                                config.Guild.Lang.ConfigInvalidNewValue s
                            | ShowValue (s, v) ->
                                match v with
                                | Channel o ->
                                    match o with
                                    | Some i -> Some (e.Guild.GetChannel(i).Mention)
                                    | None -> None
                                | Role o ->
                                    match o with
                                    | Some i -> Some (e.Guild.GetRole(i).Mention)
                                    | None -> None
                                |> function
                                    | Some s2 -> config.Guild.Lang.ConfigActualValue s s2
                                    | None -> config.Guild.Lang.ConfigActualValueNull s
                            | UnsetNull ->
                                config.Guild.Lang.ConfigUnsetNull
                            | UnsetSuccess ->
                                config.Guild.Lang.ConfigUnsetSuccess
                        |> fun s -> e.Channel.SendMessageAsync(s)
                        |> Async.AwaitTask |> Async.Ignore
                    | None ->
                        async.Zero()
            | None ->
                config.Guild.Lang.ConfigMissingName config.Guild.CommandPrefix
                |> fun s -> e.Channel.SendMessageAsync(s)
                |> Async.AwaitTask
                |> Async.Ignore
            |> Async.RunSynchronously
    }

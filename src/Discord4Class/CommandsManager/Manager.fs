namespace Discord4Class.CommandsManager

open DSharpPlus
open DSharpPlus.Entities
open DSharpPlus.EventArgs
open Discord4Class.Constants
open Discord4Class.Helpers.Railway
open Discord4Class.Helpers.Permission
open Discord4Class.Helpers.Messages
open Discord4Class.Helpers.Time
open Discord4Class.Repositories.GuildData
open Discord4Class.Repositories.RateLimits
open Discord4Class.Config.Types
open Discord4Class.CommandsManager

[<AutoOpen>]
module Manager =

    [<Literal>]
    let private maxTries = 5

    let private splitArguments maxArgs str =
        let rec loop res (str: string) count =
            match str.IndexOf " " with
            | -1 ->
                match str.Trim() with
                | "" -> res
                | str -> List.append res [str]
            | i when maxArgs = -1 || count < maxArgs ->
                loop (List.append res [str.[..i-1]])
                    (str.[i+1..].TrimStart()) (count + 1)
            | _ ->
                match str.Trim() with
                | "" -> res
                | str -> List.append res [str]
        loop [] str 1

    type CommandResult =
        { Command: BotCommand
          Args: string }

    and BotCommands =
        | BotCommands of seq<BotCommand>

        member this.GetCommandRaw (str, ?predicate) =
            let predicate = Option.defaultValue (fun _  _-> true) predicate
            let rec loop botCommands (args: string) cmd name  =
                let (BotCommands cmds) = botCommands
                match args.IndexOf " " with
                | -1 -> (args, "")
                | i -> (args.[..i-1].ToLower(), args.[i..].TrimStart())
                ||> fun s nextArgs ->
                    cmds
                    |> Seq.tryFind (fun c ->
                        List.contains s c.Names
                        && predicate c nextArgs )
                    |> function
                        | Some c ->
                            if String.length name = 0 then c.Names.[0]
                            else name + "|" + c.Names.[0]
                            |> loop c.SubCommands nextArgs (Some c)
                        | None ->
                            match cmd with
                            | Some c ->
                                ({ Command = c
                                   Args = args },
                                 name + c.Permissions.ToTag())
                                |> Ok
                            | None -> Error CommandNotFound
            loop this str None ""

        member this.GetCommandNoChecks guildConf owners memb author channel str =
            this.GetCommandRaw(str,
                fun c nextArgs ->
                    c.CheckPerms guildConf.TeacherRole owners memb author channel
                    && c.When guildConf nextArgs)

        member this.GetCommand app guildConf channel memb (guild: DiscordGuild) author owners msg =
            match memb with
            | null -> Error (MemberLeavedGuild author)
            | memb ->
                this.GetCommandNoChecks guildConf owners memb author channel msg
            >>= fun (c, name) ->
                match c.Command.CheckRateLimit app guild.Id author.Id name with
                | 0UL -> Ok (c, name)
                | waitUntil -> Error (RateLimitHit waitUntil)
            >>= fun (c, name) ->
                c.Command.CheckRequiredSettings guildConf guild
                >>= fun _ -> Ok (c, name)
            >>= fun (c, name) ->
                c.Command.CheckIsLargeGuild guild
                >>= fun _ -> Ok (c, name)
            >>= fun (c,_) -> Ok c

        member this.RunCommandAsync app guild (client: DiscordClient) memb (e: MessageCreateEventArgs) msg =
            this.GetCommand app guild e.Channel memb e.Guild e.Author client.CurrentApplication.Owners msg
            >>= fun cmdr ->
                if cmdr.Command.IsHeavy && guild.DoingHeavyTask then
                    Error DoingHeavyTask
                else Ok cmdr
            >>= switch (fun cmdr ->
                cmdr.Command.Function app guild client
                    (splitArguments cmdr.Command.MaxArgs cmdr.Args) memb e
                |> fun cmdFunc ->
                    if cmdr.Command.IsHeavy then async {
                        try
                            GD.Update.Set((fun gd -> gd.DoingHeavyTask), true)
                            |> GD.InsertOrUpdate app.Db guild.IsConfigOnDb
                                { GD.Base with
                                    Id = e.Guild.Id
                                    DoingHeavyTask = true }
                            |> Async.RunSynchronously
                            do! cmdFunc
                        finally
                            let rec loop tries =
                                try
                                    GD.Update.Set((fun gd -> gd.DoingHeavyTask), false)
                                    |> GD.Operation.UpdateOneById app.Db e.Guild.Id
                                    |> Async.RunSynchronously |> ignore
                                with
                                | _ ->
                                    if maxTries < tries then loop (tries + 1)
                                    else
                                        guild.Lang.ErrorEndHeavyTaskTriesExceeded
                                            app.SupportServer
                                        |> sendMessage e.Channel |> ignore
                            loop 1 }
                    else cmdFunc
                |> fun cmdFunc ->
                    async.TryWith(cmdFunc, Exception.cmdErrorUnknown guild.Lang e) )

        member this.SortRecursive () =
            let rec loop (BotCommands cmds) =
                Seq.sortBy (fun cmd -> String.concat "" cmd.Names) cmds
                |> Seq.map (fun cmd ->
                    {cmd with
                        SubCommands = BotCommands (loop cmd.SubCommands) })
            BotCommands (loop this)

        member private this.CollectCommandNames separator =
            let (BotCommands cmds) = this
            (Seq.fold (fun acc cmd -> acc + separator + cmd.Names.[0]) "" cmds).[1..]

        member this.ComandNamesByPermission =
            let (BotCommands commands) = this.SortRecursive()
            Seq.filter (fun cmd -> cmd.DisplayOnCommandList) commands
            |> Seq.groupBy (fun cmd -> cmd.Permissions)
            |> Seq.map (fun (perm, cmds) ->
                (perm,
                    [ for cmd in cmds do
                        for name in cmd.Names do
                            $"`%s{name}"+
                            match cmd.SubCommands.CollectCommandNames "|" with
                            | "" -> "`"
                            | str -> $" <%s{str}>`" ] ))

    and BotCommand =
        { Names: string list
          Usage: string
          Description: (GuildConfig -> string)
          DisplayOnCommandList: bool
          Permissions: CommandPermission
          MaxArgs: int
          RequiredSettings: Settings
          IsHeavy: bool
          RequiresSmallGuild: bool
          RateLimits: CommandRateLimit list
          RateLimitType: RateLimitType
          When: (GuildConfig -> string -> bool )
          Function: (AppConfig -> GuildConfig -> DiscordClient -> string list -> DiscordMember -> MessageCreateEventArgs -> Async<unit>)
          SubCommands: BotCommands }

        member this.CheckPerms (teacherRole: uint64 option) owners (memb: DiscordMember) (author: DiscordUser) (channel: DiscordChannel) =
            memb.IsOwner
            || match this.Permissions with
                | GuildPrivileged -> checkPermissions memb channel GuildPrivilegedPerm
                | BotOwners -> Seq.contains author owners
                | Teacher ->
                    teacherRole.IsSome && checkIsTeacher teacherRole.Value memb
                | NoPerms -> true

        member this.CheckRequiredSettings (guildConf: GuildConfig) (guild: DiscordGuild) =
            if this.RequiredSettings &&& Settings.TeacherRole = Settings.None then
                Ok ()
            elif guildConf.TeacherRole.IsNone then
                Error (MissingConfiguration "teacher-role")
            elif isNull (guild.GetRole guildConf.TeacherRole.Value) then
                Error (DeletedRole "teacher-role")
            else Ok ()
            >>= fun _ ->
                if this.RequiredSettings &&& Settings.TeachersText = Settings.None then
                    Ok ()
                elif guildConf.TeachersText.IsNone then
                    Error (MissingConfiguration "teachers-text")
                elif isNull (guild.GetChannel guildConf.TeachersText.Value) then
                    Error (DeletedChannel "teachers-text")
                else Ok ()
            >>= fun _ ->
                if this.RequiredSettings &&& Settings.ClassVoice = Settings.None then
                    Ok ()
                elif guildConf.ClassVoice.IsNone then
                    Error (MissingConfiguration "class-voice")
                elif isNull (guild.GetChannel guildConf.ClassVoice.Value) then
                    Error (DeletedChannel "class-voice")
                else Ok ()

        member this.CheckIsLargeGuild (guild: DiscordGuild) =
            if this.RequiresSmallGuild && guild.IsLarge then Error (CommandNotSupportedOnLargeGuild LargeGuildThreshold)
            else Ok ()

        member this.CheckRateLimit app guildId userId commandName =
            let id =
                match this.RateLimitType with
                | GuildRateLimit -> guildId
                | UserRateLimit -> userId
            match RT.GetRateLimit app.Db id commandName with
            | Ok rts ->
                CommandRateLimit.PushMissingRateLimits app.Db id commandName
                    this.RateLimits rts

                let (RateLimitWaitUntil (waitUntil, resets, adds)) =
                    CommandRateLimit.GetWaitUntil rts this.RateLimits
                RT.ResetAndAddToCounters app.Db id commandName resets adds
                |> Async.Ignore |> Async.Start
                waitUntil
            | Error IdNotFound ->
                CommandRateLimit.Insert app.Db id commandName this.RateLimits
                |> Async.Start
                0UL
            | Error RateLimitNotFound ->
                CommandRateLimit.UpdateAddName app.Db id commandName this.RateLimits
                |> Async.Start
                0UL

    let BaseCommand =
        { Names = []
          Usage = ""
          Description = fun _ -> ""
          DisplayOnCommandList = true
          Permissions = NoPerms
          MaxArgs = 0
          RequiredSettings = Settings.None
          IsHeavy = false
          RequiresSmallGuild = false
          RateLimits = []
          RateLimitType = GuildRateLimit
          When = fun _ _ -> true
          Function = fun _ _ _ _ _ _ -> async.Zero()
          SubCommands = BotCommands [] }

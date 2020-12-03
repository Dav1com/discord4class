namespace Discord4Class.CommandsManager

open DSharpPlus.Entities
open Discord4Class.Helpers.Time
open Discord4Class.Config.Types
open Discord4Class.Repositories.RateLimits

[<AutoOpen>]
module Types =

    type CommandPermission =
        | Teacher
        | GuildPrivileged
        | BotOwners
        | NoPerms

        // tag at the end of the command name in the rate limits, so the rate
        // limits for permission-overloaded commands works
        member this.ToTag () =
            match this with
            | NoPerms -> ""
            | Teacher -> "/T"
            | BotOwners -> "/O"
            | GuildPrivileged -> "/G"

    type Settings =
        | None = 0x0
        | All = 0x4
        | TeacherRole = 0x1
        | ClassVoice = 0x2
        | TeachersText = 0x4

    type CommandError =
        | CommandNotFound
        | MemberLeavedGuild of DiscordUser
        | MissingConfiguration of configName: string
        | RateLimitHit of waitUntil: uint64
        | DeletedChannel of configName: string
        | DeletedRole of configName: string
        | CommandNotSupportedOnLargeGuild of limit: int
        | DoingHeavyTask

        member this.ToMessage guild =
            match this with
            | DeletedChannel name ->
                guild.Lang.ErrorChannelDeleted name guild.CommandPrefix name
                |> Some
            | DeletedRole name ->
                guild.Lang.ErrorRoleDeleted name guild.CommandPrefix name
                |> Some
            | CommandNotSupportedOnLargeGuild limit ->
                guild.Lang.ErrorCommandNotSupportedOnLargeGuild limit
                |> Some
            | MemberLeavedGuild _ | RateLimitHit _ | DoingHeavyTask
            | MissingConfiguration _ | CommandNotFound -> None

    type RateLimitWaitUntil =
        | RateLimitWaitUntil of WaitUntil: uint64 * Resets: (int * uint64) list * Adds: int list

    type CommandRateLimit =
        { Allowed: byte
          Interval: uint64 } // in seconds

        static member PushMissingRateLimits db guildId commandName (crts: CommandRateLimit list) rts =
            if Seq.length rts < crts.Length then
                seq {
                    for i in [Seq.length rts..crts.Length - 1] do
                        { Count = 1uy
                          Reset =
                            nextInterval crts.[i].Interval }}
                |> fun sq ->
                    RT.Update.PushEach((fun rtd -> rtd.Values.[commandName]), sq)
                |> RT.Operation.UpdateOneById db guildId
                |> Async.Ignore |> Async.Start

        static member GetWaitUntil rts crts =
            Seq.indexed rts
            |> Seq.fold2 (fun (waitUntil, resets, adds) crt (i, rt) ->
                if rt.Count < crt.Allowed then
                    (waitUntil, resets, i :: adds)
                else
                    let nextReset = nextInterval crt.Interval
                    if rt.Reset < nextReset then
                        (waitUntil, (i, nextReset) :: resets, adds)
                    else
                        // the ratelimits are ordered by interval, so
                        // this always overwrite with the highest wait time
                        (rt.Reset, resets, adds) )
                (0UL, [], []) crts
            |> RateLimitWaitUntil

        static member Insert db id name crts =
            RT.Operation.InsertOne db
                { Id = id
                  Values =
                    Map.empty.Add(name,
                        [| for crt in crts do
                            { Count = 1uy;
                              Reset = nextInterval crt.Interval }|]) }

        static member UpdateAddName db id name crts =
            RT.Update.Set((fun rtd -> rtd.Values.Item name),
                seq {
                    for crt in crts do
                        { Count = 1uy
                          Reset = nextInterval crt.Interval } } )
            |> RT.Operation.UpdateOneById db id
            |> Async.Ignore

    type RateLimitType =
        | GuildRateLimit
        | UserRateLimit

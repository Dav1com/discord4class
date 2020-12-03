namespace Discord4Class.Commands

open DSharpPlus.EventArgs
open Discord4Class.Helpers.Messages
open Discord4Class.Config.Types
open Discord4Class.Commands.TeamsInternals
open Discord4Class.CommandsManager

module TeamsManual =

    type private ManualSwitches =
        | Default = 0x0
        | IgnoreErrors = 0x1

    [<Literal>]
    let private defaultSwitches = ManualSwitches.Default

    let rec private overwriteSwitches switches args pos =
        match args with
        | "ignore-errors" :: tail ->
            overwriteSwitches (switches ||| ManualSwitches.IgnoreErrors)
                tail (pos + 1)
        | args -> (switches, args, pos)

    let main app guild client args memb (e: MessageCreateEventArgs) = async {
        let (switches, processedArgs, pos) =
            overwriteSwitches defaultSwitches args 1
        match getTeamRoles guild e.Guild with
        | roles when roles.Count = 0 ->
            guild.Lang.TeamsManualNoTeams guild.CommandPrefix
            |> sendMessage e.Channel |> ignore
        | roles ->
            addReaction e.Message app.Emojis.Doing
            getTeamsMembers guild e.Guild
                (switches &&& ManualSwitches.IgnoreErrors = ManualSwitches.IgnoreErrors)
                false
            |> function
                | Ok teams ->
                    createChannels guild e.Guild.Roles.[guild.TeacherRole.Value]
                        e.Guild teams roles
                    changeReaction e.Message app.Emojis.Yes
                | Error (UserHasMultipleTeams memb) -> ()
    }

    let command =
        { BaseCommand with
            Names = [ "manual" ]
            Permissions = Teacher
            Description = fun gc ->
                gc.Lang.TeamsManualDescription
                    (if gc.Lang.TeamsNumberIsRight = "1" then
                        $"%s{gc.Lang.Team} %s{gc.Lang.NumberPlaceholder}"
                     else $"%s{gc.Lang.NumberPlaceholder} %s{gc.Lang.Team}")
                    (gc.Lang.TeamsTemplate 1) 1
                    gc.CommandPrefix gc.Lang.TeamsManualUsage
                    gc.Lang.TeamsManualOptions
            MaxArgs = -1
            RequiredSettings = Settings.TeacherRole
            IsHeavy = true
            RequiresSmallGuild = true
            RateLimits = [
                { Allowed = 3uy
                  Interval = 30UL }
                { Allowed = 30uy
                  Interval = 600UL }]
            Function = main }

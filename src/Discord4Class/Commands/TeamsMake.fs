namespace Discord4Class.Commands

open System
open DSharpPlus.EventArgs
open Discord4Class.Constants
open Discord4Class.Helpers.Messages
open Discord4Class.Helpers.Roles
open Discord4Class.Helpers.String
open Discord4Class.Config.Types
open Discord4Class.Commands.TeamsInternals
open Discord4Class.CommandsManager

module TeamsMake =

    type private TeamsSwitches =
        | Default = 0x0
        | OnlineStudentsOnly = 0x1

    [<Literal>]
    let private defaultSwitches = TeamsSwitches.Default

    let rec private overwriteSwitches switches args pos =
        match args with
        | "online" :: tail ->
            overwriteSwitches (switches ||| TeamsSwitches.OnlineStudentsOnly)
                tail (pos + 1)
        | "all" :: tail ->
            overwriteSwitches (switches &&& ~~~TeamsSwitches.OnlineStudentsOnly) tail
                (pos + 1)
        | args -> (switches, args, pos)

    let main app guild client args memb (e: MessageCreateEventArgs) = async {
        let (switches, processedArgs, pos) =
            overwriteSwitches defaultSwitches args 1
        let classVoice =
            Option.defaultValue 0UL guild.ClassVoice
            |> e.Guild.GetChannel
        match processedArgs with
        | _ when switches &&& TeamsSwitches.OnlineStudentsOnly
                    = TeamsSwitches.OnlineStudentsOnly
                 && guild.ClassVoice.IsNone ->
            guild.Lang.ErrorChannelNull "class-voice" guild.CommandPrefix "class-voice"
            |> sendMessage e.Channel |> ignore
        | _ when switches &&& TeamsSwitches.OnlineStudentsOnly
                    = TeamsSwitches.OnlineStudentsOnly
                 && isNull classVoice ->
            guild.Lang.ErrorChannelDeleted "class-voice" guild.CommandPrefix "class-voice"
            |> sendMessage e.Channel |> ignore
        | IntParse num :: _ when num > 0 ->
            if existsTeams guild e.Guild then
                addReaction e.Message app.Emojis.No
                guild.Lang.TeamsAlreadyCreated guild.CommandPrefix
                |> sendMessage e.Channel |> ignore
            else
                let teacherRole = e.Guild.GetRole guild.TeacherRole.Value
                let teamsCount = int num
                let students =
                    getStudents
                        (switches &&& TeamsSwitches.OnlineStudentsOnly = TeamsSwitches.OnlineStudentsOnly)
                        teacherRole classVoice e.Guild
                let guildRolesCount = getNonManagedRoles(e.Guild).Length
                if teamsCount * TeamsChannelsFactor > GuildMaxChannels - e.Guild.Channels.Count then
                    addReaction e.Message app.Emojis.No
                    guild.Lang.TeamsMakeChannelsLimitHit
                        ( float(GuildMaxChannels - e.Guild.Channels.Count) / float(TeamsChannelsFactor)
                          |> Math.Ceiling
                          |> int )
                    |> sendMessage e.Channel |> ignore
                elif teamsCount > GuildMaxRoles - guildRolesCount then
                    addReaction e.Message app.Emojis.No
                    guild.Lang.TeamsMakeRolesLimitHit
                        (GuildMaxRoles - guildRolesCount)
                    |> sendMessage e.Channel |> ignore
                else
                    addReaction e.Message app.Emojis.Doing
                    let teams = makeTeams guild students teamsCount
                    createRoles e.Guild teams
                    |> createChannels guild teacherRole e.Guild teams
                    changeReaction e.Message app.Emojis.Yes
        | IntParse _num :: _ -> // _num <= 0
            guild.Lang.TeamsMakeOutOfRange
            |> sendMessage e.Channel |> ignore
        | [] ->
            guild.Lang.TeamsMakeMissingCount guild.CommandPrefix
            |> sendMessage e.Channel |> ignore
        | _ :: _ ->
            guild.Lang.TeamsMakeInvalidSwitchOrSize args.[pos]
            |> sendMessage e.Channel |> ignore
    }

    let command =
        { BaseCommand with
            Names = [ "make" ]
            Description = fun gc ->
                gc.Lang.TeamsMakeDescription gc.CommandPrefix gc.Lang.TeamsMakeUsage
                    gc.Lang.TeamsMakeOptions
            Permissions = Teacher
            MaxArgs = -1
            RequiredSettings = Settings.TeacherRole
            IsHeavy = true
            RequiresSmallGuild = true
            RateLimits = [
                { Allowed = 3uy
                  Interval = 30UL }
                { Allowed = 30uy
                  Interval = 600UL } ]
            Function = main }

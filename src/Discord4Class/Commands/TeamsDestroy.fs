namespace Discord4Class.Commands

open DSharpPlus
open DSharpPlus.EventArgs
open Discord4Class.Constants
open Discord4Class.Helpers.Messages
open Discord4Class.Config.Types
open Discord4Class.Commands.TeamsInternals
open Discord4Class.CommandsManager

module TeamsDestroy =

    let main app guild client args memb (e: MessageCreateEventArgs) = async {
        addReaction e.Message app.Emojis.Doing
        e.Guild.GetChannelsAsync()
        |> Async.AwaitTask |> Async.RunSynchronously
        |> Seq.filter (isTeamChannel guild)
        |> function
            // just in case the limit is increased in the future
            | channels when Seq.length channels > LargeGuildThreshold * TeamsChannelsFactor ->
                guild.Lang.TeamsDestroyTooMuchChannels
                |> sendMessage e.Channel |> ignore
            | channels ->
                channels
                |> Seq.sortBy (fun ch ->
                    match ch.Type with
                    | ChannelType.Category -> 1
                    | _ -> 0 )
                |> Seq.map (fun ch -> async {
                    try
                        ch.DeleteAsync()
                        |> Async.AwaitTask |> Async.RunSynchronously
                        return Some ()
                    with
                    | _ -> return None } )
                |> Async.Sequential |> Async.RunSynchronously |> ignore
        getTeamRoles guild e.Guild
        |> function
            // again, this is just in case
            | roles when roles.Count > LargeGuildThreshold ->
                guild.Lang.TeamsDestroyTooMuchRoles
                |> sendMessage e.Channel |> ignore
            | roles -> deleteRoles roles
        changeReaction e.Message app.Emojis.Yes
    }

    let command =
        { BaseCommand with
            Names = [ "destroy" ]
            Description = fun gc ->
                gc.Lang.TeamsDestroyDescription
                    gc.CommandPrefix gc.Lang.TeamsDestroyUsage
            Permissions = Teacher
            IsHeavy = true
            RateLimits = [
                { Allowed = 3uy
                  Interval = 30UL }
                { Allowed = 30uy
                  Interval = 600UL } ]
            Function = main }

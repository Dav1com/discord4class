namespace Discord4Class.Commands

open System
open DSharpPlus
open DSharpPlus.Entities
open DSharpPlus.EventArgs
open DSharpPlus.Interactivity
open DSharpPlus.Interactivity.Extensions
open Discord4Class.Helpers.Messages
open Discord4Class.Config.Types
open Discord4Class.Repositories.GuildData
open Discord4Class.Repositories.MutedChannels
open Discord4Class.Repositories.Questions
open Discord4Class.CommandsManager

module Destroy =

    [<Literal>]
    let private confirmTimeout = 30.0 //seconds

    let private predicate guild (e: DiscordMessage) = Func<DiscordMessage, bool>(fun e2 ->
        e.ChannelId = e2.ChannelId && e.Author.Id = e2.Author.Id &&
            ( e2.Content.ToLower() = guild.Lang.Yes ||
              e2.Content.ToLower() = guild.Lang.No ) )

    let private afterConfirmation app guild client (confirmMsg: DiscordMessage) (e: MessageCreateEventArgs) (result: InteractivityResult<DiscordMessage>) =
        if result.TimedOut then
            guild.Lang.ConfirmationTimeoutMessage
            |> modifyMessage confirmMsg |> ignore
        elif result.Result.Content.ToLower() = guild.Lang.No then
            guild.Lang.ConfirmationCancellation
            |> sendMessage e.Channel |> ignore
        else
            [ addReactionAsync e.Message app.Emojis.Doing
              deleteMessageAsync confirmMsg
              deleteMessageAsync result.Result ]
            |> Async.Parallel |> Async.RunSynchronously |> ignore
            [ GD.Operation.DeleteOneById app.Db e.Guild.Id
              MC.Operation.DeleteOneById app.Db e.Guild.Id
              Qs.Operation.DeleteOneById app.Db e.Guild.Id ]
            |> Async.Parallel |> Async.RunSynchronously |> ignore

            changeReaction e.Message app.Emojis.Yes

    let main app guild (client: DiscordClient) _ memb (e: MessageCreateEventArgs) = async {
        let confirmMsg =
            guild.Lang.DestroyConfirmationMsg
                guild.Lang.Yes
                guild.Lang.No
            |> sendMessage e.Channel

        let inter = client.GetInteractivity()
        inter.WaitForMessageAsync(
            predicate guild e.Message, Nullable (TimeSpan.FromSeconds confirmTimeout))
        |> Async.AwaitTask |> Async.RunSynchronously
        |> afterConfirmation app guild client confirmMsg e
        |> ignore
    }

    let command =
        { BaseCommand with
            Names = [ "destroy" ]
            Description = fun gc ->
                gc.Lang.DestroyDescription gc.CommandPrefix gc.Lang.DestroyUsage
            Permissions = GuildPrivileged
            RateLimits = [
                { Allowed = 1uy
                  Interval = 30UL } ]
            Function = main }

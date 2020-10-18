namespace Discord4Class.Commands

open System
open System.Threading.Tasks
open DSharpPlus
open DSharpPlus.Entities
open DSharpPlus.EventArgs
open DSharpPlus.Interactivity
open Discord4Class.Helpers.Messages
open Discord4Class.Helpers.Permission
open Discord4Class.Config.Types
open Discord4Class.Repositories.GuildConfiguration
open Discord4Class.Repositories.MutedChannels

module Destroy =

    [<Literal>]
    let ConfirmTimeout = 30.0 //seconds
    [<Literal>]
    let RequiredPerms = Permissions.Administrator

    let private predicate guild (e : DiscordMessage) = Func<DiscordMessage, bool>(fun e2 ->
        e.ChannelId = e2.ChannelId && e.Author.Id = e2.Author.Id && (
            e2.Content.ToLower() = guild.Lang.ConfirmationYesResponse.ToLower() ||
            e2.Content.ToLower() = guild.Lang.ConfirmationNoResponse.ToLower()
        )
    )

    let private afterConfirmation app guild client (confirmMsg : DiscordMessage) (e : MessageCreateEventArgs) = Action<Task<InteractivityResult<DiscordMessage>>>(fun r ->
        let result = r.Result
        if result.TimedOut then
            guild.Lang.ConfirmationTimeoutMessage
            |> modifyMessage confirmMsg |> ignore
        else
            if result.Result.Content.ToLower() = guild.Lang.ConfirmationNoResponse.ToLower() then
                guild.Lang.ConfirmationCancellation
                |> sendMessage e.Channel |> ignore
            else
                [
                    addReactionAsync e.Message client app.Emojis.Doing
                    deleteMessageAsync confirmMsg
                    deleteMessageAsync result.Result
                ] |> Async.Parallel |> Async.RunSynchronously |> ignore
                [
                    GC.Filter.And [GC.Filter.Eq((fun gc -> gc._id), e.Guild.Id)]
                    |> GC.DeleteOne app.Db
                    MC.Filter.And [MC.Filter.Eq((fun mc -> mc._id), e.Guild.Id)]
                    |> MC.DeleteOne app.Db
                ]
                |> Async.Parallel |> Async.RunSynchronously |> ignore

                exchangeReactions e.Message client app.Emojis.Doing app.Emojis.Yes

    )

    let exec app guild (client : DiscordClient) _ (e : MessageCreateEventArgs) = async {
        if checkPermissions e RequiredPerms then
            let confirmMsg =
                guild.Lang.DestroyConfirmationMsg
                    guild.Lang.ConfirmationYesResponse
                    guild.Lang.ConfirmationNoResponse
                |> sendMessage e.Channel

            let inter = client.GetInteractivity()
            inter.WaitForMessageAsync(
                    predicate guild e.Message, Nullable (TimeSpan.FromSeconds ConfirmTimeout))
                .ContinueWith( afterConfirmation app guild client confirmMsg e)
            |> ignore
    }

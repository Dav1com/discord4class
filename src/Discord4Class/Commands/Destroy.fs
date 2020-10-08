namespace Discord4Class.Commands

open System
open System.Threading.Tasks
open DSharpPlus
open DSharpPlus.Entities
open DSharpPlus.EventArgs
open DSharpPlus.Interactivity
open Discord4Class.Helpers.Permission
open Discord4Class.Config.Types
open Discord4Class.Repositories.GuildConfiguration

module Destroy =

    [<Literal>]
    let ConfirmTimeout = 30.0 //seconds
    [<Literal>]
    let RequiredPerms = Permissions.Administrator

    let private predicate config (e : DiscordMessage) = Func<DiscordMessage, bool>(fun e2 ->
        e.ChannelId = e2.ChannelId && e.Author.Id = e2.Author.Id && (
            e2.Content.ToLower() = config.Guild.Lang.ConfirmationYesResponse.ToLower() ||
            e2.Content.ToLower() = config.Guild.Lang.ConfirmationNoResponse.ToLower()
        )
    )

    let private afterConfirmation config (confirmMsg : DiscordMessage) (e : MessageCreateEventArgs) = Action<Task<MessageContext>>(fun r ->
        let result = r.Result
        if isNull result then
            confirmMsg.Content + "\n" + config.Guild.Lang.ConfirmationTimeoutMessage
            |> fun s -> confirmMsg.ModifyAsync(Optional s)
            |> Async.AwaitTask
            |> Async.RunSynchronously
            |> ignore
        else
            if result.Message.Content.ToLower() = config.Guild.Lang.ConfirmationNoResponse.ToLower() then
                config.Guild.Lang.ConfirmationCancellation
                |> fun s -> e.Channel.SendMessageAsync(s)
                |> Async.AwaitTask
                |> Async.RunSynchronously
                |> ignore
            else
                [
                    GC.Filter.And [GC.Filter.Eq((fun gc -> gc._id), e.Guild.Id)]
                    |> GC.DeleteOne config.App.DbDatabase
                ]
                |> Async.Parallel |> Async.RunSynchronously |> ignore

                config.Guild.Lang.DestroySuccess
                |> fun s -> e.Channel.SendMessageAsync(s)
                |> Async.AwaitTask
                |> Async.RunSynchronously
                |> ignore

    )

    let exec config _ (e : MessageCreateEventArgs) = async {
        if checkPermissions e RequiredPerms then
            let! confirmMsg =
                config.Guild.Lang.DestroyConfirmationMsg
                    config.Guild.Lang.ConfirmationYesResponse
                    config.Guild.Lang.ConfirmationNoResponse
                |> fun s -> e.Channel.SendMessageAsync(s)
                |> Async.AwaitTask

            let inter = (e.Client :?> DiscordClient).GetInteractivityModule()
            inter.WaitForMessageAsync(
                    predicate config e.Message, Nullable (TimeSpan.FromSeconds ConfirmTimeout))
                .ContinueWith( afterConfirmation config confirmMsg e)
            |> ignore
    }

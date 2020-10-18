namespace Discord4Class.Commands

open System
open System.Threading.Tasks
open DSharpPlus
open DSharpPlus.Entities
open DSharpPlus.EventArgs
open DSharpPlus.Interactivity
open Discord4Class.Helpers.Messages
open Discord4Class.Helpers.Permission
open Discord4Class.Repositories.GuildConfiguration
open Discord4Class.Config.Types

module Init =

    [<Literal>]
    let RequiredPerms = Permissions.Administrator

    let TeacherPerms =
        Permissions.CreateInstantInvite +
        Permissions.AttachFiles +
        Permissions.AddReactions +
        Permissions.ManageMessages +
        Permissions.ManageNicknames +
        Permissions.MentionEveryone +
        Permissions.MoveMembers +
        Permissions.MuteMembers +
        Permissions.ReadMessageHistory +
        Permissions.SendMessages

    let private messageCreated guild (e : DiscordMessage) = Func<DiscordMessage, bool> (fun e2 ->
        e.ChannelId = e2.ChannelId && e2.Author.Id = e.Author.Id && (
            e2.Content.ToLower() = guild.Lang.ConfirmationYesResponse.ToLower() ||
            e2.Content.ToLower() = guild.Lang.ConfirmationNoResponse.ToLower()
        )
    )

    let private afterConfirmation app guild client
        (confirmMsg : DiscordMessage) (e : MessageCreateEventArgs)
        = Action<Task<InteractivityResult<DiscordMessage>>>(fun r ->

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
                    deleteMessageAsync confirmMsg
                    deleteMessageAsync e.Message
                    addReactionAsync e.Message client app.Emojis.Doing
                ] |> Async.Parallel |> Async.RunSynchronously |> ignore

                let teacherRole =
                    e.Guild.CreateRoleAsync(
                        guild.Lang.TeachersRoleName,
                        Nullable TeacherPerms,
                        Nullable DiscordColor.Blue,
                        Nullable true,
                        Nullable true
                    )
                    |> Async.AwaitTask |> Async.RunSynchronously
                e.Guild.GetMemberAsync e.Author.Id
                |> Async.AwaitTask |> Async.RunSynchronously
                |> fun m -> m.GrantRoleAsync teacherRole
                |> Async.AwaitTask |> Async.RunSynchronously

                // category container
                let category =
                    e.Guild.CreateChannelAsync(guild.Lang.ClassCategoryName, ChannelType.Category)
                    |> Async.AwaitTask
                    |> Async.RunSynchronously
                let teachersText =
                    e.Guild.CreateChannelAsync(
                        guild.Lang.TeachersClassTextChannelName,
                        ChannelType.Text,
                        category,
                        overwrites = [
                            DiscordOverwriteBuilder()
                                .For(teacherRole)
                                .Allow(Permissions.All)
                            DiscordOverwriteBuilder()
                                .For(e.Guild.EveryoneRole)
                                .Deny(Permissions.All)
                        ]
                    )
                    |> Async.AwaitTask |> Async.RunSynchronously

                let classText =
                    e.Guild.CreateChannelAsync(
                        guild.Lang.ClassTextChannelName,
                        ChannelType.Text,
                        category,
                        overwrites = [
                            DiscordOverwriteBuilder()
                                .For(e.Guild.EveryoneRole)
                                .Allow(minPermsText)
                                .Deny(Permissions.All - minPermsText)
                            DiscordOverwriteBuilder()
                                .For(teacherRole)
                                .Allow(Permissions.All)
                        ]
                    )
                    |> Async.AwaitTask |> Async.RunSynchronously
                let classVoice =
                    e.Guild.CreateChannelAsync(
                        guild.Lang.ClassVoiceChannelName,
                        ChannelType.Voice,
                        category,
                        overwrites = [
                            DiscordOverwriteBuilder()
                                .For(e.Guild.EveryoneRole)
                                .Allow(minPermsVoice)
                                .Deny(Permissions.All - minPermsVoice)
                            DiscordOverwriteBuilder()
                                .For(teacherRole)
                                .Allow(Permissions.All)
                        ] )
                    |> Async.AwaitTask |> Async.RunSynchronously

                if guild.IsConfigOnDb then
                    let filter = GC.Filter.And [ GC.Filter.Eq((fun g -> g._id), e.Guild.Id) ]
                    let update = GC.Update.Combine [
                        GC.Update.Set((fun g -> g.TeachersText), Some teachersText.Id)
                        GC.Update.Set((fun g -> g.ClassVoice), Some classVoice.Id)
                        GC.Update.Set((fun g -> g.TeacherRole), Some teacherRole.Id)
                    ]
                    GC.UpdateOne app.Db filter update
                    |> Async.Ignore
                else
                    GC.Insert app.Db
                        { GC.Base with
                            _id = e.Guild.Id
                            TeachersText = Some teachersText.Id
                            ClassVoice = Some classVoice.Id
                            TeacherRole = Some teacherRole.Id }
                |> Async.RunSynchronously

                exchangeReactions e.Message client app.Emojis.Doing app.Emojis.Yes
    )

    let exec app guild (client : DiscordClient) args (e : MessageCreateEventArgs) = async {
        if checkPermissions e RequiredPerms then
            if
                guild.TeachersText.IsSome ||
                guild.ClassVoice.IsSome ||
                guild.TeacherRole.IsSome
            then
                guild.Lang.InitAlreadyInited guild.CommandPrefix
                |> fun s -> e.Channel.SendMessageAsync(s)
                |> Async.AwaitTask
                |> Async.RunSynchronously
                |> ignore
            else
                let confirmMsg =
                    guild.Lang.InitConfirmationMsg guild.CommandPrefix
                        guild.Lang.ConfirmationYesResponse guild.Lang.ConfirmationNoResponse
                    |> fun s -> e.Channel.SendMessageAsync(s)
                    |> Async.AwaitTask
                    |> Async.RunSynchronously

                let inter = client.GetInteractivity()

                // This took me a long time to figure it out,
                // using Task.Delay, Async.Sleep, Async.RunSynchronously, even running in parallel
                // blocked the threads of the main MessageCreated event. And (Task<_> ...).RunSynchronously()
                // raises an exeption in DSharpPlus
                inter.WaitForMessageAsync(
                        messageCreated guild e.Message, Nullable (TimeSpan.FromSeconds 10.0))
                    .ContinueWith (afterConfirmation app guild client confirmMsg e)
                |> ignore
    }

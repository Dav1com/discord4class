namespace Discord4Class.Commands

open System
open System.Threading.Tasks
open DSharpPlus
open DSharpPlus.Entities
open DSharpPlus.EventArgs
open DSharpPlus.Interactivity
open Discord4Class.Helpers.Permission
open Discord4Class.Config.InnerTypes
open Discord4Class.Config.Types
open Discord4Class.Repositories.GuildConfiguration

module Init =

    [<Literal>]
    let RequiredPerms = Permissions.Administrator

    let TeacherPerms =
        Permissions.CreateInstantInvite +
        Permissions.AttachFiles +
        Permissions.AddReactions +
        Permissions.ManageMessages +
        //Permissions.ManageNicknames +
        Permissions.MentionEveryone +
        Permissions.MoveMembers +
        Permissions.MuteMembers +
        Permissions.ReadMessageHistory +
        Permissions.SendMessages

    let private messageCreated config (e : DiscordMessage) = Func<DiscordMessage, bool> (fun e2 ->
        e.ChannelId = e2.ChannelId && e2.Author.Id = e.Author.Id && (
            e2.Content.ToLower() = config.Guild.Lang.ConfirmationYesResponse.ToLower() ||
            e2.Content.ToLower() = config.Guild.Lang.ConfirmationNoResponse.ToLower()
        )
    )

    let private afterConfirmation config (client : DiscordClient) (initMsg : DiscordMessage) (e : MessageCreateEventArgs) = Action<Task<InteractivityResult<DiscordMessage>>>(fun r ->

        let result = r.Result
        if result.TimedOut then
            initMsg.Content + "\n" + config.Guild.Lang.ConfirmationTimeoutMessage
            |> fun s -> initMsg.ModifyAsync(Optional s)
            |> Async.AwaitTask
            |> Async.RunSynchronously
            |> ignore
        else
            if result.Result.Content.ToLower() = config.Guild.Lang.ConfirmationNoResponse.ToLower() then
                config.Guild.Lang.ConfirmationCancellation
                |> fun s -> e.Channel.SendMessageAsync(s)
                |> Async.AwaitTask
                |> Async.RunSynchronously
                |> ignore
            else

                let thisMember =
                    e.Guild.GetMemberAsync client.CurrentUser.Id
                    |> Async.AwaitTask |> Async.RunSynchronously

                let teacherRole =
                    e.Guild.CreateRoleAsync(
                        config.Guild.Lang.TeachersRoleName,
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
                    e.Guild.CreateChannelAsync(config.Guild.Lang.ClassCategoryName, ChannelType.Category)
                    |> Async.AwaitTask
                    |> Async.RunSynchronously
                let teachersText =
                    e.Guild.CreateChannelAsync(
                        config.Guild.Lang.TeachersClassTextChannelName,
                        ChannelType.Text,
                        category,
                        overwrites = [
                            DiscordOverwriteBuilder()
                                .For(teacherRole)
                                .Allow(Permissions.All)
                            DiscordOverwriteBuilder()
                                .For(e.Guild.EveryoneRole)
                                .Deny(Permissions.All)
                            DiscordOverwriteBuilder()
                                .For(thisMember)
                                .Allow(minPermsText)
                        ]
                    )
                    |> Async.AwaitTask |> Async.RunSynchronously

                let classText =
                    e.Guild.CreateChannelAsync(
                        config.Guild.Lang.ClassTextChannelName,
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
                            DiscordOverwriteBuilder()
                                .For(thisMember)
                                .Allow(minPermsText)
                        ]
                    )
                    |> Async.AwaitTask |> Async.RunSynchronously
                let classVoice =
                    e.Guild.CreateChannelAsync(
                        config.Guild.Lang.ClassVoiceChannelName,
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
                            DiscordOverwriteBuilder()
                                .For(thisMember)
                                .Allow(Permissions.AccessChannels)
                        ] )
                    |> Async.AwaitTask |> Async.RunSynchronously


                printfn "TEST: %A" config.Guild.IsConfigOnDb
                if config.Guild.IsConfigOnDb then
                    let filter = GC.Filter.And [ GC.Filter.Eq((fun g -> g._id), e.Guild.Id) ]
                    let update = GC.Update.Combine [
                        GC.Update.Set((fun g -> g.TeachersText), Some teachersText.Id)
                        GC.Update.Set((fun g -> g.ClassVoice), Some classVoice.Id)
                        GC.Update.Set((fun g -> g.TeacherRole), Some teacherRole.Id)
                    ]
                    GC.UpdateOne config.App.DbDatabase filter update
                    |> Async.Ignore
                else
                    GC.Insert config.App.DbDatabase
                        { GC.Base with
                            _id = e.Guild.Id
                            TeachersText = Some teachersText.Id
                            ClassVoice = Some classVoice.Id
                            TeacherRole = Some teacherRole.Id }
                |> Async.RunSynchronously

                config.Guild.Lang.InitSuccess
                |> fun s -> e.Channel.SendMessageAsync(s)
                |> Async.AwaitTask
                |> Async.RunSynchronously
                |> ignore
    )

    let exec config (client : DiscordClient) mode (e : MessageCreateEventArgs) = async {
        if checkPermissions e RequiredPerms then
            if
                config.Guild.TeachersText.IsSome ||
                config.Guild.ClassVoice.IsSome ||
                config.Guild.TeacherRole.IsSome
            then
                config.Guild.Lang.InitAlreadyInited config.Guild.CommandPrefix
                |> fun s -> e.Channel.SendMessageAsync(s)
                |> Async.AwaitTask
                |> Async.RunSynchronously
                |> ignore
            else
                let initMsg =
                    config.Guild.Lang.InitConfirmationMsg config.Guild.CommandPrefix
                        config.Guild.Lang.ConfirmationYesResponse config.Guild.Lang.ConfirmationNoResponse
                    |> fun s -> e.Channel.SendMessageAsync(s)
                    |> Async.AwaitTask
                    |> Async.RunSynchronously

                let inter = client.GetInteractivity()

                // This took me a long time to figure it out,
                // using Taks.Delay, Async.Sleep, Async.RunSynchronously, even running in parallel
                // blocked the threads of the main MessageCreated event. And (Task<_> ...).RunSynchronously()
                // raises an exeption in DSharpPlus
                inter.WaitForMessageAsync(
                        messageCreated config e.Message, Nullable (TimeSpan.FromSeconds 5.0))
                    .ContinueWith (afterConfirmation config client initMsg e)
                |> ignore
    }

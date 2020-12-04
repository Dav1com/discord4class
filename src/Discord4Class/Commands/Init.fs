namespace Discord4Class.Commands

open System
open DSharpPlus
open DSharpPlus.Entities
open DSharpPlus.EventArgs
open DSharpPlus.Interactivity
open DSharpPlus.Interactivity.Extensions
open Discord4Class.Helpers.Messages
open Discord4Class.Helpers.Permission
open Discord4Class.Repositories.GuildData
open Discord4Class.Config.Types
open Discord4Class.CommandsManager

module Init =

    let private teacherPerms =
          Permissions.AttachFiles
        + Permissions.AddReactions
        + Permissions.ManageMessages
        + Permissions.MoveMembers
        + Permissions.MuteMembers
        + Permissions.ReadMessageHistory
        + Permissions.SendMessages

    let private messageCreated guild (e: DiscordMessage) = Func<DiscordMessage, bool> (fun e2 ->
        e.ChannelId = e2.ChannelId && e2.Author.Id = e.Author.Id &&
            ( e2.Content.ToLower() = guild.Lang.Yes ||
              e2.Content.ToLower() = guild.Lang.No ) )

    let private afterConfirmation app guild (memb: DiscordMember) (confirmMsg: DiscordMessage) (e: MessageCreateEventArgs) (result: InteractivityResult<DiscordMessage>) =
        if result.TimedOut then
            guild.Lang.ConfirmationTimeoutMessage
            |> modifyMessage confirmMsg |> ignore
        else
            if result.Result.Content.ToLower() = guild.Lang.No then
                guild.Lang.ConfirmationCancellation
                |> sendMessage e.Channel |> ignore
            else
                [ deleteMessageAsync confirmMsg
                  addReactionAsync e.Message app.Emojis.Doing ]
                |> Async.Parallel |> Async.RunSynchronously |> ignore

                let teacherRole =
                    e.Guild.CreateRoleAsync(
                        guild.Lang.TeachersRoleName,
                        Nullable teacherPerms,
                        Nullable DiscordColor.Blue,
                        Nullable true,
                        Nullable true )
                    |> Async.AwaitTask |> Async.RunSynchronously
                memb.GrantRoleAsync teacherRole
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
                                .Allow(minPermsText)
                            DiscordOverwriteBuilder()
                                .For(e.Guild.EveryoneRole)
                                .Deny(Permissions.All)
                            DiscordOverwriteBuilder()
                                .For(e.Guild.CurrentMember)
                                .Allow(
                                    minPermsText ||| Permissions.UseExternalEmojis ) ] )
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
                            DiscordOverwriteBuilder()
                                .For(teacherRole)
                                .Allow(minPermsText)
                            DiscordOverwriteBuilder()
                                .For(e.Guild.CurrentMember)
                                .Allow(minPermsText ||| Permissions.UseExternalEmojis) ] )
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
                            DiscordOverwriteBuilder()
                                .For(e.Guild.CurrentMember)
                                .Allow(
                                      Permissions.MuteMembers
                                    + Permissions.AccessChannels
                                    + Permissions.MoveMembers ) ] )
                    |> Async.AwaitTask |> Async.RunSynchronously

                if guild.IsConfigOnDb then
                    GD.Update.Combine [
                        GD.Update.Set((fun g -> g.TeachersText), Nullable<_> teachersText.Id)
                        GD.Update.Set((fun g -> g.ClassVoice), Nullable<_> classVoice.Id)
                        GD.Update.Set((fun g -> g.TeacherRole), Nullable<_> teacherRole.Id) ]
                    |> GD.Operation.UpdateOneById app.Db e.Guild.Id
                    |> Async.Ignore
                else
                    GD.Operation.InsertOne app.Db
                        { GD.Base with
                            Id = e.Guild.Id
                            TeachersText = Some teachersText.Id
                            ClassVoice = Some classVoice.Id
                            TeacherRole = Some teacherRole.Id }
                |> Async.RunSynchronously

                changeReaction e.Message app.Emojis.Yes

    let main app guild (client: DiscordClient) args memb (e: MessageCreateEventArgs) = async {
        if    guild.TeacherRole.IsSome
           || guild.ClassVoice.IsSome
           || guild.TeachersText.IsSome then
            guild.Lang.InitAlreadyInited guild.CommandPrefix
            |> sendMessage e.Channel |> ignore
        else
        let confirmMsg =
            guild.Lang.InitConfirmationMsg guild.CommandPrefix
                guild.Lang.Yes
                guild.Lang.No
            |> sendMessage e.Channel

        let inter = client.GetInteractivity()

        inter.WaitForMessageAsync(
            messageCreated guild e.Message,
            Nullable (TimeSpan.FromSeconds 10.0))
        |> Async.AwaitTask |> Async.RunSynchronously
        |> afterConfirmation app guild memb confirmMsg e
    }

    let command =
        { BaseCommand with
            Names = [ "init" ]
            Description = fun gc ->
                gc.Lang.InitDescription gc.CommandPrefix gc.Lang.InitUsage
            Permissions = GuildPrivileged
            IsHeavy = true
            RateLimits = [
                { Allowed = 1uy
                  Interval = 30UL } ]
            Function = main }

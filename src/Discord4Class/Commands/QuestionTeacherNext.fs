namespace Discord4Class.Commands

open DSharpPlus
open DSharpPlus.Entities
open DSharpPlus.EventArgs
open Discord4Class.Helpers.Messages
open Discord4Class.Helpers.DiscordUrls
open Discord4Class.Repositories.Questions
open Discord4Class.Config.Types
open Discord4Class.CommandsManager

module QuestionTeacherNext =

    let private extractQuestion guild (currUser: DiscordUser) (msg: string) =
        if msg.StartsWith guild.CommandPrefix then
            msg.[msg.IndexOf ' '..].TrimStart()
        elif msg.StartsWith (currUser.Mention.Insert(2, "!")) then
            msg.[currUser.Mention.Length + 1..].TrimStart().IndexOf ' '
            |> fun i -> msg.[i..].TrimStart()
        else msg

    let private buildEmbed guildConf currUser (msgMemb: DiscordMember) (guild: DiscordGuild) (channel: DiscordChannel) (message: DiscordMessage) =
        DiscordEmbedBuilder()
            .WithDescription(extractQuestion guildConf currUser message.Content)
            .WithColor(DiscordColor.SpringGreen)
            .WithFooter(msgMemb.DisplayName, msgMemb.AvatarUrl)
            .WithTimestamp(message.Timestamp)
            .AddField(guildConf.Lang.QuestionTeacherNextEmbedSection,
                messageUrl guild.Id channel.Id message.Id)
            .Build()

    let rec main app guild (clt: DiscordClient) args memb (e: MessageCreateEventArgs) = async {
        Qs.PopQuestion app.Db e.Guild.Id
        |> Async.RunSynchronously
        |> function
            | Some {ChId = channelId; Id = messageId} ->
                e.Guild.Channels.TryGetValue channelId
                |> function
                    | (false, _) ->
                        main app guild clt args memb e
                        |> Async.RunSynchronously
                    | (true, channel) ->
                        channel.GetMessageAsync(messageId)
                        |> Async.AwaitTask |> Async.RunSynchronously
                        |> function
                            | null ->
                                main app guild clt args memb e
                                |> Async.RunSynchronously
                            | message ->
                                e.Guild.Members.TryGetValue message.Author.Id
                                |> function
                                    | (false, _) ->
                                        main app guild clt args memb e
                                        |> Async.RunSynchronously
                                    | (true, msgMemb) ->
                                        buildEmbed guild clt.CurrentUser
                                            msgMemb e.Guild channel message
                                        |> sendEmbed e.Channel
                                        |> ignore
            | None ->
                guild.Lang.QuestionTeacherNoMoreQuestions
                |> sendMessage e.Channel |> ignore
    }

    let command =
        { BaseCommand with
            Names = [ "next" ]
            Description = fun gc ->
                gc.Lang.QuestionTeacherNextDescription
                    gc.CommandPrefix gc.Lang.QuestionTeacherNextUsage
            Permissions = Teacher
            RateLimits = [
                { Allowed = 4uy
                  Interval = 10UL }
                { Allowed = 100uy
                  Interval = 600UL } ]
            Function = main }

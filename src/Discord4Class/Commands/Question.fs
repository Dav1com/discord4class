namespace Discord4Class.Commands

open System.Threading.Tasks
open DSharpPlus
open DSharpPlus.Entities
open DSharpPlus.EventArgs
open Discord4Class.Config.Types

module Question =

    let exec config clt (args : string) (e : MessageCreateEventArgs) = async {
            if config.Guild.TeachersText.IsNone then
                config.Guild.Lang.ErrorTextChannelNull "teachers-text"
                    config.Guild.CommandPrefix "teachers-text"
                |> fun s -> e.Channel.SendMessageAsync(s)
                |> Async.AwaitTask |> Async.RunSynchronously |> ignore
            elif Some e.Channel.Id = config.Guild.TeachersText then
                () // List all questions ¿?
            elif args.Trim() = "" then
                DiscordEmoji.FromName(clt, config.Guild.Lang.QuestionNotSended)
                |> e.Message.CreateReactionAsync
                |> Async.AwaitTask |> Async.RunSynchronously
            else
                match e.Guild.GetChannel config.Guild.TeachersText.Value with
                | null ->
                    config.Guild.Lang.ErrorTextChannelDeleted "teachers-text"
                        config.Guild.CommandPrefix "teachers-text"
                    |> fun s -> e.Channel.SendMessageAsync(s)
                    |> Async.AwaitTask |> Async.RunSynchronously |> ignore
                | ch ->
                    e.Author.Id
                    |> e.Guild.GetMemberAsync
                    |> Async.AwaitTask |> Async.RunSynchronously
                    |> fun m -> m.DisplayName
                    |> config.Guild.Lang.QuestionReceived args
                    |> fun s -> ch.SendMessageAsync(s)
                    |> Async.AwaitTask |> Async.RunSynchronously |> ignore

                    DiscordEmoji.FromName(clt, config.Guild.Lang.QuestionReaction)
                    |> e.Message.CreateReactionAsync
                    |> Async.AwaitTask
                    |> Async.RunSynchronously
        }

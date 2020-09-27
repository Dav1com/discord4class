namespace Discord4Class.Commands

open System
open System.Threading.Tasks
open DSharpPlus
open DSharpPlus.Entities
open DSharpPlus.EventArgs
open DSharpPlus.Interactivity
open Discord4Class.Config.InnerTypes
open Discord4Class.Config.Types
open Discord4Class.Repositories.GuildConfiguration

module Init =

    let private messageCreated config (e : DiscordMessage) = Func<DiscordMessage, bool> (fun e2 ->
        e.ChannelId = e2.ChannelId && e2.Author.Id = e.Author.Id && (
            e2.Content.ToLower() = config.Guild.Lang.ConfirmationYesResponse.ToLower() ||
            e2.Content.ToLower() = config.Guild.Lang.ConfirmationNoResponse.ToLower()
        )
    )

    let private afterConfirmation config (initMsg : DiscordMessage) (e : MessageCreateEventArgs) = Action<Task<MessageContext>>(fun r ->
        let result = r.Result
        if isNull result then
            initMsg.Content + "\n" + config.Guild.Lang.ConfirmationTimeoutMessage
            |> fun s -> initMsg.ModifyAsync(Optional s)
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
                // category container
                let category =
                    e.Guild.CreateChannelAsync(config.Guild.Lang.ClassCategoryName, ChannelType.Category)
                    |> Async.AwaitTask
                    |> Async.RunSynchronously
                [
                    e.Guild.CreateChannelAsync(
                        config.Guild.Lang.TeachersClassTextChannelName,
                        ChannelType.Text, category )
                    |> Async.AwaitTask

                    e.Guild.CreateChannelAsync(
                        config.Guild.Lang.ClassTextChannelName,
                        ChannelType.Text, category )
                    |> Async.AwaitTask

                    e.Guild.CreateChannelAsync(
                        config.Guild.Lang.ClassVoiceChannelName,
                        ChannelType.Voice, category )
                    |> Async.AwaitTask
                ]
                |> Async.Parallel
                |> Async.RunSynchronously
                |> Array.map (fun c -> c.Id)
                |> fun [|teachersText; classText; classVoice|] ->
                    match config.Guild.IsConfigOnDb with
                    | false ->
                        GC.Insert config.App.DbDatabase {
                            _id = e.Guild.Id
                            CommandPrefix = config.Bot.CommandPrefix
                            Language = config.Bot.DefaultLang
                            Channels = Some {
                                TeachersText = teachersText
                                ClassText = classText
                                ClassVoice = classVoice
                            }
                        }
                        |> Async.RunSynchronously
                    | true ->
                        let filter = GC.Filter.And [ GC.Filter.Eq((fun g -> g._id), e.Guild.Id) ]
                        let update = GC.Update.Set((fun g -> g.Channels), Some {
                            TeachersText = teachersText
                            ClassText = classText
                            ClassVoice = classVoice
                        })
                        GC.UpdateOne config.App.DbDatabase filter update
                        |> Async.RunSynchronously
                        |> ignore
                |> ignore

                config.Guild.Lang.InitSuccess
                |> fun s -> e.Channel.SendMessageAsync(s)
                |> Async.AwaitTask
                |> Async.RunSynchronously
                |> ignore
    )

    let exec config (e : MessageCreateEventArgs) =
        async {
            if config.Guild.Channels.IsSome then
                config.Guild.Lang.InitAlreadyInited config.Guild.CommandPrefix
                |> fun s -> e.Channel.SendMessageAsync(s)
                |> Async.AwaitTask
                |> Async.RunSynchronously
                |> ignore
            else
                let initMsg =
                    config.Guild.Lang.InitConfirmationMsg config.Guild.CommandPrefix
                        config.Guild.CommandPrefix config.Guild.Lang.ConfirmationYesResponse
                        config.Guild.Lang.ConfirmationNoResponse
                    |> fun s -> e.Channel.SendMessageAsync(s)
                    |> Async.AwaitTask
                    |> Async.RunSynchronously

                let inter = (e.Client :?> DiscordClient).GetInteractivityModule()

                // This took me a long time to figure it out,
                // using Taks.Delay, Async.Sleep, Async.RunSynchronously, even running in parallel
                // blocked the threads of the main MessageCreated event. And (Task<_> ...).RunSynchronously()
                // raises an exeption in DSharpPlus
                inter.WaitForMessageAsync(
                        messageCreated config e.Message, Nullable (TimeSpan.FromSeconds 30.0))
                    .ContinueWith (afterConfirmation config initMsg e)
                |> ignore
        } |> Async.StartAsTask :> Task

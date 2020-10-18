namespace Discord4Class.Helpers

open DSharpPlus.Entities

module Messages =

    let deleteMessageAsync (msg : DiscordMessage) =
        msg.DeleteAsync() |> Async.AwaitTask

    let deleteMessage m = deleteMessageAsync m |> Async.RunSynchronously

    let sendMessageAsync (ch : DiscordChannel) msg =
        ch.SendMessageAsync(msg) |> Async.AwaitTask

    let sendMessage c m = sendMessageAsync c m |> Async.RunSynchronously

    let modifyMessageAsync (msg : DiscordMessage) newMsg =
        msg.ModifyAsync(Optional newMsg) |> Async.AwaitTask

    let modifyMessage m s = modifyMessageAsync m s |> Async.RunSynchronously

    let addReactionAsync (msg : DiscordMessage) client (emoji : string) =
        if emoji.[0] = ':' then
            DiscordEmoji.FromName(client, emoji)
        else
            DiscordEmoji.FromGuildEmote(client, uint64 emoji)
        |> msg.CreateReactionAsync
        |> Async.AwaitTask

    let addReaction m c e = addReactionAsync m c e |> Async.RunSynchronously

    let removeReactionAsync (msg : DiscordMessage) client (emoji : string) =
        if emoji.[0] = ':' then
            DiscordEmoji.FromName(client, emoji)
        else
            DiscordEmoji.FromGuildEmote(client, uint64 emoji)
        |> msg.DeleteOwnReactionAsync
        |> Async.AwaitTask

    let removeReaction m c e = removeReactionAsync m c e |> Async.RunSynchronously

    let exchangeReactions (msg : DiscordMessage) client (emoji1 : string) (emoji2 : string) =
        [
            removeReactionAsync msg client emoji1
            addReactionAsync msg client emoji2
        ]
        |> Async.Parallel |> Async.RunSynchronously |> ignore

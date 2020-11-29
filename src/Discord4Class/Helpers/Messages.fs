namespace Discord4Class.Helpers

open DSharpPlus.Entities

module Messages =

    let deleteMessageAsync (msg: DiscordMessage) =
        msg.DeleteAsync() |> Async.AwaitTask

    let sendMessageAsync (ch: DiscordChannel) msg =
        ch.SendMessageAsync(msg) |> Async.AwaitTask

    let sendEmbedAsync (ch: DiscordChannel) embed =
        ch.SendMessageAsync(null, false, embed) |> Async.AwaitTask

    let modifyMessageAsync (msg: DiscordMessage) newMsg =
        msg.ModifyAsync(Optional newMsg) |> Async.AwaitTask

    let addReactionAsync (msg: DiscordMessage) emoji =
        msg.CreateReactionAsync emoji |> Async.AwaitTask

    let removeReactionAsync (msg: DiscordMessage) emoji =
        msg.DeleteOwnReactionAsync emoji |> Async.AwaitTask

    let removeReactionsAsync (m: DiscordMessage) =
        m.DeleteAllReactionsAsync() |> Async.AwaitTask

    let deleteMessage m = deleteMessageAsync m |> Async.RunSynchronously
    let sendMessage c = sendMessageAsync c >> Async.RunSynchronously
    let sendEmbed ch = sendEmbedAsync ch >> Async.RunSynchronously
    let modifyMessage m = modifyMessageAsync m >> Async.RunSynchronously
    let addReaction m = addReactionAsync m >> Async.RunSynchronously
    let removeReaction m = removeReactionAsync m >> Async.RunSynchronously
    let removeReactions m = removeReactionsAsync m |> Async.RunSynchronously

    let changeReaction msg emoji =
        removeReactions msg
        addReaction msg emoji

namespace Discord4Class.Helpers

open DSharpPlus.Entities

module Embed =

    let newEmbed color title msg =
        DiscordEmbedBuilder()
            .WithColor(color)
            .WithTitle(title)
            .WithDescription(msg)
            .Build()

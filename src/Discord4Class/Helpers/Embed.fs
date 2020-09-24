namespace Discord4Class.Helpers

open DSharpPlus.Entities

module Embed =

    let newEmbed (color : int) title msg =
        DiscordEmbedBuilder()
            .WithColor( DiscordColor(color) )
            .WithAuthor(title)
            .WithDescription(msg)
            .Build()

namespace Discord4Class.Helpers

open Discord4Class.Constants

module DiscordUrls =

    let messageUrl (guildId: uint64) (channelId: uint64) (messageId: uint64) =
        $"https://%s{DiscordDomain}channels/%u{guildId}/%u{channelId}/%i{messageId}"


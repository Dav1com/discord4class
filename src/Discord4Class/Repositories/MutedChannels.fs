namespace Discord4Class.Repositories

module MutedChannels =

    type MutedChannel =
      { ChannelId : uint64 }

    type GuildMutedChannels =
      { GuildId : uint64
        MutedAll : bool
        MutedChannels : MutedChannel list option}



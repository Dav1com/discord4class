namespace Discord4Class

open DSharpPlus

module Constants =
    [<Literal>]
    let AppName = "Discord For Classrooms"
    [<Literal>]
    let AppVersion = "0.0.0"
    [<Literal>]
    let IniPath = "config.ini"
    [<Literal>]
    let LangPath = "res/lang/"
    [<Literal>]
    let LargeGuildThreshold = 1000 // move to config.ini ¿?
    [<Literal>]
    let MessageMaxLength = 2000
    [<Literal>]
    let EmbedDescriptionMaxLength = 2048
    [<Literal>]
    let GuildMaxChannels = 500
    [<Literal>]
    let GuildMaxRoles = 250
    [<Literal>]
    let PrefixMaxSize = 2
    [<Literal>]
    let TeamsChannelsFactor = 3 // channels per teams
    [<Literal>]
    let DiscordDomain = "discord.com/"
    [<Literal>]
    let GuildPrivilegedPerm = Permissions.ManageGuild

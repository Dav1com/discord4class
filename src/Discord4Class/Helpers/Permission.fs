namespace Discord4Class.Helpers

module Permission =

    type Permission =
        | NoPerm = 0x0
        | Student = 0x1
        | Delegate = 0x2
        | Teacher = 0x4
        | Admin = 0x8
        | GuildOwner = 0x8

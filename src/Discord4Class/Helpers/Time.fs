namespace Discord4Class.Helpers

open System

module Time =

    let timeSpanFromUnixEpoch () =
        DateTime.UtcNow.Subtract(DateTime.UnixEpoch)

    let unixTimeStamp () =
        uint64 (timeSpanFromUnixEpoch().TotalSeconds)

    let nextInterval interval =
        ((unixTimeStamp() / interval) + 1UL) * interval

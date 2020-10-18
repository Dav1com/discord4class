﻿open Discord4Class.Constants
open Discord4Class.Helpers.Railway
open Discord4Class.Config.Loader
open Discord4Class.Args
open Discord4Class.Bot

[<EntryPoint>]
let main argv =
    let config = loadConfiguration IniPath

    getParser AppName
    |> parseArgv argv
    |> execArgs config.App
    >>= switch (
        getAllArgs
        >> List.fold overwriteConfig config
        >> runBot
        >> Async.RunSynchronously
    )
    |> onError (eprintf "%s")
    |> ignore

    0

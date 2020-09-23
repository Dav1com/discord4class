[<Literal>]
let IniPath = "config.ini" //TODO: move to App.config

open Discord4Class.Helpers.Railway
open Discord4Class.Config.Loader
open Discord4Class.Args
open Discord4Class.Bot

[<EntryPoint>]
let main argv =
    let config = loadConfiguration IniPath

    getParser config.App.AppName
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

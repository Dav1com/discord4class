open Discord4Class.Constants
open Discord4Class.Helpers.Railway
open Discord4Class.Config.Loader
open Discord4Class.Args
open Discord4Class.Bot

[<EntryPoint>]
let main argv =
    let config =
        match loadConfiguration IniPath with
        | Ok c -> c
        | Error e -> e.Fail()

    getParser AppName
    |> parseArgv argv
    |> execArgs
    >>= switch (
        getAllArgs
        >> List.fold overwriteConfig config
        >> runBot
        >> Async.RunSynchronously )
    |> Result.mapError (eprintf "%s")
    |> ignore

    0

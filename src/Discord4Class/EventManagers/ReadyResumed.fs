namespace Discord4Class.EventManagers

open DSharpPlus.EventArgs
open Discord4Class.Repositories.GuildData
open Discord4Class.Config.Types

module ReadyResumed =

    [<Literal>]
    let private maxTries = 100

    let rec initDatabase config tries =
        try
            GD.Update.Set((fun gd -> gd.DoingHeavyTask), false)
            |> GD.Operation.UpdateMany config.App.Db GD.Filter.Empty
            |> Async.RunSynchronously |> ignore
            printfn "Database susccessfully initialized, resuming bot"
        with
        | e ->
            if tries < maxTries then
                printfn "Database initialization failed, retring... (%i try)" tries
                Async.Sleep(3000) |> Async.RunSynchronously
                initDatabase config (tries + 1)
            else failwithf "FATAL: Couldn't initialize database"

    let main config client (e: ReadyEventArgs) = async {
        e.set_Handled true
        initDatabase config 1
        e.set_Handled false
    }

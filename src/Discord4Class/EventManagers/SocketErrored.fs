namespace Discord4Class.EventManagers

open DSharpPlus.EventArgs

module SocketErrored =

    let main config client (e: SocketErrorEventArgs) = async {
        printfn "Socket Error Ocurred: %s %s"
            (e.Exception.GetType().ToString())
            e.Exception.Message
    }

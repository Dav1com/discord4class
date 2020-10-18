namespace Discord4Class.Commands

open DSharpPlus.EventArgs
open Discord4Class.Constants
open Discord4Class.Helpers.Messages
open Discord4Class.Config.Types

module LatexMath =

    [<Literal>]
    let LatexApiEndpoint = "https://latex.codecogs.com/png.latex?"

    [<Literal>]
    let DefaultArgs = "\\dpi{120}&space;\\bg_white&space;\\large&space;"

    let exec app _guild client equation (e : MessageCreateEventArgs) = async {
        equation
        |> String.collect (function
            | ' ' -> "&space;"
            | c -> c.ToString()
        )
        |> function
        | s when s.Length > MessageMaxLength ->
            addReaction e.Message client app.Emojis.No
        | s ->
            sprintf "%s%s%s" LatexApiEndpoint DefaultArgs s
            |> sendMessage e.Channel |> ignore
    }

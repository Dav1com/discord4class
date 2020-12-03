namespace Discord4Class.Commands

open DSharpPlus.EventArgs
open Discord4Class.Constants
open Discord4Class.Helpers.Messages
open Discord4Class.Config.Types
open Discord4Class.CommandsManager

module LatexMath =

    [<Literal>]
    let private latexApiEndpoint = "https://latex.codecogs.com/png.latex?"
    [<Literal>]
    let private defaultArgs = "\\dpi{120}&space;\\bg_white&space;\\large&space;"

    let private eqMaxLength =
        MessageMaxLength - latexApiEndpoint.Length - defaultArgs.Length

    let main app guild client args memb (e : MessageCreateEventArgs) = async {
        match args with
        | (equation: string) :: _ ->
            match equation.Replace(" ", "&space;") with
            | s when s.Length > eqMaxLength ->
                addReaction e.Message app.Emojis.No
            | s ->
                latexApiEndpoint + defaultArgs + s
                |> sendMessage e.Channel |> ignore
        | [] -> addReaction e.Message app.Emojis.No
    }

    let command =
        { BaseCommand with
            Names = [ "math" ]
            Description = fun gc ->
                gc.Lang.MathDescription gc.CommandPrefix gc.Lang.MathUsage
            RateLimits = [
                { Allowed = 5uy
                  Interval = 10UL }
                { Allowed = 50uy
                  Interval = 600UL } ]
            Function = main }

namespace Discord4Class

open System

open Argu
open Discord4Class.Config
open Discord4Class.Config.Types

module Args =

    type Arguments =
        | [<AltCommandLineAttribute("-l")>] LogLevel of InnerTypes.LogLevelConf
        | [<AltCommandLineAttribute("-t")>] Token of string
        | [<AltCommandLineAttribute("-m")>] Mode of InnerTypes.BotMode
        | [<AltCommandLineAttribute("-v")>] Version

        interface IArgParserTemplate with
            member arg.Usage =
               match arg with
               | LogLevel _ -> "Sets the logging level. (0: Off, 1: Normal, 2: Debug)"
               | Token _ -> "Sets the bot token."
               | Mode _ -> "Forces the bot to act as private or public."
               | Version -> "Shows the program version."

    let getParser name =
        let errorHandler = ProcessExiter(colorizer = function ErrorCode.HelpText -> None | _ -> Some ConsoleColor.Red)
        ArgumentParser.Create<Arguments>(programName = name, errorHandler = errorHandler)

    let parseArgv argv (parser : ArgumentParser<_>) =
        parser.Parse argv

    let getAllArgs (result : ParseResults<_>) =
        result.GetAllResults()

    let execArgs (appConf : InnerTypes.AppConfig) (args : ParseResults<_>) =
        match args with
        | v when args.Contains Version ->
            printf "%s version v%s" appConf.AppName appConf.AppVersion
            Error ""
        | _ -> Ok args

    let overwriteConfig conf value =
        match value with
        | LogLevel l ->
            { conf with
                Preferences =
                    { conf.Preferences with
                        LogLevel = l }
            }
        | Token t ->
            { conf with
                Bot =
                    { conf.Bot with
                        BotToken = t }
            }
        | _ -> conf

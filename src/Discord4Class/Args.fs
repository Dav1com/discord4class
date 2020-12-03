namespace Discord4Class

open System

open Argu
open Discord4Class.Constants
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
        let errorHandler =
            ProcessExiter(colorizer = function
                | ErrorCode.HelpText -> None
                | _ -> Some ConsoleColor.Red )
        ArgumentParser.Create<Arguments>(
            programName = name,
            errorHandler = errorHandler )

    let parseArgv argv (parser: ArgumentParser<_>) = parser.Parse argv

    let getAllArgs (result: ParseResults<_>) = result.GetAllResults()

    let execArgs (args: ParseResults<_>) =
        if args.Contains Version then
            printf "%s version v%s" AppName AppVersion
            Error ""
        else Ok args

    let overwriteConfig (iConf: InnerTypes.IniConfig) value =
        match value with
        | LogLevel l ->
            { iConf with
                Preferences =
                    { iConf.Preferences with
                        LogLevel = l } }
        | Token t ->
            { iConf with
                Bot =
                    { iConf.Bot with
                        BotToken = t } }
        | _ -> iConf

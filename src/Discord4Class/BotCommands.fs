#nowarn "40"
namespace Discord4Class

open Discord4Class.CommandsManager.Types
open Discord4Class.CommandsManager.Manager
open Discord4Class.Commands

module BotCommands =

    let rec Commands =
        BotCommands (seq { // I need this to be lazy (it evaluates itself on the help command)
            Ping.command
            Lang.command
            Prefix.command
            Init.command
            Destroy.command
            { Config.command with
                SubCommands = BotCommands [
                    ConfigGet.command
                    ConfigSet.command
                    ConfigUnset.command ] }
            { QuestionTeacher.command with
               SubCommands = BotCommands [
                   QuestionTeacherNext.command
                   QuestionTeacherCount.command ] }
            Question.command
            { Teams.command with
                SubCommands = BotCommands [
                    TeamsMake.command
                    TeamsSize.command
                    TeamsManual.command
                    TeamsDestroy.command
                    TeamsMove.command
                    TeamsReturn.command ] }
            // This will wait until next ver, with the hand command
            // Mute.command
            LatexMath.command
            Help.dmCommand Commands
            Help.command Commands
        })

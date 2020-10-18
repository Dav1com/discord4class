namespace Discord4Class.Commands

open System.Threading.Tasks
open DSharpPlus
open DSharpPlus.Entities
open DSharpPlus.EventArgs
open Discord4Class.Helpers.Messages
open Discord4Class.Config.Types

module Question =

    let exec app guild clt (args : string) (e : MessageCreateEventArgs) = async {
        if guild.TeachersText.IsNone then
            guild.Lang.ErrorTextChannelNull "teachers-text"
                guild.CommandPrefix "teachers-text"
            |> sendMessage e.Channel |> ignore
        elif Some e.Channel.Id = guild.TeachersText then
            () // List all questions ¿?
        elif args.Trim() = "" then
            addReaction e.Message clt app.Emojis.No
        else
            match e.Guild.GetChannel guild.TeachersText.Value with
            | null ->
                guild.Lang.ErrorTextChannelDeleted "teachers-text"
                    guild.CommandPrefix "teachers-text"
                |> sendMessage e.Channel |> ignore
            | ch ->
                e.Author.Id
                |> e.Guild.GetMemberAsync
                |> Async.AwaitTask |> Async.RunSynchronously
                |> fun m -> m.DisplayName
                |> guild.Lang.QuestionReceived args
                |> sendMessage ch |> ignore
                addReaction e.Message clt app.Emojis.Sended
        }

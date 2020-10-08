namespace Discord4Class.Lang

open FSharp.Core
open FSharp.Reflection
open Discord4Class.Helpers.TypeShortcuts

// It would be nice to have a better way of doing this
module Types =

    type LangStrings =
      { //<StringsDefinition>
        ClassCategoryName : string
        ClassTextChannelName : string
        ClassVoiceChannelName : string
        ConfigActualValue : string
        ConfigActualValueNull : string
        ConfigInvalidName : string
        ConfigInvalidNewValue : string
        ConfigMissingName : string
        ConfigSuccess : string
        ConfigUnsetNull : string
        ConfigUnsetSuccess : string
        ConfirmationCancellation : string
        ConfirmationNoResponse : string
        ConfirmationTimeoutMessage : string
        ConfirmationYesResponse : string
        DestroyConfirmationMsg : string
        DestroySuccess : string
        ErrorCmdNotFound : string
        ErrorCmdUnknown : string
        ErrorEmbedAuthor : string
        InitAlreadyInited : string
        InitConfirmationMsg : string
        InitSuccess : string
        JoinGuildMessage : string
        LangMissingArg : string
        LangNotFound : string
        LangSuccess : string
        PingSuccess : string
        PrefixMissingArg : string
        PrefixNoChange : string
        PrefixSuccess : string
        PrefixTooLong : string
        QuestionNotInitialized : string
        QuestionReaction : string
        QuestionReceived : string
        QuestionSended : string
        ResponseToDm : string
        StudentsRoleName : string
        TeachersClassTextChannelName : string
        TeachersRoleName : string
        TeachersRoomCategoryName : string
        TeachersRoomTextChannelName : string
        //</StringsDefinition>
        }

        // While reading the file, the backslashes are scaped
        member this.SanitizeBackslashes () =
            FSharpValue.GetRecordFields this
            |> Array.map (fun s -> s.ToString().Replace("\\n", "\n") :> obj)
            |> fun a -> FSharpValue.MakeRecord(typeof<LangStrings>, a) :?> LangStrings

    type LangBuilders =
      { //<BuilderDefinition>
        ClassCategoryName : string
        ClassTextChannelName : string
        ClassVoiceChannelName : string
        ConfigActualValue : s -> s -> string
        ConfigActualValueNull : s -> string
        ConfigInvalidName : string
        ConfigInvalidNewValue : s -> string
        ConfigMissingName : s -> string
        ConfigSuccess : string
        ConfigUnsetNull : string
        ConfigUnsetSuccess : string
        ConfirmationCancellation : string
        ConfirmationNoResponse : string
        ConfirmationTimeoutMessage : string
        ConfirmationYesResponse : string
        DestroyConfirmationMsg : s -> s -> string
        DestroySuccess : string
        ErrorCmdNotFound : s -> string
        ErrorCmdUnknown : s -> s -> string
        ErrorEmbedAuthor : string
        InitAlreadyInited : s -> string
        InitConfirmationMsg : s -> s -> s -> s -> string
        InitSuccess : string
        JoinGuildMessage : s -> s -> s -> string
        LangMissingArg : s -> string
        LangNotFound : string
        LangSuccess : s -> string
        PingSuccess : i -> string
        PrefixMissingArg : s -> string
        PrefixNoChange : string
        PrefixSuccess : s -> string
        PrefixTooLong : i -> string
        QuestionNotInitialized : s -> string
        QuestionReaction : string
        QuestionReceived : s -> s -> string
        QuestionSended : string
        ResponseToDm : s -> s -> string
        StudentsRoleName : string
        TeachersClassTextChannelName : string
        TeachersRoleName : string
        TeachersRoomCategoryName : string
        TeachersRoomTextChannelName : string
        //</BuilderDefinition>
        }

        static member OfStrings (l : LangStrings) =
            {
                //<BuilderConversion>
                ClassCategoryName = sprintf (Printf.StringFormat<_> l.ClassCategoryName)
                ClassTextChannelName = sprintf (Printf.StringFormat<_> l.ClassTextChannelName)
                ClassVoiceChannelName = sprintf (Printf.StringFormat<_> l.ClassVoiceChannelName)
                ConfigActualValue = sprintf (Printf.StringFormat<_> l.ConfigActualValue)
                ConfigActualValueNull = sprintf (Printf.StringFormat<_> l.ConfigActualValueNull)
                ConfigInvalidName = sprintf (Printf.StringFormat<_> l.ConfigInvalidName)
                ConfigInvalidNewValue = sprintf (Printf.StringFormat<_> l.ConfigInvalidNewValue)
                ConfigMissingName = sprintf (Printf.StringFormat<_> l.ConfigMissingName)
                ConfigSuccess = sprintf (Printf.StringFormat<_> l.ConfigSuccess)
                ConfigUnsetNull = sprintf (Printf.StringFormat<_> l.ConfigUnsetNull)
                ConfigUnsetSuccess = sprintf (Printf.StringFormat<_> l.ConfigUnsetSuccess)
                ConfirmationCancellation = sprintf (Printf.StringFormat<_> l.ConfirmationCancellation)
                ConfirmationNoResponse = sprintf (Printf.StringFormat<_> l.ConfirmationNoResponse)
                ConfirmationTimeoutMessage = sprintf (Printf.StringFormat<_> l.ConfirmationTimeoutMessage)
                ConfirmationYesResponse = sprintf (Printf.StringFormat<_> l.ConfirmationYesResponse)
                DestroyConfirmationMsg = sprintf (Printf.StringFormat<_> l.DestroyConfirmationMsg)
                DestroySuccess = sprintf (Printf.StringFormat<_> l.DestroySuccess)
                ErrorCmdNotFound = sprintf (Printf.StringFormat<_> l.ErrorCmdNotFound)
                ErrorCmdUnknown = sprintf (Printf.StringFormat<_> l.ErrorCmdUnknown)
                ErrorEmbedAuthor = sprintf (Printf.StringFormat<_> l.ErrorEmbedAuthor)
                InitAlreadyInited = sprintf (Printf.StringFormat<_> l.InitAlreadyInited)
                InitConfirmationMsg = sprintf (Printf.StringFormat<_> l.InitConfirmationMsg)
                InitSuccess = sprintf (Printf.StringFormat<_> l.InitSuccess)
                JoinGuildMessage = sprintf (Printf.StringFormat<_> l.JoinGuildMessage)
                LangMissingArg = sprintf (Printf.StringFormat<_> l.LangMissingArg)
                LangNotFound = sprintf (Printf.StringFormat<_> l.LangNotFound)
                LangSuccess = sprintf (Printf.StringFormat<_> l.LangSuccess)
                PingSuccess = sprintf (Printf.StringFormat<_> l.PingSuccess)
                PrefixMissingArg = sprintf (Printf.StringFormat<_> l.PrefixMissingArg)
                PrefixNoChange = sprintf (Printf.StringFormat<_> l.PrefixNoChange)
                PrefixSuccess = sprintf (Printf.StringFormat<_> l.PrefixSuccess)
                PrefixTooLong = sprintf (Printf.StringFormat<_> l.PrefixTooLong)
                QuestionNotInitialized = sprintf (Printf.StringFormat<_> l.QuestionNotInitialized)
                QuestionReaction = sprintf (Printf.StringFormat<_> l.QuestionReaction)
                QuestionReceived = sprintf (Printf.StringFormat<_> l.QuestionReceived)
                QuestionSended = sprintf (Printf.StringFormat<_> l.QuestionSended)
                ResponseToDm = sprintf (Printf.StringFormat<_> l.ResponseToDm)
                StudentsRoleName = sprintf (Printf.StringFormat<_> l.StudentsRoleName)
                TeachersClassTextChannelName = sprintf (Printf.StringFormat<_> l.TeachersClassTextChannelName)
                TeachersRoleName = sprintf (Printf.StringFormat<_> l.TeachersRoleName)
                TeachersRoomCategoryName = sprintf (Printf.StringFormat<_> l.TeachersRoomCategoryName)
                TeachersRoomTextChannelName = sprintf (Printf.StringFormat<_> l.TeachersRoomTextChannelName)
                //</BuilderConversion>
            }

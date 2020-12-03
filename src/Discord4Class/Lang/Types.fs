namespace Discord4Class.Lang

open FSharp.Core
open FSharp.Reflection

module Types =

    type b = bool
    type s = string
    type c = char
    type i = int
    type d = int
    type u = uint32
    type x = int
    type X = int
    type o = obj
    type f = float
    type O = obj
    type A = obj

    type LangStrings =
        { //<StringsDefinition>
          ClassCategoryName:string
          ClassTextChannelName:string
          ClassVoiceChannelName:string
          ConfigDescription:string
          ConfigGetDescription:string
          ConfigGetMissingName:string
          ConfigGetUsage:string
          ConfigGetValue:string
          ConfigGetValueNull:string
          ConfigInvalidName:string
          ConfigInvalidSubcommand:string
          ConfigMissingSubcommand:string
          ConfigNames:string
          ConfigSetDescription:string
          ConfigSetInvalidValue:string
          ConfigSetMissingName:string
          ConfigSetMissingNewValue:string
          ConfigSetUsage:string
          ConfigUnsetDescription:string
          ConfigUnsetGuildNoData:string
          ConfigUnsetMissingName:string
          ConfigUnsetUsage:string
          ConfigUsage:string
          ConfirmationCancellation:string
          ConfirmationTimeoutMessage:string
          DeletedChannel:string
          DeletedRole:string
          DestroyConfirmationMsg:string
          DestroyDescription:string
          DestroySuccess:string
          DestroyUsage:string
          ErrorChannelDeleted:string
          ErrorChannelNull:string
          ErrorCmdNotFound:string
          ErrorCmdUnknown:string
          ErrorCommandNotSupportedOnLargeGuild:string
          ErrorEmbedAuthor:string
          ErrorEndHeavyTaskTriesExceeded:string
          ErrorRoleDeleted:string
          ErrorRoleNull:string
          Everyone:string
          HelpCommandNotFound:string
          HelpConfiguration:string
          HelpConfigurationDescription:string
          HelpDescription:string
          HelpEmbedDescription:string
          HelpEmbedTitle:string
          HelpGettingStarted:string
          HelpJoinServerTitle:string
          HelpLanguage:string
          HelpLanguageDescription:string
          HelpNotAvailable:string
          HelpReadingTheHelp:string
          HelpReadingTheHelpDescription:string
          HelpSupportServer:string
          HelpSupportServerDescription:string
          HelpUsage:string
          InitAlreadyInited:string
          InitConfirmationMsg:string
          InitDescription:string
          InitUsage:string
          JoinGuildMessage:string
          JoinLargeGuildMessage:string
          LangActualValue:string
          LangDescription:string
          LangNotFound:string
          LangSuccess:string
          LangUsage:string
          ManageGuild:string
          MathDescription:string
          MathUsage:string
          MuteDescription:string
          MuteUsage:string
          No:string
          NumberPlaceholder:string
          PingDescription:string
          PingSuccess:string
          PingUsage:string
          PrefixActualValue:string
          PrefixDescription:string
          PrefixNoChange:string
          PrefixNoPermission:string
          PrefixSuccess:string
          PrefixTooLong:string
          PrefixUsage:string
          QuestionDescription:string
          QuestionTeacherCount:string
          QuestionTeacherCountDescription:string
          QuestionTeacherCountUsage:string
          QuestionTeacherDescription:string
          QuestionTeacherInvalidSubcommand:string
          QuestionTeacherMissingSubcommand:string
          QuestionTeacherNextDescription:string
          QuestionTeacherNextEmbedSection:string
          QuestionTeacherNextUsage:string
          QuestionTeacherNoMoreQuestions:string
          QuestionTeacherUsage:string
          QuestionUsage:string
          ResponseToDm:string
          StudentsRoleName:string
          Teacher:string
          TeachersClassTextChannelName:string
          TeachersRoleName:string
          TeachersRoomCategoryName:string
          TeachersRoomTextChannelName:string
          Team:string
          TeamsAlreadyCreated:string
          TeamsDescription:string
          TeamsDestroyDescription:string
          TeamsDestroyTooMuchChannels:string
          TeamsDestroyTooMuchRoles:string
          TeamsDestroyUsage:string
          TeamsInvalidSubcommand:string
          TeamsMakeChannelsLimitHit:string
          TeamsMakeDescription:string
          TeamsMakeInvalidSwitchOrSize:string
          TeamsMakeMissingCount:string
          TeamsMakeOptions:string
          TeamsMakeOutOfRange:string
          TeamsMakeRolesLimitHit:string
          TeamsMakeUsage:string
          TeamsManualChannelsLimitHit:string
          TeamsManualDescription:string
          TeamsManualNoTeams:string
          TeamsManualOptions:string
          TeamsManualUsage:string
          TeamsMissingSubcommand:string
          TeamsMoveDescription:string
          TeamsMoveNoChannels:string
          TeamsMoveNoMembers:string
          TeamsMoveUsage:string
          TeamsNumberIsRight:string
          TeamsReturnDescription:string
          TeamsReturnNoChannels:string
          TeamsReturnNoMembers:string
          TeamsReturnUsage:string
          TeamsSizeChannelsLimitHit:string
          TeamsSizeDescription:string
          TeamsSizeInvalidSwitchOrSize:string
          TeamsSizeMissingSize:string
          TeamsSizeOptions:string
          TeamsSizeOutOfRange:string
          TeamsSizeRolesLimitHit:string
          TeamsSizeUsage:string
          TeamsTemplate:string
          TeamsUsage:string
          Yes:string
          //</StringsDefinition>
        }

        // While reading the file, the backslashes are escaped
        member this.SanitizeBackslashes () =
            FSharpValue.GetRecordFields this
            |> Array.map (fun s ->
                s.ToString()
                    .Replace("\\n", "\n")
                    .Replace("\\t", "\t") :> obj)
            |> fun a ->
                FSharpValue.MakeRecord(typeof<LangStrings>, a) :?> LangStrings

    type LangBuilders =
        {
            //<BuilderDefinition>
            ClassCategoryName:string;ClassTextChannelName:string;ClassVoiceChannelName:string;ConfigDescription:s->s->s->string;ConfigGetDescription:s->s->s->string;ConfigGetMissingName:s->string;ConfigGetUsage:string;ConfigGetValue:s->string;ConfigGetValueNull:s->string;ConfigInvalidName:string;ConfigInvalidSubcommand:s->string;ConfigMissingSubcommand:s->string;ConfigNames:string;ConfigSetDescription:s->s->s->string;ConfigSetInvalidValue:s->string;ConfigSetMissingName:s->string;ConfigSetMissingNewValue:s->string;ConfigSetUsage:string;ConfigUnsetDescription:s->s->s->string;ConfigUnsetGuildNoData:string;ConfigUnsetMissingName:s->string;ConfigUnsetUsage:string;ConfigUsage:string;ConfirmationCancellation:string;ConfirmationTimeoutMessage:string;DeletedChannel:string;DeletedRole:string;DestroyConfirmationMsg:s->s->string;DestroyDescription:s->s->string;DestroySuccess:string;DestroyUsage:string;ErrorChannelDeleted:s->s->s->string;ErrorChannelNull:s->s->s->string;ErrorCmdNotFound:s->string;ErrorCmdUnknown:s->s->string;ErrorCommandNotSupportedOnLargeGuild:i->string;ErrorEmbedAuthor:string;ErrorEndHeavyTaskTriesExceeded:s->string;ErrorRoleDeleted:s->s->s->string;ErrorRoleNull:s->s->s->string;Everyone:string;HelpCommandNotFound:s->string;HelpConfiguration:string;HelpConfigurationDescription:string;HelpDescription:s->s->string;HelpEmbedDescription:s->string;HelpEmbedTitle:string;HelpGettingStarted:string;HelpJoinServerTitle:string;HelpLanguage:string;HelpLanguageDescription:string;HelpNotAvailable:string;HelpReadingTheHelp:string;HelpReadingTheHelpDescription:string;HelpSupportServer:string;HelpSupportServerDescription:s->string;HelpUsage:string;InitAlreadyInited:s->string;InitConfirmationMsg:s->s->s->string;InitDescription:s->s->string;InitUsage:string;JoinGuildMessage:s->s->s->string;JoinLargeGuildMessage:string;LangActualValue:s->string;LangDescription:s->s->string;LangNotFound:string;LangSuccess:s->string;LangUsage:string;ManageGuild:string;MathDescription:s->s->string;MathUsage:string;MuteDescription:i->s->s->string;MuteUsage:string;No:string;NumberPlaceholder:string;PingDescription:s->s->string;PingSuccess:i->string;PingUsage:string;PrefixActualValue:s->string;PrefixDescription:s->s->string;PrefixNoChange:string;PrefixNoPermission:s->string;PrefixSuccess:s->string;PrefixTooLong:i->string;PrefixUsage:string;QuestionDescription:s->s->string;QuestionTeacherCount:i->string;QuestionTeacherCountDescription:s->s->string;QuestionTeacherCountUsage:string;QuestionTeacherDescription:s->s->string;QuestionTeacherInvalidSubcommand:s->string;QuestionTeacherMissingSubcommand:s->string;QuestionTeacherNextDescription:s->s->string;QuestionTeacherNextEmbedSection:string;QuestionTeacherNextUsage:string;QuestionTeacherNoMoreQuestions:string;QuestionTeacherUsage:string;QuestionUsage:string;ResponseToDm:s->s->string;StudentsRoleName:string;Teacher:string;TeachersClassTextChannelName:string;TeachersRoleName:string;TeachersRoomCategoryName:string;TeachersRoomTextChannelName:string;Team:string;TeamsAlreadyCreated:s->string;TeamsDescription:s->s->s->string;TeamsDestroyDescription:s->s->string;TeamsDestroyTooMuchChannels:string;TeamsDestroyTooMuchRoles:string;TeamsDestroyUsage:string;TeamsInvalidSubcommand:s->string;TeamsMakeChannelsLimitHit:i->string;TeamsMakeDescription:s->s->s->string;TeamsMakeInvalidSwitchOrSize:s->string;TeamsMakeMissingCount:s->string;TeamsMakeOptions:string;TeamsMakeOutOfRange:string;TeamsMakeRolesLimitHit:i->string;TeamsMakeUsage:string;TeamsManualChannelsLimitHit:i->string;TeamsManualDescription:s->s->i->s->s->s->string;TeamsManualNoTeams:s->string;TeamsManualOptions:string;TeamsManualUsage:string;TeamsMissingSubcommand:s->string;TeamsMoveDescription:s->s->string;TeamsMoveNoChannels:s->string;TeamsMoveNoMembers:s->string;TeamsMoveUsage:string;TeamsNumberIsRight:string;TeamsReturnDescription:s->s->s->string;TeamsReturnNoChannels:s->string;TeamsReturnNoMembers:s->string;TeamsReturnUsage:string;TeamsSizeChannelsLimitHit:i->string;TeamsSizeDescription:s->s->s->string;TeamsSizeInvalidSwitchOrSize:s->string;TeamsSizeMissingSize:s->string;TeamsSizeOptions:string;TeamsSizeOutOfRange:string;TeamsSizeRolesLimitHit:i->string;TeamsSizeUsage:string;TeamsTemplate:i->string;TeamsUsage:string;Yes:string;
            //</BuilderDefinition>
        }

        static member OfStrings (l: LangStrings) =
            {
                //<BuilderConversion>
                ClassCategoryName=sprintf(Printf.StringFormat<_>l.ClassCategoryName);ClassTextChannelName=sprintf(Printf.StringFormat<_>l.ClassTextChannelName);ClassVoiceChannelName=sprintf(Printf.StringFormat<_>l.ClassVoiceChannelName);ConfigDescription=sprintf(Printf.StringFormat<_>l.ConfigDescription);ConfigGetDescription=sprintf(Printf.StringFormat<_>l.ConfigGetDescription);ConfigGetMissingName=sprintf(Printf.StringFormat<_>l.ConfigGetMissingName);ConfigGetUsage=sprintf(Printf.StringFormat<_>l.ConfigGetUsage);ConfigGetValue=sprintf(Printf.StringFormat<_>l.ConfigGetValue);ConfigGetValueNull=sprintf(Printf.StringFormat<_>l.ConfigGetValueNull);ConfigInvalidName=sprintf(Printf.StringFormat<_>l.ConfigInvalidName);ConfigInvalidSubcommand=sprintf(Printf.StringFormat<_>l.ConfigInvalidSubcommand);ConfigMissingSubcommand=sprintf(Printf.StringFormat<_>l.ConfigMissingSubcommand);ConfigNames=sprintf(Printf.StringFormat<_>l.ConfigNames);ConfigSetDescription=sprintf(Printf.StringFormat<_>l.ConfigSetDescription);ConfigSetInvalidValue=sprintf(Printf.StringFormat<_>l.ConfigSetInvalidValue);ConfigSetMissingName=sprintf(Printf.StringFormat<_>l.ConfigSetMissingName);ConfigSetMissingNewValue=sprintf(Printf.StringFormat<_>l.ConfigSetMissingNewValue);ConfigSetUsage=sprintf(Printf.StringFormat<_>l.ConfigSetUsage);ConfigUnsetDescription=sprintf(Printf.StringFormat<_>l.ConfigUnsetDescription);ConfigUnsetGuildNoData=sprintf(Printf.StringFormat<_>l.ConfigUnsetGuildNoData);ConfigUnsetMissingName=sprintf(Printf.StringFormat<_>l.ConfigUnsetMissingName);ConfigUnsetUsage=sprintf(Printf.StringFormat<_>l.ConfigUnsetUsage);ConfigUsage=sprintf(Printf.StringFormat<_>l.ConfigUsage);ConfirmationCancellation=sprintf(Printf.StringFormat<_>l.ConfirmationCancellation);ConfirmationTimeoutMessage=sprintf(Printf.StringFormat<_>l.ConfirmationTimeoutMessage);DeletedChannel=sprintf(Printf.StringFormat<_>l.DeletedChannel);DeletedRole=sprintf(Printf.StringFormat<_>l.DeletedRole);DestroyConfirmationMsg=sprintf(Printf.StringFormat<_>l.DestroyConfirmationMsg);DestroyDescription=sprintf(Printf.StringFormat<_>l.DestroyDescription);DestroySuccess=sprintf(Printf.StringFormat<_>l.DestroySuccess);DestroyUsage=sprintf(Printf.StringFormat<_>l.DestroyUsage);ErrorChannelDeleted=sprintf(Printf.StringFormat<_>l.ErrorChannelDeleted);ErrorChannelNull=sprintf(Printf.StringFormat<_>l.ErrorChannelNull);ErrorCmdNotFound=sprintf(Printf.StringFormat<_>l.ErrorCmdNotFound);ErrorCmdUnknown=sprintf(Printf.StringFormat<_>l.ErrorCmdUnknown);ErrorCommandNotSupportedOnLargeGuild=sprintf(Printf.StringFormat<_>l.ErrorCommandNotSupportedOnLargeGuild);ErrorEmbedAuthor=sprintf(Printf.StringFormat<_>l.ErrorEmbedAuthor);ErrorEndHeavyTaskTriesExceeded=sprintf(Printf.StringFormat<_>l.ErrorEndHeavyTaskTriesExceeded);ErrorRoleDeleted=sprintf(Printf.StringFormat<_>l.ErrorRoleDeleted);ErrorRoleNull=sprintf(Printf.StringFormat<_>l.ErrorRoleNull);Everyone=sprintf(Printf.StringFormat<_>l.Everyone);HelpCommandNotFound=sprintf(Printf.StringFormat<_>l.HelpCommandNotFound);HelpConfiguration=sprintf(Printf.StringFormat<_>l.HelpConfiguration);HelpConfigurationDescription=sprintf(Printf.StringFormat<_>l.HelpConfigurationDescription);HelpDescription=sprintf(Printf.StringFormat<_>l.HelpDescription);HelpEmbedDescription=sprintf(Printf.StringFormat<_>l.HelpEmbedDescription);HelpEmbedTitle=sprintf(Printf.StringFormat<_>l.HelpEmbedTitle);HelpGettingStarted=sprintf(Printf.StringFormat<_>l.HelpGettingStarted);HelpJoinServerTitle=sprintf(Printf.StringFormat<_>l.HelpJoinServerTitle);HelpLanguage=sprintf(Printf.StringFormat<_>l.HelpLanguage);HelpLanguageDescription=sprintf(Printf.StringFormat<_>l.HelpLanguageDescription);HelpNotAvailable=sprintf(Printf.StringFormat<_>l.HelpNotAvailable);HelpReadingTheHelp=sprintf(Printf.StringFormat<_>l.HelpReadingTheHelp);HelpReadingTheHelpDescription=sprintf(Printf.StringFormat<_>l.HelpReadingTheHelpDescription);HelpSupportServer=sprintf(Printf.StringFormat<_>l.HelpSupportServer);HelpSupportServerDescription=sprintf(Printf.StringFormat<_>l.HelpSupportServerDescription);HelpUsage=sprintf(Printf.StringFormat<_>l.HelpUsage);InitAlreadyInited=sprintf(Printf.StringFormat<_>l.InitAlreadyInited);InitConfirmationMsg=sprintf(Printf.StringFormat<_>l.InitConfirmationMsg);InitDescription=sprintf(Printf.StringFormat<_>l.InitDescription);InitUsage=sprintf(Printf.StringFormat<_>l.InitUsage);JoinGuildMessage=sprintf(Printf.StringFormat<_>l.JoinGuildMessage);JoinLargeGuildMessage=sprintf(Printf.StringFormat<_>l.JoinLargeGuildMessage);LangActualValue=sprintf(Printf.StringFormat<_>l.LangActualValue);LangDescription=sprintf(Printf.StringFormat<_>l.LangDescription);LangNotFound=sprintf(Printf.StringFormat<_>l.LangNotFound);LangSuccess=sprintf(Printf.StringFormat<_>l.LangSuccess);LangUsage=sprintf(Printf.StringFormat<_>l.LangUsage);ManageGuild=sprintf(Printf.StringFormat<_>l.ManageGuild);MathDescription=sprintf(Printf.StringFormat<_>l.MathDescription);MathUsage=sprintf(Printf.StringFormat<_>l.MathUsage);MuteDescription=sprintf(Printf.StringFormat<_>l.MuteDescription);MuteUsage=sprintf(Printf.StringFormat<_>l.MuteUsage);No=sprintf(Printf.StringFormat<_>l.No);NumberPlaceholder=sprintf(Printf.StringFormat<_>l.NumberPlaceholder);PingDescription=sprintf(Printf.StringFormat<_>l.PingDescription);PingSuccess=sprintf(Printf.StringFormat<_>l.PingSuccess);PingUsage=sprintf(Printf.StringFormat<_>l.PingUsage);PrefixActualValue=sprintf(Printf.StringFormat<_>l.PrefixActualValue);PrefixDescription=sprintf(Printf.StringFormat<_>l.PrefixDescription);PrefixNoChange=sprintf(Printf.StringFormat<_>l.PrefixNoChange);PrefixNoPermission=sprintf(Printf.StringFormat<_>l.PrefixNoPermission);PrefixSuccess=sprintf(Printf.StringFormat<_>l.PrefixSuccess);PrefixTooLong=sprintf(Printf.StringFormat<_>l.PrefixTooLong);PrefixUsage=sprintf(Printf.StringFormat<_>l.PrefixUsage);QuestionDescription=sprintf(Printf.StringFormat<_>l.QuestionDescription);QuestionTeacherCount=sprintf(Printf.StringFormat<_>l.QuestionTeacherCount);QuestionTeacherCountDescription=sprintf(Printf.StringFormat<_>l.QuestionTeacherCountDescription);QuestionTeacherCountUsage=sprintf(Printf.StringFormat<_>l.QuestionTeacherCountUsage);QuestionTeacherDescription=sprintf(Printf.StringFormat<_>l.QuestionTeacherDescription);QuestionTeacherInvalidSubcommand=sprintf(Printf.StringFormat<_>l.QuestionTeacherInvalidSubcommand);QuestionTeacherMissingSubcommand=sprintf(Printf.StringFormat<_>l.QuestionTeacherMissingSubcommand);QuestionTeacherNextDescription=sprintf(Printf.StringFormat<_>l.QuestionTeacherNextDescription);QuestionTeacherNextEmbedSection=sprintf(Printf.StringFormat<_>l.QuestionTeacherNextEmbedSection);QuestionTeacherNextUsage=sprintf(Printf.StringFormat<_>l.QuestionTeacherNextUsage);QuestionTeacherNoMoreQuestions=sprintf(Printf.StringFormat<_>l.QuestionTeacherNoMoreQuestions);QuestionTeacherUsage=sprintf(Printf.StringFormat<_>l.QuestionTeacherUsage);QuestionUsage=sprintf(Printf.StringFormat<_>l.QuestionUsage);ResponseToDm=sprintf(Printf.StringFormat<_>l.ResponseToDm);StudentsRoleName=sprintf(Printf.StringFormat<_>l.StudentsRoleName);Teacher=sprintf(Printf.StringFormat<_>l.Teacher);TeachersClassTextChannelName=sprintf(Printf.StringFormat<_>l.TeachersClassTextChannelName);TeachersRoleName=sprintf(Printf.StringFormat<_>l.TeachersRoleName);TeachersRoomCategoryName=sprintf(Printf.StringFormat<_>l.TeachersRoomCategoryName);TeachersRoomTextChannelName=sprintf(Printf.StringFormat<_>l.TeachersRoomTextChannelName);Team=sprintf(Printf.StringFormat<_>l.Team);TeamsAlreadyCreated=sprintf(Printf.StringFormat<_>l.TeamsAlreadyCreated);TeamsDescription=sprintf(Printf.StringFormat<_>l.TeamsDescription);TeamsDestroyDescription=sprintf(Printf.StringFormat<_>l.TeamsDestroyDescription);TeamsDestroyTooMuchChannels=sprintf(Printf.StringFormat<_>l.TeamsDestroyTooMuchChannels);TeamsDestroyTooMuchRoles=sprintf(Printf.StringFormat<_>l.TeamsDestroyTooMuchRoles);TeamsDestroyUsage=sprintf(Printf.StringFormat<_>l.TeamsDestroyUsage);TeamsInvalidSubcommand=sprintf(Printf.StringFormat<_>l.TeamsInvalidSubcommand);TeamsMakeChannelsLimitHit=sprintf(Printf.StringFormat<_>l.TeamsMakeChannelsLimitHit);TeamsMakeDescription=sprintf(Printf.StringFormat<_>l.TeamsMakeDescription);TeamsMakeInvalidSwitchOrSize=sprintf(Printf.StringFormat<_>l.TeamsMakeInvalidSwitchOrSize);TeamsMakeMissingCount=sprintf(Printf.StringFormat<_>l.TeamsMakeMissingCount);TeamsMakeOptions=sprintf(Printf.StringFormat<_>l.TeamsMakeOptions);TeamsMakeOutOfRange=sprintf(Printf.StringFormat<_>l.TeamsMakeOutOfRange);TeamsMakeRolesLimitHit=sprintf(Printf.StringFormat<_>l.TeamsMakeRolesLimitHit);TeamsMakeUsage=sprintf(Printf.StringFormat<_>l.TeamsMakeUsage);TeamsManualChannelsLimitHit=sprintf(Printf.StringFormat<_>l.TeamsManualChannelsLimitHit);TeamsManualDescription=sprintf(Printf.StringFormat<_>l.TeamsManualDescription);TeamsManualNoTeams=sprintf(Printf.StringFormat<_>l.TeamsManualNoTeams);TeamsManualOptions=sprintf(Printf.StringFormat<_>l.TeamsManualOptions);TeamsManualUsage=sprintf(Printf.StringFormat<_>l.TeamsManualUsage);TeamsMissingSubcommand=sprintf(Printf.StringFormat<_>l.TeamsMissingSubcommand);TeamsMoveDescription=sprintf(Printf.StringFormat<_>l.TeamsMoveDescription);TeamsMoveNoChannels=sprintf(Printf.StringFormat<_>l.TeamsMoveNoChannels);TeamsMoveNoMembers=sprintf(Printf.StringFormat<_>l.TeamsMoveNoMembers);TeamsMoveUsage=sprintf(Printf.StringFormat<_>l.TeamsMoveUsage);TeamsNumberIsRight=sprintf(Printf.StringFormat<_>l.TeamsNumberIsRight);TeamsReturnDescription=sprintf(Printf.StringFormat<_>l.TeamsReturnDescription);TeamsReturnNoChannels=sprintf(Printf.StringFormat<_>l.TeamsReturnNoChannels);TeamsReturnNoMembers=sprintf(Printf.StringFormat<_>l.TeamsReturnNoMembers);TeamsReturnUsage=sprintf(Printf.StringFormat<_>l.TeamsReturnUsage);TeamsSizeChannelsLimitHit=sprintf(Printf.StringFormat<_>l.TeamsSizeChannelsLimitHit);TeamsSizeDescription=sprintf(Printf.StringFormat<_>l.TeamsSizeDescription);TeamsSizeInvalidSwitchOrSize=sprintf(Printf.StringFormat<_>l.TeamsSizeInvalidSwitchOrSize);TeamsSizeMissingSize=sprintf(Printf.StringFormat<_>l.TeamsSizeMissingSize);TeamsSizeOptions=sprintf(Printf.StringFormat<_>l.TeamsSizeOptions);TeamsSizeOutOfRange=sprintf(Printf.StringFormat<_>l.TeamsSizeOutOfRange);TeamsSizeRolesLimitHit=sprintf(Printf.StringFormat<_>l.TeamsSizeRolesLimitHit);TeamsSizeUsage=sprintf(Printf.StringFormat<_>l.TeamsSizeUsage);TeamsTemplate=sprintf(Printf.StringFormat<_>l.TeamsTemplate);TeamsUsage=sprintf(Printf.StringFormat<_>l.TeamsUsage);Yes=sprintf(Printf.StringFormat<_>l.Yes);
                //</BuilderConversion>
            }

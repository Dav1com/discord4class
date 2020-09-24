namespace Discord4Class.Lang

open System.Runtime
open System.Reflection
open FSharp.Core
open FSharp.Reflection
open FSharp.Quotations
open FSharp.Quotations.Evaluator.QuotationEvaluationExtensions
open Discord4Class.Helpers.TypeShorcuts

// It would be nice to have a better way of doing this
module Types =

    type LangStrings =
      { //<StringsDefinition>
        ErrorCmdNotFound : string
        ErrorCmdUnknown : string
        ErrorEmbedAuthor : string
        JoinGuildMessage : string
        LangMissingArg : string
        LangNotFound : string
        LangSuccess : string
        PingSuccess : string
        ResponseToDm : string
        //</StringsDefinition>
        }

        // While reading the file, the backslashes are scaped
        member this.SanitizeBackslashes () =
            FSharpValue.GetRecordFields this
            |> Array.map (fun s -> s.ToString().Replace("\\n", "\n") :> obj)
            |> fun a -> FSharpValue.MakeRecord(typeof<LangStrings>, a) :?> LangStrings

    type LangBuilders =
      { //<BuilderDefinition>
        ErrorCmdNotFound : s -> string
        ErrorCmdUnknown : s -> s -> string
        ErrorEmbedAuthor : string
        JoinGuildMessage : s -> s -> s -> string
        LangMissingArg : s -> s -> string
        LangNotFound : s -> string
        LangSuccess : string
        PingSuccess : i -> string
        ResponseToDm : s -> s -> string
        //</BuilderDefinition>
        }

        static member OfStrings (l : LangStrings) =
            {
                //<BuilderConversion>
                ErrorCmdNotFound = sprintf (Printf.StringFormat<_> l.ErrorCmdNotFound)
                ErrorCmdUnknown = sprintf (Printf.StringFormat<_> l.ErrorCmdUnknown)
                ErrorEmbedAuthor = sprintf (Printf.StringFormat<_> l.ErrorEmbedAuthor)
                JoinGuildMessage = sprintf (Printf.StringFormat<_> l.JoinGuildMessage)
                LangMissingArg = sprintf (Printf.StringFormat<_> l.LangMissingArg)
                LangNotFound = sprintf (Printf.StringFormat<_> l.LangNotFound)
                LangSuccess = sprintf (Printf.StringFormat<_> l.LangSuccess)
                PingSuccess = sprintf (Printf.StringFormat<_> l.PingSuccess)
                ResponseToDm = sprintf (Printf.StringFormat<_> l.ResponseToDm)
                //</BuilderConversion>
            }

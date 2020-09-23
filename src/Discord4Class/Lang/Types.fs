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
        ErrorCmdNotFound : s -> s -> string
        ErrorCmdUnknown : s -> s -> s -> string
        ErrorEmbedAuthor : string
        JoinGuildMessage : s -> s -> s -> string
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
                PingSuccess = sprintf (Printf.StringFormat<_> l.PingSuccess)
                ResponseToDm = sprintf (Printf.StringFormat<_> l.ResponseToDm)
                //</BuilderConversion>
            }

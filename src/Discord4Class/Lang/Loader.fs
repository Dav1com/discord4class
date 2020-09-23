namespace Discord4Class.Lang

open System.IO
open Microsoft.Extensions.Configuration
open FsConfig
open Discord4Class.Lang.Types

module Loader =

    let private loadLangFileAsync lang (fullPath : string) = async {
        let c =
            ConfigurationBuilder()
                .AddIniFile(fullPath)
                .Build()
        let ac = AppConfig c
        return (lang, ac.Get<LangStrings> ())
    }

    let loadLangFiles path =
        Directory.GetFiles(Directory.GetCurrentDirectory(), path + "*.ini")
        |> Array.map (
            fun f -> loadLangFileAsync (Path.GetFileNameWithoutExtension f) (Path.GetFullPath f)
        )
        |> Async.Parallel
        |> Async.RunSynchronously
        |> Array.filter (fun (_,r) ->
            match r with
            | Ok _ -> true
            | Error _ -> false
        )
        // TODO: remove the warning
        |> Array.map (fun (s, Ok l) ->
            (s, LangBuilders.OfStrings (l.SanitizeBackslashes()) )
        )
        |> Map.ofSeq

    let getLangOrDefault def (map : Map<string, LangBuilders>) key =
        map
        |> Map.tryFind key
        |> function
            | Some l -> l
            | None -> map.[def]

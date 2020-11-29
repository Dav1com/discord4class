namespace Discord4Class.Lang

open System.IO
open Microsoft.Extensions.Configuration
open FsConfig
open Discord4Class.Lang.Types

module Loader =

    let private loadLangFileAsync (lang : string) (fullPath : string) = async {
        let c =
            ConfigurationBuilder()
                .AddIniFile(fullPath)
                .Build()
        let ac = AppConfig c
        return (lang.ToLower(), ac.Get<LangStrings> ())
    }

    let loadLangFiles path =
        Directory.GetFiles(Directory.GetCurrentDirectory(), path + "*.ini")
        |> Array.map (
            fun f -> loadLangFileAsync (Path.GetFileNameWithoutExtension f) (Path.GetFullPath f)
        )
        |> Async.Parallel
        |> Async.RunSynchronously
        // TODO: remove the warning
        |> Array.map (fun (s, la) ->
            match la with
            | Ok l ->
                ( s, LangBuilders.OfStrings <| l.SanitizeBackslashes() )
            | Error l ->
                printfn "Error while loading lang file '%s': %A" s l
                exit(-1) )
        |> Map.ofSeq

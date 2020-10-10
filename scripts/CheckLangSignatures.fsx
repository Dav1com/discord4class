open System
open System.IO
open System.Text.RegularExpressions

printf "Checking lang strings signatures integrity... "

let LangFolder = __SOURCE_DIRECTORY__ + "/../res/lang/"
let BaseFile = "en-US.ini"

let readLines (filePath:string) = seq {
    use sr = new StreamReader (filePath)
    while not sr.EndOfStream do
        yield sr.ReadLine ()
}

let signature str =
    str
    |> (Regex "%[A-z]").Matches
    |> List.ofSeq
    |> List.map (fun m -> m.Value.[1])

let getLangSignatures path =
    readLines path
    |> Seq.map (fun s ->
        (s.Split "=")
        |> Array.map (fun s2 -> s2.Trim())
        |> fun a -> (a.[0], signature a.[1])
    )
    |> Map.ofSeq

let LangSignatures = getLangSignatures <| LangFolder + BaseFile

type Errors =
    | MissingString of string * string // LangFileName * MissingStringName
    | SignatureNotMatch of string * string * string // LangFileName * ErroredString * GivenSignature
    | NoError

System.IO.Directory.GetFiles(LangFolder, "*.ini")
|> Array.filter (fun s -> Path.GetFileName s <> LangFolder + BaseFile)
|> Array.fold (fun acc s ->
    let thisFileSignatures = getLangSignatures s
    LangSignatures
    |> Map.map (fun k v ->
        thisFileSignatures.TryFind k
        |> function
            | Some sign ->
                if sign <> v then
                    sign
                    |> List.map (fun c -> c.ToString())
                    |> String.concat " "
                    |> fun l -> SignatureNotMatch (Path.GetFileNameWithoutExtension s, k, l)
                else
                    NoError
            | None -> MissingString (Path.GetFileNameWithoutExtension s, k)
    )
    |> Map.toList
    |> List.map (fun (a,b) -> b)
    |> List.filter (function
        | NoError -> false
        | _ -> true
    )
    |> List.append acc
) []
|> function
    | [] -> printfn "OK"
    | errors ->
        printfn "Errors Found:"
        errors
        |> List.iter (function
            | MissingString (file, str) ->
                eprintfn "  Missing string %s on file %s" str file
            | SignatureNotMatch (file, str, given) ->
                let expected =
                    LangSignatures.[str]
                    |> List.map (fun c -> c.ToString())
                    |> String.concat " "
                eprintfn "  Signature dont match on %s on file %s, expected '%s' but given '%s'" str file expected given
            | NoError -> ()
        )
        exit(-1) // stops build

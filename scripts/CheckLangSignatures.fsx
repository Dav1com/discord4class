open System
open System.IO
open System.Text.RegularExpressions

printf "Checking lang strings signatures integrity... "

let LangFolder = __SOURCE_DIRECTORY__ + "/../res/lang/"
let BaseFile = "en-us.ini"

let readLines (filePath: string) = seq {
    use sr = new StreamReader(filePath)
    while not sr.EndOfStream do
        sr.ReadLine()
}

let signature str =
    str
    |> (Regex "%[A-z]").Matches
    |> List.ofSeq
    |> List.map (fun m -> m.Value.[1])

let getLangSignatures path =
    readLines path
    |> Seq.choose (fun s ->
        if s.IndexOf "=" = -1 then None
        else
            s.Split "="
            |> Array.map (fun s2 -> s2.Trim())
            |> fun a -> Some (a.[0], signature a.[1]) )
    |> List.ofSeq

let LangSignatures = getLangSignatures <| LangFolder + BaseFile

type Errors =
    | MissingString of LangFileName: string * MissingStringName: string
    | SignatureNotMatch of LangFileName: string * ErroredString: string * GivenSignature: string
    | DuplicatedKey of LangFileName: string * DuplicatedKeyName: string * Count: int
    | NoError

System.IO.Directory.GetFiles(LangFolder, "*.ini")
|> Array.fold (fun acc s ->
    let thisFileSignatures = getLangSignatures s
    LangSignatures
    |> List.map (fun (k, v) ->
        List.tryFind (fun (k2,_) -> k = k2) thisFileSignatures
        |> function
            | Some (_,sign) when sign <> v ->
                sign
                |> List.map (fun c -> c.ToString())
                |> String.concat " "
                |> fun l -> SignatureNotMatch (Path.GetFileNameWithoutExtension s, k, l)
            | Some _ -> NoError
            | None -> MissingString (Path.GetFileNameWithoutExtension s, k) )
    |> List.filter ((<>) NoError)
    |> List.append (
        thisFileSignatures
        |> List.groupBy (fun (key, value) -> key)
        |> List.choose (fun (key, l) ->
            if l.Length = 1 then None
            else Some (DuplicatedKey (Path.GetFileNameWithoutExtension s, key, l.Length) )) )
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
                    List.find (fun (s, _) -> s = str) LangSignatures
                    |> fun (_, signature) -> signature
                    |> List.map (fun c -> c.ToString())
                    |> String.concat " "
                eprintfn "  Signature dont match on %s on file %s, expected '%s' but given '%s'" str file expected given
            | DuplicatedKey (file, str, count) ->
                eprintfn "  Duplicated key %s on file %s, %i times" str file count
            | NoError -> () )
        exit(-1) // stops build

open System
open System.IO
open System.Text.RegularExpressions

printf "Generating Lang Types... "

let BaseFilePath = __SOURCE_DIRECTORY__ + "/../res/lang/en-US.ini"
let TargetFilePath = __SOURCE_DIRECTORY__ + "/../src/Discord4Class/Lang/Types.fs"
let StringsStartTag = "//<StringsDefinition>"
let StringsEndTag = "//</StringsDefinition>"
let BuildersStartTag = "//<BuilderDefinition>"
let BuilderEndTag = "//</BuilderDefinition>"
let ConversionStartTag = "//<BuilderConversion>"
let ConversionEndTag = "//</BuilderConversion>"

let readLines (filePath:string) = seq {
    use sr = new StreamReader (filePath)
    while not sr.EndOfStream do
        yield sr.ReadLine ()
}

let KeysValues =
    readLines BaseFilePath
    |> Seq.map (fun s ->
        (s.Split "=")
        |> Array.map (fun s2 -> s2.Trim())
        |> fun a -> (a.[0], a.[1])
    )
    |> Map.ofSeq

let TargetFile = readLines TargetFilePath |> List.ofSeq

type FileSearchResult =
  { Column : int
    Line : int}

let findLineAndColumn (lines : string list) (str : string) =
    let line =
        lines
        |> List.findIndex (fun s -> s.Contains str)
    let column = lines.[line].IndexOf str
    {
        Column = column
        Line = line
    }

let mutable result = []
let mutable searchResult = findLineAndColumn TargetFile StringsStartTag

result <- List.append result (TargetFile.GetSlice(Some 0, Some searchResult.Line))

result <-
    List.append result [
        for KeyValue (k,v) in KeysValues do
            let s = String.replicate (searchResult.Column) " "
            yield s + k + ":string"
    ]

let mutable searchResultEnd = findLineAndColumn TargetFile StringsEndTag
searchResult <- findLineAndColumn TargetFile BuildersStartTag

result <- List.append result (TargetFile.GetSlice(Some searchResultEnd.Line, Some searchResult.Line))

let signature str =
    (str
    |> (Regex "%[A-z]").Matches
    |> List.ofSeq
    |> List.map (fun x -> x.Value.[1].ToString() + "->" )
    |> String.Concat)

result <-
    List.append result [
        (String.replicate (searchResult.Column) " ") + (
            [
                for KeyValue (k,v) in KeysValues do
                    yield k + ":" + (signature v) + "string;"
            ] |> String.concat ""
        )
    ]

searchResultEnd <- findLineAndColumn TargetFile BuilderEndTag
searchResult <- findLineAndColumn TargetFile ConversionStartTag

result <- List.append result (TargetFile.GetSlice(Some searchResultEnd.Line, Some searchResult.Line))

result <-
    List.append result [
        (String.replicate (searchResult.Column) " ") + (
            [
                for KeyValue (k,v) in KeysValues do
                    yield k + "=sprintf(Printf.StringFormat<_>l." + k + ");"
            ] |> String.concat ""
        )
    ]

searchResultEnd <- findLineAndColumn TargetFile ConversionEndTag

result <- List.append result (TargetFile.GetSlice(Some searchResultEnd.Line, None))

File.Delete TargetFilePath

File.WriteAllLines(TargetFilePath, result)

printfn "OK"

namespace Discord4Class.Helpers

open System

module String =

    let isNumeric =
        String.forall (fun c -> '0' <= c && c <= '9')

    let isSnakeCase =
        String.forall (fun c -> ('a' <= c && c <= 'z') || c = '_')

    let (|IntParse|_|) (s: string) =
        match Int32.TryParse s with
        | (false, _) -> None
        | (true, i) -> Some i

    let (|Int64Parse|_|) (s: string) =
        match Int64.TryParse s with
        | (false, _) -> None
        | (true, i) -> Some i

    let (|UInt64Parse|_|) (s: string) =
        match UInt64.TryParse s with
        | (false, _) -> None
        | (true, i) -> Some i

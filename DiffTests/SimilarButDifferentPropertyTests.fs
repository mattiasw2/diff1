module SimilarButDifferentPropertyTests

open CompareFiles

open FsCheck
open FsCheck.Xunit
open FsCheckAddons

type SimilarButDifferentProperties() =
    do Arb.register<NonNullString>() |> ignore // Register the non-null generator

    [<Property>]
    member _.``Not Identical Property`` (line1: string, line2: string) =
        if CompareFiles.similarButDifferent line1 line2 then
            line1 <> line2
        else
            true // No violation

    [<Property>]
    member _.``Symmetric Property`` (line1: string, line2: string) =
        CompareFiles.similarButDifferent line1 line2 = CompareFiles.similarButDifferent line2 line1

    [<Property>]
    member _.``Extend Lines Property`` (line1: string, line2: string, suffix: string) =
        if line1.Length < 10 || line2.Length < 10 then true
        else
            let maxSuffixLength = max 1 (int (0.2 * float (min line1.Length line2.Length)))
            let truncatedSuffix = if suffix.Length > maxSuffixLength then suffix.Substring(0, maxSuffixLength) else suffix
            let result = CompareFiles.similarButDifferent line1 line2
            let extendedResult = CompareFiles.similarButDifferent (line1 + truncatedSuffix) (line2 + truncatedSuffix)
            result = extendedResult

    [<Property>]
    member _.``Identical Lines Always False Property`` (line: string) =
        not (CompareFiles.similarButDifferent line line)

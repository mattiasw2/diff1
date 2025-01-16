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

    // ("{cbaaaaaaa", "cb{\029aaaaaa", "WMa")

    [<Property>]
    member _.``If the shorter strings are similar, the same made longer, should still be similar`` (line1: string, line2: string, suffix: string) =
        (line1.Length >= 10 && line2.Length >= 10 && suffix.Length > 2) ==>
        lazy (
            let maxSuffixLength = max 1 (int (0.2 * float (min line1.Length line2.Length)))
            let truncatedSuffix = if suffix.Length > maxSuffixLength then suffix.Substring(0, maxSuffixLength) else suffix
            let result = CompareFiles.similarButDifferent line1 line2
            let extendedResult = CompareFiles.similarButDifferent (line1 + truncatedSuffix) (line2 + truncatedSuffix)
            if result then extendedResult = true else true
        )

    [<Property>]
    member _.``Identical Lines Always False Property`` (line: string) =
        not (CompareFiles.similarButDifferent line line)

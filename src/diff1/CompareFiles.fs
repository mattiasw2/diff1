module CompareFiles

open System
open System.IO

/// Determines whether two lines are similar but different based on the following criteria:
/// 1. Similarity: The lines must have at least `X` consecutive identical characters.
/// 2. Difference: The lines must have at least `Y` consecutive differing characters.
///
/// Parameters:
/// - line1: The first line to compare.
/// - line2: The second line to compare.
///
/// Returns:
/// - true if the lines are similar but different.
/// - false otherwise.
let similarButDifferent (line1: string) (line2: string) : bool =
    // Handle empty strings and single characters
    if String.IsNullOrEmpty(line1) || String.IsNullOrEmpty(line2) || 
       line1.Length < 2 || line2.Length < 2 then
        false
    else
        // Split into characters (handling Unicode properly)
        let chars1 = line1.EnumerateRunes() |> Seq.map (fun r -> r.ToString()) |> Seq.toList
        let chars2 = line2.EnumerateRunes() |> Seq.map (fun r -> r.ToString()) |> Seq.toList
        let runeLen1 = chars1.Length
        let runeLen2 = chars2.Length
        let minRuneLength = min runeLen1 runeLen2
        
        // Calculate common prefix and suffix using runes
        let mutable prefixLen = 0
        while prefixLen < minRuneLength && chars1.[prefixLen] = chars2.[prefixLen] do
            prefixLen <- prefixLen + 1

        let mutable suffixLen = 0
        while suffixLen < minRuneLength - prefixLen && 
              chars1.[runeLen1 - 1 - suffixLen] = chars2.[runeLen2 - 1 - suffixLen] do
            suffixLen <- suffixLen + 1

        // Also check for character frequency similarity
        let charSet1 = Set.ofList chars1
        let charSet2 = Set.ofList chars2
        let commonChars = Set.intersect charSet1 charSet2 |> Set.count
        
        // Calculate the minimum required identical characters based on string length
        let minIdentical = 
            if minRuneLength <= 4 then 2
            elif minRuneLength <= 10 then 3
            else minRuneLength / 3  // For longer strings, require 33% similarity

        // Calculate the minimum required different characters
        let minDifferent = 
            if minRuneLength <= 4 then 1
            elif minRuneLength <= 10 then 2
            else 3

        // The common parts count towards identical characters
        let commonCount = max (prefixLen + suffixLen) commonChars

        // Additional check for length difference
        let lengthDiffOk = float (max line1.Length line2.Length - min line1.Length line2.Length) <= float (max line1.Length line2.Length) * 0.5

        // Check for significant reordering
        let reordered = commonChars > minRuneLength / 2 && prefixLen + suffixLen < minRuneLength / 3

        // Special case for single character difference at the end
        let singleCharDiff = 
            abs(runeLen1 - runeLen2) <= 1 && 
            (prefixLen >= minRuneLength - 1 || suffixLen >= minRuneLength - 1)

        // Special case for emoji or punctuation change
        let singleTokenDiff =
            prefixLen + suffixLen >= minRuneLength - 1 &&
            abs(runeLen1 - runeLen2) <= 1

        // Ensure the lines are not identical and meet criteria
        line1 <> line2 && 
        (lengthDiffOk || reordered || singleCharDiff || singleTokenDiff) && 
        (commonCount >= minIdentical || singleTokenDiff) && 
        ((max line1.Length line2.Length - min line1.Length line2.Length + (minRuneLength - commonCount)) >= minDifferent || singleTokenDiff)

// Function to compute the Longest Common Subsequence (LCS)
// Optimized LCS using Dynamic Programming
let lcs (xs: 'a list) (ys: 'a list) : 'a list =
    let m = xs.Length
    let n = ys.Length
    let dp = Array2D.create (m + 1) (n + 1) 0

    for i in 1 .. m do
        for j in 1 .. n do
            if xs.[i - 1] = ys.[j - 1] then
                dp.[i, j] <- dp.[i - 1, j - 1] + 1
            else
                dp.[i, j] <- max dp.[i - 1, j] dp.[i, j - 1]

    let rec backtrack (i: int) (j: int) (acc: 'a list) : 'a list =
        if i = 0 || j = 0 then acc
        elif xs.[i - 1] = ys.[j - 1] then
            backtrack (i - 1) (j - 1) (xs.[i - 1] :: acc)
        elif dp.[i - 1, j] > dp.[i, j - 1] then
            backtrack (i - 1) j acc
        else
            backtrack i (j - 1) acc

    backtrack m n []

type DiffPart =
    | Common of string
    | Difference of string * string

let highlightDifferences (line1: string) (line2: string) : string =
    let maxLength = max line1.Length line2.Length
    
    // Convert to rune arrays for proper Unicode handling
    let runes1 = line1.EnumerateRunes() |> Seq.map (fun r -> r.ToString()) |> Seq.toArray
    let runes2 = line2.EnumerateRunes() |> Seq.map (fun r -> r.ToString()) |> Seq.toArray
    let runeLen1 = runes1.Length
    let runeLen2 = runes2.Length
    
    // Identify initial differences
    let rec identifyDiffs (i: int) (acc: DiffPart list) (current1: string) (current2: string) : DiffPart list =
        if i >= max runeLen1 runeLen2 then
            if current1 <> "" || current2 <> "" then
                acc @ [Difference(current1, current2)]
            else acc
        else
            let c1 = if i < runeLen1 then runes1.[i] else ""
            let c2 = if i < runeLen2 then runes2.[i] else ""
            
            if c1 = c2 then
                let baseAcc =
                    if current1 <> "" || current2 <> "" then
                        acc @ [Difference(current1, current2)]
                    else acc
                
                match List.tryLast baseAcc with
                | Some(Common s) ->
                    let init = List.take (List.length baseAcc - 1) baseAcc
                    identifyDiffs (i + 1) (init @ [Common (s + c1)]) "" ""
                | _ ->
                    identifyDiffs (i + 1) (baseAcc @ [Common c1]) "" ""
            else
                identifyDiffs (i + 1) acc (current1 + c1) (current2 + c2)

    // Helper function to calculate prefix length
    let findPrefixLength (s1: string[]) (s2: string[]) : int =
        let mutable i = 0
        let len = min s1.Length s2.Length
        while i < len && s1.[i] = s2.[i] do
            i <- i + 1
        i

    // Helper function to calculate suffix length
    let findSuffixLength (s1: string[]) (s2: string[]) : int =
        let mutable i = 0
        let len = min s1.Length s2.Length
        while i < len && s1.[s1.Length - 1 - i] = s2.[s2.Length - 1 - i] do
            i <- i + 1
        i

    // Post-process each Difference part to split into smaller parts
    let rec splitDifferences (parts: DiffPart list) : DiffPart list =
        let rec splitDifferencePart (d1: string) (d2: string) : DiffPart list =
            let d1Runes = d1.EnumerateRunes() |> Seq.map (fun r -> r.ToString()) |> Seq.toArray
            let d2Runes = d2.EnumerateRunes() |> Seq.map (fun r -> r.ToString()) |> Seq.toArray
            
            let prefixLen = findPrefixLength d1Runes d2Runes
            let suffixLen = findSuffixLength d1Runes d2Runes
            
            let diff1 = d1Runes |> Array.skip prefixLen |> Array.take (d1Runes.Length - prefixLen - suffixLen) |> String.concat ""
            let diff2 = d2Runes |> Array.skip prefixLen |> Array.take (d2Runes.Length - prefixLen - suffixLen) |> String.concat ""
            let commonPrefix = d1Runes |> Array.take prefixLen |> String.concat ""
            let commonSuffix = d1Runes |> Array.skip (d1Runes.Length - suffixLen) |> String.concat ""

            let parts = []
            let parts = if commonPrefix <> "" then parts @ [Common commonPrefix] else parts
            let parts = if diff1 <> "" || diff2 <> "" then parts @ [Difference(diff1, diff2)] else parts
            let parts = if commonSuffix <> "" then parts @ [Common commonSuffix] else parts
            parts
        
        match parts with
        | [] -> []
        | Difference(d1, d2) :: rest -> splitDifferencePart d1 d2 @ splitDifferences rest
        | part :: rest -> part :: splitDifferences rest

    // Render output based on parts
    let rec render (acc: string) (diffs: DiffPart list) : string =
        match diffs with
        | [] -> acc
        | Common s :: rest -> render (acc + s) rest
        | Difference (d1, d2) :: rest -> render (acc + $"({d1}|{d2})") rest

    // Process input
    let diffs = identifyDiffs 0 [] "" ""
    let refinedDiffs = splitDifferences diffs
    render "" refinedDiffs

// Function to compare two files using LCS
let compareFiles (file1: string) (file2: string) : unit =
    let lines1 = File.ReadAllLines(file1) |> Array.toList
    let lines2 = File.ReadAllLines(file2) |> Array.toList
    let common = lcs lines1 lines2

    let rec diff (xs: string list) (ys: string list) (common: string list) (acc: string list) : string list =
        match xs, ys, common with
        | [], [], [] -> List.rev acc // All lists are empty
        | [], [], _ -> List.rev acc // xs and ys are empty, common may have elements
        | [], y::ytail, _ -> // xs is empty, handle added lines
            diff [] ytail common ((sprintf "+ %s" y) :: acc)
        | x::xtail, [], _ -> // ys is empty, handle removed lines
            diff xtail [] common ((sprintf "- %s" x) :: acc)
        | x::xtail, y::ytail, [] -> // common is empty, handle remaining lines
            if x = y then
                diff xtail ytail [] ((sprintf "  %s" x) :: acc)
            else if similarButDifferent x y then
                let highlighted = highlightDifferences x y
                diff xtail ytail [] ((sprintf "~ %s" highlighted) :: acc)
            else
                // Treat as removed and added
                diff xtail ytail [] ((sprintf "- %s" x) :: (sprintf "+ %s" y) :: acc)
        | x::xtail, y::ytail, c::ctail when x = c && y = c -> // Lines match common
            diff xtail ytail ctail ((sprintf "  %s" x) :: acc)
        | x::xtail, _, c::ctail when x = c -> // Line in xs matches common
            diff xtail ys ctail ((sprintf "- %s" x) :: acc)
        | _, y::ytail, c::ctail when y = c -> // Line in ys matches common
            diff xs ytail ctail ((sprintf "+ %s" y) :: acc)
        | x::xtail, y::ytail, c::ctail -> // Handle modified lines
            if x = y then
                diff xtail ytail ctail ((sprintf "  %s" x) :: acc)
            else if similarButDifferent x y then
                let highlighted = highlightDifferences x y
                diff xtail ytail ctail ((sprintf "~ %s" highlighted) :: acc)
            else
                // Treat as removed and added
                diff xtail ytail ctail ((sprintf "- %s" x) :: (sprintf "+ %s" y) :: acc)

    let differences = diff lines1 lines2 common []
    differences |> List.iter (printfn "%s")

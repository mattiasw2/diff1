module CompareFiles

open System
open System.IO
open System.Globalization
open StringHelper

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

/// Highlights the differences between two similar strings by wrapping the differing parts in parentheses
/// For example: "hello world" and "hello there" becomes "hello (world|there)"
let highlightDifferences (line1: string) (line2: string) : string =
    if line1 = line2 then
        line1  // Return original line if strings are identical
    else
        let elements1 = StringHelper.getTextElements line1
        let elements2 = StringHelper.getTextElements line2
        
        // Find common prefix length in text elements
        let rec findPrefixLen i =
            if i >= elements1.Length || i >= elements2.Length then i
            elif elements1.[i] = elements2.[i] then findPrefixLen (i + 1)
            else i
        let prefixElemCount = findPrefixLen 0
        let prefix = String.Join("", elements1 |> Array.take prefixElemCount)
        
        // Find common suffix length in text elements
        let rec findSuffixLen i =
            let i1 = elements1.Length - 1 - i
            let i2 = elements2.Length - 1 - i
            if i1 < prefixElemCount || i2 < prefixElemCount then i
            elif elements1.[i1] = elements2.[i2] then findSuffixLen (i + 1)
            else i
        let suffixElemCount = findSuffixLen 0
        
        // Extract differing parts
        let diff1 = 
            if elements1.Length - prefixElemCount - suffixElemCount > 0 then
                String.Join("", elements1 |> Array.skip prefixElemCount |> Array.take (elements1.Length - prefixElemCount - suffixElemCount))
            else ""
        let diff2 = 
            if elements2.Length - prefixElemCount - suffixElemCount > 0 then
                String.Join("", elements2 |> Array.skip prefixElemCount |> Array.take (elements2.Length - prefixElemCount - suffixElemCount))
            else ""
        
        let suffix = 
            if suffixElemCount = 0 then ""
            else String.Join("", elements1 |> Array.skip (elements1.Length - suffixElemCount))
        
        // If the differences are long and contain common substrings, try to break them down further
        if diff1.Length > 3 && diff2.Length > 3 then
            let (innerPrefix, innerDiff1, innerDiff2) = StringHelper.extractDifferences diff1 diff2
            prefix + "(" + innerDiff1 + "|" + innerDiff2 + ")" + suffix
        else
            prefix + "(" + diff1 + "|" + diff2 + ")" + suffix

/// Compare two files and print their differences
let compareFiles (file1: string) (file2: string) : unit =
    let lines1 = File.ReadAllLines(file1)  // Let exceptions propagate
    let lines2 = File.ReadAllLines(file2)
    
    let common = lcs (List.ofArray lines1) (List.ofArray lines2)
    
    let rec processLines (lines1: string list) (lines2: string list) (common: string list) =
        match lines1, lines2, common with
        | [], [], [] -> ()  // Done processing
        | l1::rest1, l2::rest2, c::restCommon when l1 = c && l2 = c ->
            printfn "  %s" l1  // Common line
            processLines rest1 rest2 restCommon
        | l1::rest1, l2::rest2, _ when similarButDifferent l1 l2 ->
            printfn "~ %s" (highlightDifferences l1 l2)  // Similar lines
            processLines rest1 rest2 common
        | l1::rest1, l2::rest2, _ ->
            // Different lines - show added line first, then removed
            printfn "+ %s" l2  // Line added
            printfn "- %s" l1  // Line removed
            processLines rest1 rest2 common
        | [], l2::rest2, _ ->
            printfn "+ %s" l2  // Line added
            processLines [] rest2 common
        | l1::rest1, [], _ ->
            printfn "- %s" l1  // Line removed
            processLines rest1 [] common
        | _, _, _ -> ()  // Should not happen

    processLines (List.ofArray lines1) (List.ofArray lines2) common

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
let similarButDifferent (line1: string) (line2: string) =
    let minIdentical = (min line1.Length line2.Length) / 2 
    let minDifferent = 1 // At least 1 differing character

    // Convert lines to sets of characters
    let charSet1 = Set.ofSeq line1
    let charSet2 = Set.ofSeq line2

    // Count the number of identical characters (regardless of position)
    let identicalCount = Set.intersect charSet1 charSet2 |> Set.count

    // Count the number of differing characters
    let differentCount = (Set.union charSet1 charSet2 |> Set.count) - identicalCount

    // Ensure the lines are not identical
    if line1 = line2 then
        false
    else
        // Check if the lines meet the similarity and difference criteria
        identicalCount >= minIdentical && differentCount >= minDifferent

// Function to compute the Longest Common Subsequence (LCS)
// Optimized LCS using Dynamic Programming
let lcs (xs: 'a list) (ys: 'a list) =
    let m = xs.Length
    let n = ys.Length
    let dp = Array2D.create (m + 1) (n + 1) 0

    for i in 1 .. m do
        for j in 1 .. n do
            if xs.[i - 1] = ys.[j - 1] then
                dp.[i, j] <- dp.[i - 1, j - 1] + 1
            else
                dp.[i, j] <- max dp.[i - 1, j] dp.[i, j - 1]

    let rec backtrack i j acc =
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

let highlightDifferences (line1: string) (line2: string) =
    let maxLength = max line1.Length line2.Length
    
    // Identify initial differences
    let rec identifyDiffs i acc current1 current2 =
        if i >= maxLength then
            if current1 <> "" || current2 <> "" then
                acc @ [Difference(current1, current2)]
            else acc
        else
            let c1 = if i < line1.Length then line1.[i].ToString() else ""
            let c2 = if i < line2.Length then line2.[i].ToString() else ""
            
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
    let findPrefixLength (s1: string) (s2: string) : int =
        let mutable i = 0
        let len = min s1.Length s2.Length
        while i < len && s1.[i] = s2.[i] do
            i <- i + 1
        i

    // Helper function to calculate suffix length
    let findSuffixLength (s1: string) (s2: string) : int =
        let mutable i = 0
        let len = min s1.Length s2.Length
        while i < len && s1.[s1.Length - 1 - i] = s2.[s2.Length - 1 - i] do
            i <- i + 1
        i

    // Post-process each Difference part to split into smaller parts
    let rec splitDifferences parts =
        let rec splitDifferencePart (d1: string) (d2: string) : DiffPart list =
            let prefixLen = findPrefixLength d1 d2
            let suffixLen = findSuffixLength d1 d2
            let diff1 = d1.Substring(prefixLen, d1.Length - prefixLen - suffixLen)
            let diff2 = d2.Substring(prefixLen, d2.Length - prefixLen - suffixLen)
            let commonPrefix = d1.Substring(0, prefixLen)
            let commonSuffix = d1.Substring(d1.Length - suffixLen)

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
    let rec render acc diffs =
        match diffs with
        | [] -> acc
        | Common s :: rest -> render (acc + s) rest
        | Difference (d1, d2) :: rest -> render (acc + $"({d1}|{d2})") rest

    // Process input
    let diffs = identifyDiffs 0 [] "" ""
    let refinedDiffs = splitDifferences diffs
    render "" refinedDiffs

// Function to compare two files using LCS
let compareFiles (file1: string) (file2: string) =
    let lines1 = File.ReadAllLines(file1) |> Array.toList
    let lines2 = File.ReadAllLines(file2) |> Array.toList
    let common = lcs lines1 lines2

    let rec diff xs ys common acc =
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

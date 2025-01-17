module StringHelper

open System.Globalization
open System

/// Helper function to detect if a string contains emojis or special characters
let containsSpecialCharacters (s: string) =
    s |> Seq.exists (fun c -> int c > 127)

/// Helper function to get text elements from a string
let getTextElements (s: string) =
    let enumerator = StringInfo.GetTextElementEnumerator(s)
    [| while enumerator.MoveNext() do
         yield enumerator.GetTextElement() |]

/// Helper function to get text elements from a string, treating zero-width joiners as separate elements
let getTextElementsWithJoiners (s: string) =
    let rec splitElements (enumerator: TextElementEnumerator) acc =
        if not (enumerator.MoveNext()) then
            List.rev acc
        else
            let element = enumerator.GetTextElement()
            if element.Contains("\u200D") then  // Contains zero-width joiner
                let parts = element.Split([|'\u200D'|], StringSplitOptions.None)
                let joinedParts = 
                    parts 
                    |> Array.collect (fun p -> 
                        if String.IsNullOrEmpty(p) then [||]
                        else [|p; "\u200D"|])
                    |> Array.take (Array.length parts * 2 - 1)  // Remove last joiner if it was added
                splitElements enumerator (List.append (List.ofArray (Array.rev joinedParts)) acc)
            else
                splitElements enumerator (element :: acc)
    
    splitElements (StringInfo.GetTextElementEnumerator(s)) []
    |> Array.ofList

/// Helper function to get text element positions and lengths in a string
let getTextElementPositions (s: string) =
    let enumerator = StringInfo.GetTextElementEnumerator(s)
    let rec getPositions pos acc =
        if not (enumerator.MoveNext()) then
            List.rev acc
        else
            let element = enumerator.GetTextElement()
            let len = element.Length
            getPositions (pos + len) ((pos, len) :: acc)
    getPositions 0 [] |> Array.ofList

/// Helper function to get the character position after a given number of text elements
let getPositionAfterElements (positions: (int * int)[]) (elementCount: int) =
    if elementCount = 0 then 0
    elif elementCount > positions.Length then 
        let lastPos, lastLen = positions.[positions.Length - 1]
        lastPos + lastLen
    else
        let pos, len = positions.[elementCount - 1]
        pos + len

/// Helper function to calculate prefix length in terms of character positions.
/// This function returns the length in bytes of the common prefix, which can be used with String.Substring.
/// For example:
/// - For ASCII strings "hello" and "help", returns 3 (length of "hel")
/// - For strings with emojis "👨‍👩‍👧test" and "👨‍👩‍👧other", returns 8 (length of "👨‍👩‍👧")
/// - For Greek letters "αβγtest" and "αβδtest", returns 4 (length of "αβ", each Greek letter is 2 bytes)
let findPrefixLengthSpecial (s1: string) (s2: string) : int =
    if s1 = s2 then s1.Length
    else
        let elements1 = getTextElements s1
        let elements2 = getTextElements s2
        let positions1 = getTextElementPositions s1
        let rec findPrefix i acc =
            if i >= elements1.Length || i >= elements2.Length then
                acc
            elif elements1.[i] = elements2.[i] then
                let pos1, len1 = positions1.[i]
                findPrefix (i + 1) (pos1 + len1)
            else
                acc
        findPrefix 0 0

/// Helper function to calculate prefix length in terms of character positions.
/// This function returns the length in characters (not text elements) of the common prefix.
/// For example:
/// - For ASCII strings "hello" and "help", returns 3 (length of "hel")
/// - For strings with emojis "👨‍👩‍👧test" and "👨‍👩‍👧other", returns 4 (length of "👨‍👩‍👧")
let rec findPrefixLength (s1: string) (s2: string) : int =
    if containsSpecialCharacters s1 || containsSpecialCharacters s2 then
        findPrefixLengthSpecial s1 s2
    else
        let rec findPrefix i =
            if i >= s1.Length || i >= s2.Length then i
            elif s1.[i] = s2.[i] then findPrefix (i + 1)
            else i
        findPrefix 0

/// Helper function to calculate suffix length in terms of character positions.
/// This function returns the length in bytes of the common suffix, which can be used with String.Substring.
/// For example:
/// - For ASCII strings "world" and "fold", returns 3 (length of "old")
/// - For strings with emojis "test👨‍👩‍👧" and "best👨‍👩‍👧", returns 8 (length of "👨‍👩‍👧")
/// - For Greek letters "αβγtest" and "δβγtest", returns 6 (length of "γtest", where γ is 2 bytes)
let findSuffixLengthSpecial (s1: string) (s2: string) : int =
    if s1 = s2 then s1.Length
    else
        let elements1 = getTextElements s1
        let elements2 = getTextElements s2
        let positions1 = getTextElementPositions s1
        let prefixLen = findPrefixLength s1 s2
        let rec findSuffix i acc =
            let i1 = elements1.Length - 1 - i
            let i2 = elements2.Length - 1 - i
            if i1 < 0 || i2 < 0 then
                acc
            else
                let pos1, len1 = positions1.[i1]
                // Don't include suffix if it overlaps with prefix
                if pos1 < prefixLen then
                    acc
                elif elements1.[i1] = elements2.[i2] then
                    findSuffix (i + 1) (s1.Length - pos1)
                else
                    acc
        findSuffix 0 0

/// Helper function to calculate suffix length in terms of character positions.
/// This function returns the length in characters (not text elements) of the common suffix.
/// For example:
/// - For ASCII strings "world" and "fold", returns 3 (length of "old")
/// - For strings with emojis "test👨‍👩‍👧" and "best👨‍👩‍👧", returns 4 (length of "👨‍👩‍👧")
let rec findSuffixLength (s1: string) (s2: string) : int =
    if containsSpecialCharacters s1 || containsSpecialCharacters s2 then
        findSuffixLengthSpecial s1 s2
    else
        let rec findSuffix i =
            let i1 = s1.Length - 1 - i
            let i2 = s2.Length - 1 - i
            if i1 < 0 || i2 < 0 || i1 < findPrefixLength s1 s2 then i
            elif s1.[i1] = s2.[i2] then findSuffix (i + 1)
            else i
        findSuffix 0

/// Helper function to extract differences between strings.
/// The returned tuple contains (prefix, s1Diff, s2Diff) where:
/// - prefix is the common prefix in terms of character positions
/// - s1Diff is the differing part of s1 after the prefix and before any common suffix
/// - s2Diff is the differing part of s2 after the prefix and before any common suffix
let extractDifferences (s1: string) (s2: string) : string * string * string =
    if s1 = s2 then
        (s1, "", "")
    else
        let prefixLen = findPrefixLength s1 s2
        let suffixLen = findSuffixLength s1 s2
        let s1Len = s1.Length
        let s2Len = s2.Length
        
        // Make sure we don't have overlapping prefix and suffix
        let adjustedSuffixLen = 
            if prefixLen + suffixLen > min s1Len s2Len
            then max 0 (min s1Len s2Len - prefixLen)
            else suffixLen
            
        let prefix = s1.Substring(0, prefixLen)
        
        let s1Diff = 
            if s1Len - prefixLen - adjustedSuffixLen > 0 
            then s1.Substring(prefixLen, s1Len - prefixLen - adjustedSuffixLen)
            else ""
        let s2Diff = 
            if s2Len - prefixLen - adjustedSuffixLen > 0
            then s2.Substring(prefixLen, s2Len - prefixLen - adjustedSuffixLen)
            else ""
            
        // If we're dealing with special characters, ensure we don't split in the middle of a text element
        let adjustDiffForSpecialChars (s: string) (diff: string) =
            if containsSpecialCharacters diff then
                let elements = getTextElements diff
                String.Join("", elements)
            else diff
            
        let s1DiffAdjusted = adjustDiffForSpecialChars s1 s1Diff
        let s2DiffAdjusted = adjustDiffForSpecialChars s2 s2Diff
            
        (prefix, s1DiffAdjusted, s2DiffAdjusted)

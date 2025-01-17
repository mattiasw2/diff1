module StringHelperTests

open Xunit
open StringHelper

[<Fact>]
let ``findPrefixLength should return correct prefix length`` () =
    let testCases = [
        ("", "", 0)
        ("test🎮", "test📱", 4)
        ("hello world", "hello there", 6)
        ("αβγtest", "αβγother", 3)
        ("abc", "def", 0)
        ("abc", "abdef", 2)
        ("", "hello", 0)
        ("hello", "", 0)
        ("👨‍👩‍👧", "👨‍👩‍👦", 0)  // Different emoji sequences, no common prefix
        ("αβγ", "αβδ", 2)
        ("👨‍👩", "👨‍", 0)  // Partial family emoji match
        ("a", "abc", 1)
        ("hello", "hello world", 5)
        ("αβγtest", "αβγother", 3)
        ("hello", "help", 3)  // Common prefix "hel"
        ("abc", "def", 0)     // No common prefix
        ("", "test", 0)       // Empty first string
        ("test", "", 0)       // Empty second string
        ("", "", 0)           // Both empty
        ("👨‍👩", "👨‍", 0)      // Different emoji sequences
        ("👨‍👩‍👧", "👨‍👩‍👦", 0)  // Different emoji sequences
        ("test🎮", "test📱", 4)  // Common prefix up to emoji
        ("αβγ", "αβδ", 2)     // Common prefix in Greek letters
    ]
    
    for (s1, s2, expected) in testCases do
        let result = StringHelper.findPrefixLength s1 s2
        let message = sprintf "Expected prefix length of %d, got %d for strings '%s' and '%s'" expected result s1 s2
        Assert.True((result = expected), message)

[<Fact>]
let ``findPrefixLengthSpecial should work with plain strings`` () =
    let testCases = [
        ("hello", "help", 3)     // Common prefix "hel"
        ("abc", "def", 0)        // No common prefix
        ("", "test", 0)          // Empty first string
        ("test", "", 0)          // Empty second string
        ("", "", 0)              // Both empty
        ("hello", "hello", 5)    // Identical strings
        ("abc", "abdef", 2)      // Partial match
    ]
    
    for (s1, s2, expected) in testCases do
        let result = StringHelper.findPrefixLengthSpecial s1 s2
        let message = sprintf "Expected prefix length of %d, got %d for strings '%s' and '%s'" expected result s1 s2
        Assert.True((result = expected), message)

[<Fact>]
let ``findSuffixLength should return correct suffix length`` () =
    let testCases = [
        ("", "", 0)
        ("test🎮", "test📱", 0)
        ("αβγtest", "δβγtest", 6)  // "γtest" is a common suffix (2 bytes for γ + 4 for "test")
        ("a", "cba", 1)
        ("test👨‍👩‍👧", "other👨‍👩‍👧", 8)  // Full emoji sequence "👨‍👩‍👧" is 8 bytes
        ("", "hello", 0)
        ("world hello", "goodbye hello", 6)
        ("αβγ", "δβγ", 2)
        ("👨‍👩‍👧", "👧", 0)  // Not a suffix - 👧 is part of a larger text element
        ("abc", "deabc", 3)
        ("abc", "def", 0)
        ("hello", "", 0)
        ("the world", "hello world", 6)
        ("👨‍👩‍👧", "👨‍👩‍👧", 8)  // Full complex emoji match (8 bytes)
        ("hello", "jello", 4)  // Common suffix "ello"
        ("abc", "def", 0)      // No common suffix
        ("", "test", 0)        // Empty first string
        ("test", "", 0)        // Empty second string
        ("", "", 0)            // Both empty
        ("αβγtest", "δβγtest", 6)  // Common suffix with Greek letters
        ("test🎮", "best🎮", 5)    // Common suffix with emoji (1 byte for 't' + 4 bytes for 🎮)
        ("👨‍👩‍👧", "👨‍👩‍👦", 0)    // Different emoji sequences
    ]
    
    for (s1, s2, expected) in testCases do
        let result = StringHelper.findSuffixLength s1 s2
        let message = sprintf "Expected suffix length of %d, got %d for strings '%s' and '%s'" expected result s1 s2
        Assert.True((result = expected), message)

[<Fact>]
let ``findSuffixLengthSpecial should work with plain strings`` () =
    let testCases = [
        ("hello", "jello", 4)    // Common suffix "ello"
        ("abc", "def", 0)        // No common suffix
        ("", "test", 0)          // Empty first string
        ("test", "", 0)          // Empty second string
        ("", "", 0)              // Both empty
        ("hello", "hello", 5)    // Identical strings
        ("abc", "deabc", 3)      // Partial match
    ]
    
    for (s1, s2, expected) in testCases do
        let result = StringHelper.findSuffixLengthSpecial s1 s2
        let message = sprintf "Expected suffix length of %d, got %d for strings '%s' and '%s'" expected result s1 s2
        Assert.True((result = expected), message)

[<Fact>]
let ``substring operations should work with text element positions`` () =
    let testCases = [
        // (input, start, length, expected)
        ("test👨‍👩‍👧abc", 4, 8, "👨‍👩‍👧")  // Extract full emoji sequence
        ("αβγtest", 2, 5, "γtest")  // Extract gamma plus ASCII text (1 byte for γ + 4 for test)
        ("👨‍👩‍👧test", 8, 4, "test")  // Extract from after emoji (emoji is 8 bytes)
        ("test👨‍👩‍👧", 0, 4, "test")  // Extract from start
    ]
    
    // First verify the positions
    let input = "αβγtest"
    let positions = StringHelper.getTextElementPositions input
    Assert.True((positions.Length = 7), "Position count")  // 3 Greek letters + 4 ASCII letters
    Assert.True((positions.[0] = (0, 1)), "Position 0")  // α
    Assert.True((positions.[1] = (1, 1)), "Position 1")  // β
    Assert.True((positions.[2] = (2, 1)), "Position 2")  // γ
    Assert.True((positions.[3] = (3, 1)), "Position 3")  // t
    Assert.True((positions.[4] = (4, 1)), "Position 4")  // e
    Assert.True((positions.[5] = (5, 1)), "Position 5")  // s
    Assert.True((positions.[6] = (6, 1)), "Position 6")  // t
    
    for (input, start, length, expected) in testCases do
        let result = input.Substring(start, length)
        let message = sprintf "Substring failed for input '%s', start %d, length %d. Expected '%s', got '%s'" 
                            input start length expected result
        Assert.True((result = expected), message)

[<Fact>]
let ``getTextElementPositions should give correct positions for Greek letters`` () =
    let input = "αβγtest"
    let positions = StringHelper.getTextElementPositions input
    
    // Each Greek letter should be 1 byte, followed by ASCII letters
    Assert.True((positions.Length = 7), "Position count")  // 3 Greek letters + 4 ASCII letters
    Assert.True((positions.[0] = (0, 1)), "Position 0")  // α
    Assert.True((positions.[1] = (1, 1)), "Position 1")  // β
    Assert.True((positions.[2] = (2, 1)), "Position 2")  // γ
    Assert.True((positions.[3] = (3, 1)), "Position 3")  // t
    Assert.True((positions.[4] = (4, 1)), "Position 4")  // e
    Assert.True((positions.[5] = (5, 1)), "Position 5")  // s
    Assert.True((positions.[6] = (6, 1)), "Position 6")  // t

[<Fact>]
let ``extractDifferences should correctly split strings`` () =
    let testCases = [
        ("start here", "end here", "", "start", "end")  // Common suffix " here"
        ("goodbye world", "goodbye earth", "goodbye ", "world", "earth")
        ("αβγ", "αβδ", "αβ", "γ", "δ")
        ("same", "same", "same", "", "")
        ("", "", "", "", "")
        ("totally different", "completely unique", "", "totally different", "completely unique")
        ("👨‍👩‍👧test", "👨‍👩‍👧other", "👨‍👩‍👧", "test", "other")
        ("👨‍👩‍👦 coding", "👨‍👩‍👦 coding", "👨‍👩‍👦 coding", "", "")
        ("hello world", "hello there", "hello ", "world", "there")
        ("test👨‍👩‍👧", "test👨‍👩‍👦", "test", "👨‍👩‍👧", "👨‍👩‍👦")
        ("a", "b", "", "a", "b")
        ("test👨‍👩‍👧end", "test👨‍👩‍👦end", "test", "👨‍👩‍👧", "👨‍👩‍👦")
        ("the end", "the beginning", "the ", "end", "beginning")
        ("hello", "help", "hel", "lo", "p")     // Common prefix
        ("jello", "hello", "", "j", "h")        // Different first letters, no common prefix
        ("", "test", "", "", "test")            // Empty first string
        ("test", "", "", "test", "")            // Empty second string
        ("", "", "", "", "")                    // Both empty
        ("test🎮", "test📱", "test", "🎮", "📱")  // Common prefix with different emojis
        ("αβγ", "αβδ", "αβ", "γ", "δ")          // Common prefix with Greek letters
    ]
    
    for (s1, s2, expectedPrefix, expectedS1, expectedS2) in testCases do
        let (prefix, diff1, diff2) = StringHelper.extractDifferences s1 s2
        let message = sprintf "Failed for strings '%s' and '%s'" s1 s2
        Assert.True((prefix = expectedPrefix), sprintf "Expected prefix '%s', got '%s'. %s" expectedPrefix prefix message)
        Assert.True((diff1 = expectedS1), sprintf "Expected diff1 '%s', got '%s'. %s" expectedS1 diff1 message)
        Assert.True((diff2 = expectedS2), sprintf "Expected diff2 '%s', got '%s'. %s" expectedS2 diff2 message)



[<Fact>]
let ``getTextElements should handle all emoji lengths`` () =
    let testCases = [
        ("🎮", 2, "Single emoji character")  // Game controller
        ("👨‍👩", 5, "Two emoji characters with joiner")  // Man and woman
        ("👨‍👩‍👦", 8, "Three emoji characters with joiners")  // Family with boy
        ("👨‍👩‍👧‍👦", 11, "Four emoji characters with joiners")  // Family with girl and boy
    ]
    
    for (input, expectedLength, description) in testCases do
        let elements = StringHelper.getTextElements input
        Assert.True((elements.Length = 1), sprintf "%s should be treated as a single text element" description)
        let element = elements.[0]
        Assert.True((element.Length = expectedLength), sprintf "%s: Expected length of %d, got %d" description expectedLength element.Length)

[<Fact>]
let ``getTextElements should handle all special characters`` () =
    let testCases = [
        // Single emojis
        ("🎮", [|"🎮"|], "Single game controller emoji")
        ("📱", [|"📱"|], "Single phone emoji")
        ("👧", [|"👧"|], "Single person emoji")
        
        // Complex emojis
        ("👨‍👩‍👧", [|"👨‍👩‍👧"|], "Family with girl emoji")
        ("👨‍👩‍👦", [|"👨‍👩‍👦"|], "Family with boy emoji")
        ("👨‍👩", [|"👨‍👩"|], "Man and woman emoji")
        ("👨‍", [|"👨‍"|], "Incomplete emoji sequence")
        
        // Greek letters
        ("αβγ", [|"α"; "β"; "γ"|], "Greek letters")
        
        // Mixed content
        ("test🎮", [|"t"; "e"; "s"; "t"; "🎮"|], "ASCII with emoji")
        ("αβγtest", [|"α"; "β"; "γ"; "t"; "e"; "s"; "t"|], "Greek with ASCII")
        ("test👨‍👩‍👧end", [|"t"; "e"; "s"; "t"; "👨‍👩‍👧"; "e"; "n"; "d"|], "ASCII with complex emoji")
        
        // Edge cases
        ("", [||], "Empty string")
        ("hello", [|"h"; "e"; "l"; "l"; "o"|], "ASCII only")
        
        // Spaces
        (" ", [|" "|], "Single space")
        ("a b", [|"a"; " "; "b"|], "Letters with space")
    ]
    
    for (input, expected, description) in testCases do
        let result = StringHelper.getTextElements input
        let message = sprintf "Failed for case: %s" description
        Assert.True((result = expected), message)

[<Fact>]
let ``verify character lengths for different Unicode characters`` () =
    let testCases = [
        // ASCII (1 byte each in UTF-16)
        ("a", 1, "ASCII letter")
        ("1", 1, "ASCII digit")
        
        // Greek letters (1 byte each in UTF-16, part of BMP)
        ("α", 1, "Greek alpha")
        ("β", 1, "Greek beta")
        ("γ", 1, "Greek gamma")
        ("Ω", 1, "Greek omega")
        
        // CJK characters (1 code unit each in UTF-16, part of BMP)
        ("中", 1, "Chinese character")
        ("日", 1, "Japanese character")
        ("한", 1, "Korean character")
        
        // Surrogate pairs (2 code units each in UTF-16)
        ("𐐷", 2, "Deseret letter")  // U+10437
        ("𝄞", 2, "Musical symbol G clef")  // U+1D11E
        
        // Simple emojis (2 code units each in UTF-16)
        ("😀", 2, "Simple emoji")  // D83D DE00
        ("🌟", 2, "Star emoji")  // D83C DF1F
        ("🎮", 2, "Game controller emoji")  // D83C DFAe
        ("📱", 2, "Phone emoji")  // D83D DCF1
        ("👧", 2, "Girl emoji")  // D83D DC67
        
        // Complex emojis with ZWJ sequences
        ("👨‍👩‍👧", 8, "Family emoji")  // 👨(2) + ZWJ(1) + 👩(2) + ZWJ(1) + 👧(2) = 8
        ("👨‍💻", 5, "Man technologist")  // 👨(2) + ZWJ(1) + 💻(2) = 5
        
        // Combining characters
        ("é", 1, "Latin e with acute")  // Precomposed form
        ("e\u0301", 2, "Latin e + combining acute")  // Decomposed form
    ]
    
    for (input, expectedLen, description) in testCases do
        let positions = StringHelper.getTextElementPositions input
        let actualLen = input.Length
        let message = sprintf "Length mismatch for %s (%s). Expected %d, got %d. Code points: %s" 
                            input description expectedLen actualLen 
                            (input |> Seq.map (fun c -> sprintf "U+%04X" (int c)) |> String.concat " ")
        Assert.True((actualLen = expectedLen), message)
        
        // Also verify our position calculations
        Assert.True((positions.Length = 1), sprintf "Position count for %s (%s)" input description)  // Each test case is one text element
        Assert.True((positions.[0] = (0, actualLen)), sprintf "Position for %s (%s)" input description)  // Should start at 0 with the correct length
        
        // Test substring extraction
        let result = input.Substring(0, actualLen)
        Assert.True((result = input), sprintf "Substring for %s (%s)" input description)

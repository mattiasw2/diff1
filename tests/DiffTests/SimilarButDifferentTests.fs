module SimilarButDifferentTests

open Xunit

// Test cases for the `similarButDifferent` function using Theory
[<Theory>]
[<InlineData("", "", false)>]  // Empty strings
[<InlineData("", "nonempty", false)>]  // One empty, one non-empty
[<InlineData("a", "b", false)>]  // Single different characters
[<InlineData("abc", "def", false)>]
[<InlineData("abc", "xyz", false)>]
[<InlineData("F# is fun!", "cool is F#!", true)>]
[<InlineData("F# is fun!", "F# is cool!!!", true)>]
[<InlineData("F# is fun!", "F# is cool!", true)>]
[<InlineData("F# is fun!", "F# is fun!", false)>]
[<InlineData("F# is fun!", "F# is fun.", true)>]
[<InlineData("Hello", "Hello", false)>]  // Identical strings
[<InlineData("Hello, World!", "Hello, World!", false)>]
[<InlineData("Hello🌍", "Hello🌎", true)>]  // Emojis
[<InlineData("Very long string that has many characters", "Very long string with different ending", true)>]
[<InlineData("αβγ", "αδγ", true)>]  // Unicode characters
let ``Test SimilarButDifferent`` (line1: string, line2: string, expected: bool) =
    Assert.Equal(expected, CompareFiles.similarButDifferent line1 line2)

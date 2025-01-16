module SimilarButDifferentTests

open Xunit

// Test cases for the `similarButDifferent` function using Theory
[<Theory>]
[<InlineData("F# is fun!", "F# is cool!", true)>]
[<InlineData("abc", "xyz", false)>]
[<InlineData("F# is fun!", "F# is fun.", true)>]
[<InlineData("Hello, World!", "Hello, World!", false)>]
[<InlineData("abc", "def", false)>]
[<InlineData("F# is fun!", "F# is fun!", false)>]
[<InlineData("F# is fun!", "F# is cool!", true)>]
[<InlineData("F# is fun!", "F# is cool!!!", true)>]
[<InlineData("F# is fun!", "cool is F#!", true)>]
let ``Test SimilarButDifferent`` (line1: string, line2: string, expected: bool) =
    Assert.Equal(expected, CompareFiles.similarButDifferent line1 line2)

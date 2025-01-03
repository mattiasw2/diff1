module SimilarButDifferentTests

open Xunit

// Test cases for the `similarButDifferent` function
[<Fact>]
let ``Lines with at least 3 identical and 1 differing characters should be similar but different`` () =
    let line1 = "F# is fun!"
    let line2 = "F# is cool!"
    Assert.True(Program.similarButDifferent line1 line2)

[<Fact>]
let ``Lines with less than 3 identical characters should not be similar but different`` () =
    let line1 = "abc"
    let line2 = "xyz"
    Assert.False(Program.similarButDifferent line1 line2)

[<Fact>]
let ``Lines with less than 1 differing character should not be similar but different`` () =
    let line1 = "F# is fun!"
    let line2 = "F# is fun."
    Assert.True(Program.similarButDifferent line1 line2)

[<Fact>]
let ``Lines with identical content should not be similar but different`` () =
    let line1 = "Hello, World!"
    let line2 = "Hello, World!"
    Assert.False(Program.similarButDifferent line1 line2)

[<Fact>]
let ``Lines with no identical characters should not be similar but different`` () =
    let line1 = "abc"
    let line2 = "def"
    Assert.False(Program.similarButDifferent line1 line2)

[<Fact>]
let ``Lines with identical characters but no differing characters should not be similar but different`` () =
    let line1 = "F# is fun!"
    let line2 = "F# is fun!"
    Assert.False(Program.similarButDifferent line1 line2)

[<Fact>]
let ``Lines with at least 3 identical and 1 differing characters in different positions should be similar but different`` () =
    let line1 = "F# is fun!"
    let line2 = "F# is cool!"
    Assert.True(Program.similarButDifferent line1 line2)

[<Fact>]
let ``Lines with at least 3 identical and 1 differing characters but different lengths should be similar but different`` () =
    let line1 = "F# is fun!"
    let line2 = "F# is cool!!!"
    Assert.True(Program.similarButDifferent line1 line2)

[<Fact>]
let ``Lines with at least 3 identical and 1 differing characters but reversed should be similar but different`` () =
    let line1 = "F# is fun!"
    let line2 = "cool is F#!"
    Assert.True(Program.similarButDifferent line1 line2)
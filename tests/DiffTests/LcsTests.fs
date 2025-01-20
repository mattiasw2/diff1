module LcsTests

open Xunit
open CompareFiles

[<Theory>]
[<InlineData("", "", "")>]  // Empty lists
[<InlineData("a", "b", "")>]  // No common elements
[<InlineData("a,b,c", "b,c,d", "b,c")>]  // Common subsequence
[<InlineData("a,b,c,d", "b,d,e", "b,d")>]  // Scattered common elements
[<InlineData("hello,world", "hello,there,world", "hello,world")>]  // Common prefix and suffix
[<InlineData("x,y,z", "x,y,z", "x,y,z")>]  // Identical lists
let ``Test lcs function`` (list1: string, list2: string, expected: string) =
    let parseList (s: string) = 
        if System.String.IsNullOrEmpty(s) then []
        else s.Split(',') |> List.ofArray
    let result = lcs (parseList list1) (parseList list2)
    Assert.Equal<string list>(parseList expected, result)

[<Fact>]
let ``Test lcs with long sequences`` () =
    let longList1 = List.init 100 (fun i -> $"item{i}")
    let longList2 = List.init 100 (fun i -> if i % 2 = 0 then $"item{i}" else $"other{i}")
    let result = lcs longList1 longList2
    Assert.True(result.Length > 0, $"Expected non-empty result, but got length {result.Length}")

[<Fact>]
let ``Test lcs with empty sequences`` () =
    let empty1: string list = []
    let empty2: string list = []
    Assert.Equal<string list>([], lcs empty1 empty2)

[<Fact>]
let ``Test lcs with emoji sequences`` () =
    let seq1 = ["👋"; "🌍"; "👨‍💻"; "🎉"]
    let seq2 = ["👋"; "🌎"; "👨‍💻"; "🎊"]
    let expected = ["👋"; "👨‍💻"]
    Assert.Equal<string list>(expected, lcs seq1 seq2)

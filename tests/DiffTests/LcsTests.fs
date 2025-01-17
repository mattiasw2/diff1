module LcsTests

open Xunit
open CompareFiles

[<Theory>]
[<InlineData([], [], [])>]  // Empty lists
[<InlineData([|"a"|], [|"b"|], [])>]  // No common elements
[<InlineData([|"a"; "b"; "c"|], [|"b"; "c"; "d"|], [|"b"; "c"|])>]  // Common subsequence
[<InlineData([|"x"; "y"; "z"|], [|"x"; "y"; "z"|], [|"x"; "y"; "z"|])>]  // Identical lists
[<InlineData([|"a"; "b"; "c"; "d"|], [|"b"; "d"; "e"|], [|"b"; "d"|])>]  // Scattered common elements
[<InlineData([|"hello"; "world"|], [|"hello"; "there"; "world"|], [|"hello"; "world"|])>]  // Common prefix and suffix
let ``Test lcs function`` (list1: string[], list2: string[], expected: string[]) =
    let result = lcs (List.ofArray list1) (List.ofArray list2)
    Assert.Equal<string list>(List.ofArray expected, result)

[<Fact>]
let ``Test lcs with long sequences`` () =
    let seq1 = ["The"; "quick"; "brown"; "fox"; "jumps"; "over"; "the"; "lazy"; "dog"]
    let seq2 = ["The"; "brown"; "cat"; "jumps"; "under"; "the"; "lazy"; "dog"]
    let expected = ["The"; "brown"; "jumps"; "the"; "lazy"; "dog"]
    Assert.Equal<string list>(expected, lcs seq1 seq2)

[<Fact>]
let ``Test lcs with Unicode strings`` () =
    let seq1 = ["α"; "β"; "γ"; "δ"]
    let seq2 = ["α"; "γ"; "ε"; "δ"]
    let expected = ["α"; "γ"; "δ"]
    Assert.Equal<string list>(expected, lcs seq1 seq2)

[<Fact>]
let ``Test lcs with emoji sequences`` () =
    let seq1 = ["👋"; "🌍"; "👨‍💻"; "🎉"]
    let seq2 = ["👋"; "🌎"; "👨‍💻"; "🎊"]
    let expected = ["👋"; "👨‍💻"]
    Assert.Equal<string list>(expected, lcs seq1 seq2)

module HighlightDifferencesTests

open Xunit

// Test cases for the `highlightDifferences` function
[<Theory>]
[<InlineData("abcd", "abc", "abc(d|)")>] // Extra character at the end
[<InlineData("abc", "abcd", "abc(|d)")>] // Extra character at the end
[<InlineData("foo", "bar", "(foo|bar)")>] // Completely different lines
[<InlineData("xfooy", "xbary", "x(foo|bar)y")>] // Differences in the middle
[<InlineData("abc", "def", "(abc|def)")>] // Completely different lines
[<InlineData("abc", "abc", "abc")>] // Identical lines
[<InlineData("ab", "abc", "ab(|c)")>] // Extra character at the end
[<InlineData("abc", "ab", "ab(c|)")>] // Extra character at the end
[<InlineData("abcde", "axcye", "a(b|x)c(d|y)e")>] // Multiple differences
[<InlineData("abc", "axc", "a(b|x)c")>] // Single difference in the middle
[<InlineData("abc", "ayc", "a(b|y)c")>] // Single difference in the middle
[<InlineData("abc", "abz", "ab(c|z)")>] // Single difference at the end
[<InlineData("abc", "zbc", "(a|z)bc")>] // Single difference at the start
[<InlineData("hello world", "hello there", "hello (world|there)")>] // Multi-word difference
[<InlineData("the quick brown fox", "the slow brown fox", "the (quick|slow) brown fox")>] // Multi-word difference
[<InlineData("this is a test", "this was a test", "this (i|wa)s a test")>] // Multi-word difference
[<InlineData("one two three", "one two four", "one two (three|four)")>] // Multi-word difference
let ``highlightDifferences should correctly highlight differences`` (left: string) (right: string) (expected: string) =
    let result = CompareFiles.highlightDifferences left right
    Assert.Equal(expected, result)
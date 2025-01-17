module HighlightDifferencesTests

open Xunit

// Test cases for the `highlightDifferences` function
[<Theory>]
[<InlineData("  abc  ", "  xyz  ", "  (abc|xyz)  ")>] // Preserve whitespace
[<InlineData("", "", "")>]  // Empty strings to make extractDifferences return ("","","")
[<InlineData("", "abc", "(|abc)")>] // One empty, one non-empty
[<InlineData("ab", "abc", "ab(|c)")>] // Extra character at the end
[<InlineData("abc", "", "(abc|)")>] // One non-empty, one empty
[<InlineData("abc", "ab", "ab(c|)")>] // Extra character at the end
[<InlineData("abc", "abc", "abc")>] // Identical lines
[<InlineData("abc", "abcd", "abc(|d)")>] // Extra character at the end
[<InlineData("abc", "abz", "ab(c|z)")>] // Single difference at the end
[<InlineData("abc", "axc", "a(b|x)c")>] // Single difference in the middle
[<InlineData("abc", "ayc", "a(b|y)c")>] // Single difference in the middle
[<InlineData("abc", "def", "(abc|def)")>] // Completely different lines
[<InlineData("abc", "zbc", "(a|z)bc")>] // Single difference at the start
[<InlineData("abcd", "abc", "abc(d|)")>] // Extra character at the end
[<InlineData("abcde", "axcye", "a(bcd|xcy)e")>] // Multiple differences
[<InlineData("foo", "bar", "(foo|bar)")>] // Completely different lines
[<InlineData("Hello test! Bye", "Hello test! Bye", "Hello test! Bye")>] 
[<InlineData("Hello test! Bye", "Hello world! Bye", "Hello (test|world)! Bye")>]
[<InlineData("hello world", "hello there", "hello (world|there)")>] // Multi-word difference
[<InlineData("Hello🌍", "Hello🌎", "Hello(🌍|🌎)")>] // Emojis
[<InlineData("one two three", "one two four", "one two (three|four)")>] // Multi-word difference
[<InlineData("prefix abcd suffix", "prefix abcd suffix", "prefix abcd suffix")>]  // Identical strings to make extractDifferences return ("abcd","","")
[<InlineData("prefix abcde suffix", "prefix fghij suffix", "prefix (abcde|fghij) suffix")>]  // Long diffs (>3 chars) with no common substrings
[<InlineData("prefix middle1 suffix", "prefix middle1 suffix", "prefix middle1 suffix")>] 
[<InlineData("prefix middle1 suffix", "prefix middle2 suffix", "prefix middle(1|2) suffix")>]
[<InlineData("start 123 end", "start 123 end", "start 123 end")>] 
[<InlineData("start 123 end", "start 456 end", "start (123|456) end")>]
[<InlineData("start 12345 end", "start 12345 end", "start 12345 end")>]  // Another case with identical strings
[<InlineData("start 12345 end", "start 67890 end", "start (12345|67890) end")>]  // Another case with long diffs (>3 chars)
[<InlineData("the quick brown fox", "the slow brown fox", "the (quick|slow) brown fox")>] // Multi-word difference
[<InlineData("this is a test", "this was a test", "this (i|wa)s a test")>] // Multi-word difference
[<InlineData("xfooy", "xbary", "x(foo|bar)y")>] // Differences in the middle
[<InlineData("αβγ", "αδγ", "α(β|δ)γ")>] // Unicode characters
[<InlineData("🌍 abc 🌎", "🌍 abc 🌎", "🌍 abc 🌎")>] 
[<InlineData("🌍 abc 🌎", "🌍 xyz 🌎", "🌍 (abc|xyz) 🌎")>]
[<InlineData("👨‍💻 coding", "👩‍💻 coding", "(👨‍💻|👩‍💻) coding")>] // Complex emojis with ZWJ - compare complete sequences
let ``highlightDifferences should correctly highlight differences`` (left: string) (right: string) (expected: string) =
    let result = CompareFiles.highlightDifferences left right
    Assert.Equal(expected, result)
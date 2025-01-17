module StringHelperTests

open Xunit
open CompareFiles

[<Theory>]
[<InlineData("hello", "hello world", 5)>]  // Full prefix match
[<InlineData("hello world", "hello there", 6)>]  // Partial prefix match
[<InlineData("", "hello", 0)>]  // Empty first string
[<InlineData("hello", "", 0)>]  // Empty second string
[<InlineData("", "", 0)>]  // Both empty
[<InlineData("abc", "def", 0)>]  // No common prefix
[<InlineData("a", "abc", 1)>]  // Single character prefix
[<InlineData("abc", "abdef", 2)>]  // Partial prefix with different lengths
[<InlineData("αβγ", "αβδ", 2)>]  // Unicode prefix
[<InlineData("👨‍💻", "👨‍", 2)>]  // Emoji prefix (with ZWJ)
[<InlineData("👨‍💻", "👨‍💻", 4)>]  // Full emoji match
[<InlineData("👨‍💻test", "👨‍💻other", 4)>]  // Emoji prefix with different suffixes
[<InlineData("αβγtest", "αβγother", 3)>]  // Unicode prefix with different suffixes
[<InlineData("test👨‍💻", "test👩‍💻", 4)>]  // Common prefix with different emojis
let ``findPrefixLength should return correct prefix length`` (s1: string, s2: string, expected: int) =
    let result = CompareFiles.findPrefixLength s1 s2
    Assert.Equal(expected, result)

[<Theory>]
[<InlineData("world hello", "goodbye hello", 6)>]  // Full suffix match
[<InlineData("the world", "hello world", 6)>]  // Partial suffix match
[<InlineData("", "hello", 0)>]  // Empty first string
[<InlineData("hello", "", 0)>]  // Empty second string
[<InlineData("", "", 0)>]  // Both empty
[<InlineData("abc", "def", 0)>]  // No common suffix
[<InlineData("a", "cba", 1)>]  // Single character suffix
[<InlineData("abc", "deabc", 3)>]  // Full suffix with different lengths
[<InlineData("αβγ", "δβγ", 2)>]  // Unicode suffix
[<InlineData("👨‍💻", "💻", 1)>]  // Emoji suffix
[<InlineData("👨‍💻", "👨‍💻", 4)>]  // Full emoji match
[<InlineData("test👨‍💻", "other👨‍💻", 4)>]  // Different prefixes with emoji suffix
[<InlineData("test👨‍💻", "test👩‍💻", 0)>]  // Same prefix with different emojis
[<InlineData("αβγtest", "δβγtest", 4)>]  // Different Unicode prefixes with same suffix
let ``findSuffixLength should return correct suffix length`` (s1: string, s2: string, expected: int) =
    let result = CompareFiles.findSuffixLength s1 s2
    Assert.Equal(expected, result)

[<Theory>]
[<InlineData("hello world", "hello there", "hello ", " world", " there")>]  // Common prefix
[<InlineData("goodbye world", "goodbye earth", "goodbye ", " world", " earth")>]  // Common prefix
[<InlineData("the end", "the beginning", "the ", " end", " beginning")>]  // Common prefix only
[<InlineData("start here", "end here", "", "start ", "end ")>]  // Common suffix only
[<InlineData("totally different", "completely unique", "", "totally different", "completely unique")>]  // No common parts
[<InlineData("", "", "", "", "")>]  // Empty strings
[<InlineData("same", "same", "same", "", "")>]  // Identical strings
[<InlineData("a", "b", "", "a", "b")>]  // Single different characters
[<InlineData("αβγ", "αδγ", "α", "βγ", "δγ")>]  // Unicode strings
[<InlineData("👨‍💻 coding", "👩‍💻 coding", "", "👨‍💻 coding", "👩‍💻 coding")>]  // Complex emoji strings
[<InlineData("test👨‍💻", "test👩‍💻", "test", "👨‍💻", "👩‍💻")>]  // Common prefix with different emojis
[<InlineData("👨‍💻test", "👨‍💻other", "👨‍💻", "test", "other")>]  // Common emoji prefix
[<InlineData("test👨‍💻end", "test👩‍💻end", "test", "👨‍💻", "👩‍💻")>]  // Common prefix and suffix with different emojis
let ``extractDifferences should correctly split strings`` 
    (s1: string, s2: string, expectedPrefix: string, expectedS1: string, expectedS2: string) =
    let (prefix, diff1, diff2) = CompareFiles.extractDifferences s1 s2
    Assert.Equal(expectedPrefix, prefix)
    Assert.Equal(expectedS1, diff1)
    Assert.Equal(expectedS2, diff2)

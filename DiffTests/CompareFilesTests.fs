module CompareFilesTests

open System
open System.IO
open Xunit

// Helper function to create temporary files and run the diff
let runDiffWithTempFiles (content1: string) (content2: string) =
    let file1 = Path.GetTempFileName()
    let file2 = Path.GetTempFileName()
    // Normalize line endings to LF
    let normalizedContent1 = content1.Replace("\r\n", "\n").Replace("\r", "\n")
    let normalizedContent2 = content2.Replace("\r\n", "\n").Replace("\r", "\n")
    File.WriteAllText(file1, normalizedContent1)
    File.WriteAllText(file2, normalizedContent2)
    let output = ConsoleCapture.CaptureOutput (fun () -> CompareFiles.compareFiles file1 file2)
    File.Delete(file1)
    File.Delete(file2)
    // Normalize line endings in the output
    output.Replace("\r\n", "\n").Replace("\r", "\n")

// Parameterized tests using Theory and InlineData
[<Theory>]
[<InlineData("Hello, World!\nThis is a test.", "Hello, World!\nThis is a test.", "  Hello, World!\n  This is a test.")>]
[<InlineData("Hello, World!", "Hello, World!\nNew line here.", "  Hello, World!\n+ New line here.")>]
[<InlineData("Hello, World!\nThis is a test.", "Hello, World!", "  Hello, World!\n- This is a test.")>]
[<InlineData("Hello, World!\nF# is funn!", "Hello, World!\nF# is cool!", "  Hello, World!\n~ F# is (funn|cool)!")>]
[<InlineData("Hello, World!\nF# is fun!", "Hello, World!\nF# is cool!", "  Hello, World!\n~ F# is (fun|cool)!")>]
[<InlineData("Hello, World!\nThis is a test.\nF# is funn!\nGoodbye!", "Hello, World!\nThis is a test.\nF# is cool!\nNew line here.", 
             "  Hello, World!\n  This is a test.\n~ F# is (funn|cool)!\n+ New line here.\n- Goodbye!")>]
[<InlineData("", "", "")>]
let ``Diff tests should handle various cases`` (content1: string, content2: string, expected: string) =
    let result = runDiffWithTempFiles content1 content2
    Assert.Equal(expected, result)

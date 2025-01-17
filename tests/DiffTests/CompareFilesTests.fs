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
[<InlineData("line1\n\nline3", "line1\nline2\nline3", "  line1\n+ line2\n- \n  line3")>]  // Empty line vs content
[<InlineData("αβγ\nδεζ", "αβγ\nηθι", "  αβγ\n+ ηθι\n- δεζ")>]  // Unicode content
[<InlineData("Hello🌍\nGoodbye", "Hello🌎\nGoodbye", "~ Hello(🌍|🌎)\n  Goodbye")>]  // Emoji content
let ``Diff tests should handle various cases`` (content1: string, content2: string, expected: string) =
    let result = runDiffWithTempFiles content1 content2
    Assert.Equal(expected, result)

[<Fact>]
let ``Test compareFiles with file not found`` () =
    let nonExistentFile1 = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())
    let nonExistentFile2 = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())
    
    Assert.Throws<FileNotFoundException>(fun () -> 
        CompareFiles.compareFiles nonExistentFile1 nonExistentFile2)

[<Fact>]
let ``Test compareFiles with one empty file`` () =
    let file1 = Path.GetTempFileName()
    let file2 = Path.GetTempFileName()
    
    File.WriteAllText(file1, "")
    File.WriteAllText(file2, "line1\nline2")
    
    let output = ConsoleCapture.CaptureOutput (fun () -> 
        CompareFiles.compareFiles file1 file2)
    
    let expected = "+ line1\n+ line2"
    Assert.Equal(expected.Replace("\r\n", "\n").Replace("\r", "\n"), 
                output.Replace("\r\n", "\n").Replace("\r", "\n"))
    
    File.Delete(file1)
    File.Delete(file2)

[<Fact>]
let ``Test compareFiles with only similar but different lines`` () =
    let file1 = Path.GetTempFileName()
    let file2 = Path.GetTempFileName()
    
    File.WriteAllText(file1, "Hello World!\nF# is fun!\nGoodbye all!")
    File.WriteAllText(file2, "Hello There!\nF# is cool!\nBye everyone!")
    
    let output = ConsoleCapture.CaptureOutput (fun () -> 
        CompareFiles.compareFiles file1 file2)
    
    let expected = "~ Hello (World|There)!\n~ F# is (fun|cool)!\n~ (Goodbye all|Bye everyone)!"
    Assert.Equal(expected.Replace("\r\n", "\n").Replace("\r", "\n"), 
                output.Replace("\r\n", "\n").Replace("\r", "\n"))
    
    File.Delete(file1)
    File.Delete(file2)

[<Fact>]
let ``Test compareFiles with large files`` () =
    let file1 = Path.GetTempFileName()
    let file2 = Path.GetTempFileName()
    
    let lines1 = [| for i in 1..100 -> sprintf "Line %d" i |]
    let lines2 = 
        [| for i in 1..100 -> 
            if i = 50 then "Modified Line 50"
            else sprintf "Line %d" i |]
    
    File.WriteAllLines(file1, lines1)
    File.WriteAllLines(file2, lines2)
    
    let output = ConsoleCapture.CaptureOutput (fun () -> 
        CompareFiles.compareFiles file1 file2)
    
    // Verify that we have both the original and modified lines
    Assert.Contains("Line 50", output)
    Assert.Contains("Modified Line 50", output)
    
    File.Delete(file1)
    File.Delete(file2)

[<Fact>]
let ``Test compareFiles with read-only files`` () =
    let file1 = Path.GetTempFileName()
    let file2 = Path.GetTempFileName()
    
    File.WriteAllText(file1, "test1")
    File.WriteAllText(file2, "test2")
    
    // Make files read-only
    let attrs1 = File.GetAttributes(file1)
    let attrs2 = File.GetAttributes(file2)
    File.SetAttributes(file1, attrs1 ||| FileAttributes.ReadOnly)
    File.SetAttributes(file2, attrs2 ||| FileAttributes.ReadOnly)
    
    let output = ConsoleCapture.CaptureOutput (fun () -> 
        CompareFiles.compareFiles file1 file2)
    
    let expected = "~ test(1|2)"
    Assert.Equal(expected.Replace("\r\n", "\n").Replace("\r", "\n"), 
                output.Replace("\r\n", "\n").Replace("\r", "\n"))
    
    // Reset attributes for deletion
    File.SetAttributes(file1, attrs1)
    File.SetAttributes(file2, attrs2)
    File.Delete(file1)
    File.Delete(file2)

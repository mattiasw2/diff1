module Program

open System
open System.IO

open CompareFiles

// Main entry point
[<EntryPoint>]
let main argv =
    if argv.Length <> 2 then
        printfn "Usage: diff <file1> <file2>"
        1
    else
        let file1 = argv.[0]
        let file2 = argv.[1]
        compareFiles file1 file2
        0
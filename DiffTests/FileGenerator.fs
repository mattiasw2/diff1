module FileGenerator

open System
open System.IO

/// Generates random content for a file.
let generateContent (lines: int) (maxWidth: int) (seed: int) : string list =
    let random = Random(seed)
    let generateLine () =
        let width = random.Next(1, maxWidth + 1) // Random width for each line
        String.init width (fun _ -> string (char (random.Next(32, 127))))
    List.init lines (fun _ -> generateLine ())

/// Applies differences to the generated content based on indices.
let applyDifferences (content1: string list) (content2: string list) (differences: int) (seed: int) : string list * string list =
    let random = Random(seed)
    let mutable diffCount = 0
    let mutable modified1 = content1 |> Array.ofList
    let mutable modified2 = content2 |> Array.ofList

    // Pass 1: Edit rows
    for _ in 1..(differences / 2) do
        if modified2.Length > 0 then
            let lineIndex = random.Next(0, modified2.Length)
            match random.Next(0, 2) with
            | 0 -> // Replace whole row in modified2
                let newLine2 = String.init (random.Next(1, 80)) (fun _ -> string (char (random.Next(32, 127))))
                modified2.[lineIndex] <- newLine2
            | 1 -> // Change part of the row
                let line2 = modified2.[lineIndex]
                let len2 = line2.Length

                // Decide the number of characters to replace
                let replaceCount = random.Next(1, min 10 len2) // Replace 1 to 5 characters (or max length)
                let startPos = random.Next(0, len2 - replaceCount + 1)

                let modifiedPart = String.init replaceCount (fun _ -> string (char (random.Next(32, 127))))
                let newLine2 = line2.[..startPos-1] + modifiedPart + line2.[startPos+replaceCount..]

                modified2.[lineIndex] <- newLine2
            | _ -> ()

            diffCount <- diffCount + 1


    // Pass 2: Add and remove rows
    while diffCount < differences do
        match random.Next(0, 2) with
        | 0 when modified2.Length > 0 -> // Remove a line
            let lineIndex = random.Next(0, modified2.Length)
            modified2 <- Array.removeAt lineIndex modified2
            diffCount <- diffCount + 1
        | 1 -> // Add a new line
            let newLine = String.init (random.Next(1, 80)) (fun _ -> string (char (random.Next(32, 127))))
            let position = random.Next(0, modified2.Length + 1) // Random position, including the end
            modified2 <- Array.insertAt position newLine modified2
            diffCount <- diffCount + 1
        | _ -> ()

    List.ofArray modified1, List.ofArray modified2

/// Writes content to files.
let writeToFile (filePath: string) (content: string list) =
    File.WriteAllLines(filePath, content)

/// Main function to generate files with differences.
let generateFiles (lines: int) (maxWidth: int) (differences: int) (seed: int) (file1: string) (file2: string) =
    let content1 = generateContent lines maxWidth seed
    let content2 = content1
    let modified1, modified2 = applyDifferences content1 content2 differences seed

    writeToFile file1 modified1
    writeToFile file2 modified2

/// Example usage:
/// generateFiles 1000 80 50 42 "file1.txt" "file2.txt"

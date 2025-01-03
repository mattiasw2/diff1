module Program

[<EntryPoint>]
let main _ = 
    System.Console.WriteLine("Create test data files c:/temp/file1.txt and c:/temp/big1.txt")
    let _ = FileGenerator.generateFiles 1000 80 50 42 "c:/temp/file1.txt" "c:/temp/file2.txt"
    let _ = FileGenerator.generateFiles 100000 1000 5000 43 "c:/temp/big1.txt" "c:/temp/big2.txt"
    0

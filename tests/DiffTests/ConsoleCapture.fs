module ConsoleCapture
open System
open System.IO

/// Captures the console output of a given function
let CaptureOutput (action: unit -> unit) =
    let originalOut = Console.Out
    use writer = new StringWriter()
    Console.SetOut(writer)
    action()
    Console.SetOut(originalOut)
    writer.ToString().TrimEnd()

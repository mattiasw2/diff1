module FsCheckAddons

open FsCheck

// Configure FsCheck to disallow null strings
type NonNullString = 
    static member String() =
        Arb.Default.String() 
        |> Arb.filter (fun s -> not (isNull s)) // Exclude null strings

let _ = Arb.register<NonNullString>()
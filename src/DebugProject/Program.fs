open System
open System.Xml.Linq
open EFConcurrencyModeTest
open EFConcurrencyModeTest.Tests

[<EntryPoint>]
let main argv = 
    let cmt = ConcurrencyModeTester()
    cmt.ConcurrencyColumnNamePatterns <- [| "VerNo" |]
    let result = cmt.BadConcurrencyModes((XDocument.Parse csdl).Root, 
                                         (XDocument.Parse ssdl).Root, 
                                         (XDocument.Parse msl).Root)
    printfn "%s" (cmt.FormatBadConcurrencyModes result)
    printfn "Done. Press Enter."
    Console.ReadLine() |> ignore
    0

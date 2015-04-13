namespace System
open System.Reflection
open System.Runtime.InteropServices

[<assembly: AssemblyTitleAttribute("EFConcurrencyModeTest")>]
[<assembly: AssemblyProductAttribute("EFConcurrencyModeTest")>]
[<assembly: AssemblyCopyrightAttribute("wezeku.com")>]
[<assembly: AssemblyDescriptionAttribute("Helper class for unit tests of Entity Framework concurrency mode.")>]
[<assembly: AssemblyVersionAttribute("2.0.1")>]
[<assembly: AssemblyFileVersionAttribute("2.0.1")>]
[<assembly: ComVisibleAttribute(false)>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "2.0.1"

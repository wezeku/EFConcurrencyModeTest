namespace System
open System.Reflection
open System.Runtime.InteropServices

[<assembly: AssemblyTitleAttribute("EFConcurrencyModeTest")>]
[<assembly: AssemblyProductAttribute("EFConcurrencyModeTest")>]
[<assembly: AssemblyCopyrightAttribute("wezeku.com")>]
[<assembly: AssemblyDescriptionAttribute("Helper class for unit tests of Entity Framework concurrency mode.")>]
[<assembly: AssemblyVersionAttribute("1.0.2")>]
[<assembly: AssemblyFileVersionAttribute("1.0.2")>]
[<assembly: ComVisibleAttribute(false)>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "1.0.2"

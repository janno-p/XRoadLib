namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("XRoadLib")>]
[<assembly: AssemblyProductAttribute("XRoadLib")>]
[<assembly: AssemblyDescriptionAttribute("A .NET library for implementing service interfaces of X-Road providers using Code-First Development approach.")>]
[<assembly: AssemblyVersionAttribute("1.0")>]
[<assembly: AssemblyFileVersionAttribute("1.0")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "1.0"

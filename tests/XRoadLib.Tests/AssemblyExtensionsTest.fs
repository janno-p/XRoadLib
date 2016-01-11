namespace XRoadLib.Tests

open FsUnit
open NUnit.Framework
open System
open XRoadLib
open XRoadLib.Extensions

[<TestFixture>]
module AssemblyExtensionsTest =
    type Marker = class end

    [<Test>]
    let ``find missing X-Road producer name`` () =
        typeof<Marker>.Assembly.FindProducerName() |> should be Null

    [<Test>]
    let ``get missing X-Road producer name`` () =
        TestDelegate(fun _ -> typeof<Marker>.Assembly.GetProducerName() |> ignore)
        |> should (throwWithMessage @"Unable to extract producer name from contract assembly `XRoadLib.Tests`.") typeof<Exception>

    [<Test>]
    let ``find existing X-Road producer name`` () =
        typeof<XRoadLib.Tests.Contract.Class1>.Assembly.FindProducerName() |> should equal "test-producer"

    [<Test>]
    let ``get existing X-Road producer name`` () =
        typeof<XRoadLib.Tests.Contract.Class1>.Assembly.GetProducerName() |> should equal "test-producer"

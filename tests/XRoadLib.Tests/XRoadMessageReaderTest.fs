namespace XRoadLib.Tests

open FsUnit
open NUnit.Framework
open System.Collections.Specialized
open System.IO
open System.Text
open XRoadLib
open XRoadLib.Serialization

[<TestFixture>]
module XRoadMessageReaderTest =
    [<Test>]
    let ``can handle buffer limit`` () =
        use stream = new MemoryStream(Array.init 10 (fun _ -> 32uy))
        use reader = new XRoadMessageReader(stream, new NameValueCollection(), Encoding.UTF8, Path.GetTempPath())
        stream.Position <- 0L

        let mutable chunk = [| |]

        reader.ReadChunkOrLine(&chunk, 3) |> should equal XRoadMessageReader.ChunkStop.BufferLimit
        chunk |> should equal [| 32uy; 32uy; 32uy |]

        reader.ReadChunkOrLine(&chunk, 3) |> should equal XRoadMessageReader.ChunkStop.BufferLimit
        chunk |> should equal [| 32uy; 32uy; 32uy |]

        reader.ReadChunkOrLine(&chunk, 3) |> should equal XRoadMessageReader.ChunkStop.BufferLimit
        chunk |> should equal [| 32uy; 32uy; 32uy |]

        reader.ReadChunkOrLine(&chunk, 3) |> should equal XRoadMessageReader.ChunkStop.EndOfStream
        chunk |> should equal [| 32uy |]

    [<Test>]
    let ``can handle line marker`` () =
        use stream = new MemoryStream([| 32uy; 32uy; 32uy; 32uy; 13uy; 10uy; 32uy; 32uy; 32uy; 32uy |])
        use reader = new XRoadMessageReader(stream, new NameValueCollection(), Encoding.UTF8, Path.GetTempPath())
        stream.Position <- 0L

        let mutable chunk = [| |]

        reader.ReadChunkOrLine(&chunk, 10) |> should equal XRoadMessageReader.ChunkStop.NewLine
        chunk |> should equal [| 32uy; 32uy; 32uy; 32uy |]

        reader.ReadChunkOrLine(&chunk, 10) |> should equal XRoadMessageReader.ChunkStop.EndOfStream
        chunk |> should equal [| 32uy; 32uy; 32uy; 32uy |]

    [<Test>]
    let ``can handle chunk beginning with marker`` () =
        use stream = new MemoryStream([| 32uy; 32uy; 32uy; 32uy; 13uy; 10uy; 32uy; 32uy; 32uy; 32uy |])
        use reader = new XRoadMessageReader(stream, new NameValueCollection(), Encoding.UTF8, Path.GetTempPath())
        stream.Position <- 0L

        let mutable chunk = [| |]

        reader.ReadChunkOrLine(&chunk, 4) |> should equal XRoadMessageReader.ChunkStop.BufferLimit
        chunk |> should equal [| 32uy; 32uy; 32uy; 32uy |]

        reader.ReadChunkOrLine(&chunk, 4) |> should equal XRoadMessageReader.ChunkStop.NewLine
        chunk |> should equal [| |]

        reader.ReadChunkOrLine(&chunk, 4) |> should equal XRoadMessageReader.ChunkStop.BufferLimit
        chunk |> should equal [| 32uy; 32uy; 32uy; 32uy |]

        reader.ReadChunkOrLine(&chunk, 4) |> should equal XRoadMessageReader.ChunkStop.EndOfStream
        chunk |> should equal [| |]

    [<Test>]
    let ``can handle splitting marker`` () =
        use stream = new MemoryStream([| 32uy; 32uy; 32uy; 32uy; 13uy; 10uy; 32uy; 32uy; 32uy; 32uy |])
        use reader = new XRoadMessageReader(stream, new NameValueCollection(), Encoding.UTF8, Path.GetTempPath())
        stream.Position <- 0L

        let mutable chunk = [| |]

        reader.ReadChunkOrLine(&chunk, 5) |> should equal XRoadMessageReader.ChunkStop.NewLine
        chunk |> should equal [| 32uy; 32uy; 32uy; 32uy |]

        reader.ReadChunkOrLine(&chunk, 5) |> should equal XRoadMessageReader.ChunkStop.EndOfStream
        chunk |> should equal [| 32uy; 32uy; 32uy; 32uy |]

    [<Test>]
    let ``can handle multiple markers in a row`` () =
        use stream = new MemoryStream([| 40uy; 13uy; 10uy; 13uy; 10uy; 13uy; 10uy; 13uy; 10uy; 40uy |])
        use reader = new XRoadMessageReader(stream, new NameValueCollection(), Encoding.UTF8, Path.GetTempPath())
        stream.Position <- 0L

        let mutable chunk = [| |]

        reader.ReadChunkOrLine(&chunk, 5) |> should equal XRoadMessageReader.ChunkStop.NewLine
        chunk |> should equal [| 40uy |]

        reader.ReadChunkOrLine(&chunk, 5) |> should equal XRoadMessageReader.ChunkStop.NewLine
        chunk |> should equal [| |]

        reader.ReadChunkOrLine(&chunk, 5) |> should equal XRoadMessageReader.ChunkStop.NewLine
        chunk |> should equal [| |]

        reader.ReadChunkOrLine(&chunk, 5) |> should equal XRoadMessageReader.ChunkStop.NewLine
        chunk |> should equal [| |]

        reader.ReadChunkOrLine(&chunk, 5) |> should equal XRoadMessageReader.ChunkStop.EndOfStream
        chunk |> should equal [| 40uy |]

    [<Test>]
    let ``can handle recurring marker buffer limit`` () =
        use stream = new MemoryStream([| 40uy; 13uy; 13uy; 13uy; 13uy; 13uy; 13uy; 10uy; 33uy; 34uy; 40uy; 40uy |])
        use reader = new XRoadMessageReader(stream, new NameValueCollection(), Encoding.UTF8, Path.GetTempPath())
        stream.Position <- 0L

        let mutable chunk = [| |]

        reader.ReadChunkOrLine(&chunk, 5) |> should equal XRoadMessageReader.ChunkStop.BufferLimit
        chunk |> should equal [| 40uy; 13uy; 13uy; 13uy; 13uy |]

        reader.ReadChunkOrLine(&chunk, 5) |> should equal XRoadMessageReader.ChunkStop.NewLine
        chunk |> should equal [| 13uy |]

        reader.ReadChunkOrLine(&chunk, 5) |> should equal XRoadMessageReader.ChunkStop.EndOfStream
        chunk |> should equal [| 33uy; 34uy; 40uy; 40uy |]

[<TestFixture>]
module HeaderTests =
    let parseHeader xml xrdns =
        use stream = new MemoryStream()
        use streamWriter = new StreamWriter(stream, Encoding.UTF8)
        streamWriter.WriteLine(@"<Envelope xmlns=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:id=""http://x-road.eu/xsd/identifiers"">")
        streamWriter.WriteLine(sprintf @"<Header xmlns:xrd=""%s"">" xrdns)
        streamWriter.WriteLine(xml: string)
        streamWriter.WriteLine(@"</Header>")
        streamWriter.WriteLine(@"</Envelope>")
        streamWriter.Flush()
        stream.Position <- 0L
        use reader = new XRoadMessageReader(stream, NameValueCollection(), Encoding.UTF8, Path.GetTempPath())
        use msg = new XRoadMessage()
        reader.Read(msg, false)
        msg.Header, msg.UnresolvedHeaders, msg.Protocol

    [<Test>]
    let ``no header`` () =
        let hdr,uhs,pr = parseHeader "" NamespaceConstants.XROAD_V4
        hdr |> should be Null
        uhs.Count |> should equal 0
        pr |> should equal XRoadProtocol.Undefined

    [<Test>]
    let ``validates presence of client element`` () =
        TestDelegate(fun _ -> parseHeader "<xrd:id>test</xrd:id>" NamespaceConstants.XROAD_V4 |> ignore)
        |> should (throwWithMessage "X-Road header `client` element is mandatory.") typeof<XRoadException>

    [<Test>]
    let ``validates content of client element`` () =
        TestDelegate(fun _ -> parseHeader "<xrd:client />" NamespaceConstants.XROAD_V4 |> ignore)
        |> should (throwWithMessage "Element `http://x-road.eu/xsd/xroad.xsd:client` cannot be empty.") typeof<XRoadException>

    [<Test>]
    let ``validates objectType attribute of client element`` () =
        TestDelegate(fun _ -> parseHeader @"<xrd:client></xrd:client>" NamespaceConstants.XROAD_V4 |> ignore)
        |> should (throwWithMessage "Element `http://x-road.eu/xsd/xroad.xsd:client` must have attribute `http://x-road.eu/xsd/identifiers:objectType`.") typeof<XRoadException>

    [<Test>]
    let ``validates xRoadInstance subelement of client element`` () =
        TestDelegate(fun _ -> parseHeader @"<xrd:client id:objectType=""MEMBER""></xrd:client>" NamespaceConstants.XROAD_V4 |> ignore)
        |> should (throwWithMessage "Element `http://x-road.eu/xsd/xroad.xsd:client` must have child element `http://x-road.eu/xsd/identifiers:xRoadInstance`.") typeof<XRoadException>

    [<Test>]
    let ``validates xRoadInstance subelement of client element with unknown element`` () =
        TestDelegate(fun _ -> parseHeader @"<xrd:client id:objectType=""MEMBER""><x /></xrd:client>" NamespaceConstants.XROAD_V4 |> ignore)
        |> should (throwWithMessage "Element `http://x-road.eu/xsd/xroad.xsd:client` must have child element `http://x-road.eu/xsd/identifiers:xRoadInstance`.") typeof<XRoadException>

    [<Test>]
    let ``validates memberClass subelement of client element`` () =
        TestDelegate(fun _ -> parseHeader @"<xrd:client id:objectType=""MEMBER""><id:xRoadInstance /></xrd:client>" NamespaceConstants.XROAD_V4 |> ignore)
        |> should (throwWithMessage "Element `http://x-road.eu/xsd/xroad.xsd:client` must have child element `http://x-road.eu/xsd/identifiers:memberClass`.") typeof<XRoadException>

    [<Test>]
    let ``validates memberCode subelement of client element`` () =
        TestDelegate(fun _ -> parseHeader @"<xrd:client id:objectType=""MEMBER""><id:xRoadInstance /><id:memberClass /></xrd:client>" NamespaceConstants.XROAD_V4 |> ignore)
        |> should (throwWithMessage "Element `http://x-road.eu/xsd/xroad.xsd:client` must have child element `http://x-road.eu/xsd/identifiers:memberCode`.") typeof<XRoadException>

    [<Test>]
    let ``validates invalid subelement of client element`` () =
        TestDelegate(fun _ -> parseHeader @"<xrd:client id:objectType=""MEMBER""><id:xRoadInstance /><id:memberClass /><id:memberCode /><x /></xrd:client>" NamespaceConstants.XROAD_V4 |> ignore)
        |> should (throwWithMessage "Unexpected element `http://schemas.xmlsoap.org/soap/envelope/:x` in element `http://x-road.eu/xsd/xroad.xsd:client`.") typeof<XRoadException>

    [<Test>]
    let ``validates presence of id element`` () =
        TestDelegate(fun _ -> parseHeader @"<xrd:client id:objectType=""MEMBER""><id:xRoadInstance /><id:memberClass /><id:memberCode /></xrd:client>" NamespaceConstants.XROAD_V4 |> ignore)
        |> should (throwWithMessage "X-Road header `id` element is mandatory.") typeof<XRoadException>

    [<Test>]
    let ``validates presence of protocolVersion element`` () =
        TestDelegate(fun _ -> parseHeader @"<xrd:client id:objectType=""MEMBER""><id:xRoadInstance /><id:memberClass /><id:memberCode /></xrd:client><xrd:id />" NamespaceConstants.XROAD_V4 |> ignore)
        |> should (throwWithMessage "X-Road header `protocolVersion` element is mandatory.") typeof<XRoadException>

    [<Test>]
    let ``validates value of protocolVersion element`` () =
        TestDelegate(fun _ -> parseHeader @"<xrd:client id:objectType=""MEMBER""><id:xRoadInstance /><id:memberClass /><id:memberCode /></xrd:client><xrd:id /><xrd:protocolVersion />" NamespaceConstants.XROAD_V4 |> ignore)
        |> should (throwWithMessage "Unsupported X-Road v6 protocol version value ``.") typeof<XRoadException>

    [<Test>]
    let ``collects client mandatory values`` () =
        let xrh,_,pr = parseHeader @"<xrd:client id:objectType=""MEMBER""><id:xRoadInstance>EE</id:xRoadInstance><id:memberClass>GOV</id:memberClass><id:memberCode>12345</id:memberCode></xrd:client><xrd:id>ABCDE</xrd:id><xrd:protocolVersion>4.0</xrd:protocolVersion>" NamespaceConstants.XROAD_V4
        xrh |> should not' (be Null)
        pr |> should equal XRoadProtocol.Version40
        xrh.Client |> should not' (be Null)
        xrh.Client.XRoadInstance |> should equal "EE"
        xrh.Client.MemberClass |> should equal "GOV"
        xrh.Client.MemberCode |> should equal "12345"
        xrh.Client.ObjectType |> should equal XRoadLib.Header.XRoadObjectType.Member
        xrh.Client.SubsystemCode |> should be Null
        xrh.Id |> should equal "ABCDE"
        xrh.Issue |> should be Null
        xrh.ProtocolVersion |> should equal "4.0"
        xrh.Service |> should be Null
        xrh.UserId |> should be Null

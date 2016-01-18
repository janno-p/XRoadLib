namespace XRoadLib.Tests

open FsUnit
open NUnit.Framework
open System.Collections.Specialized
open System.IO
open System.Text
open System.Xml.Linq
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

module Util =
    let parseHeader xml xrdns =
        use stream = new MemoryStream()
        use streamWriter = new StreamWriter(stream, Encoding.UTF8)
        streamWriter.WriteLine(@"<Envelope xmlns=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:id=""http://x-road.eu/xsd/identifiers"" xmlns:repr=""http://x-road.eu/xsd/representation.xsd"">")
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

[<TestFixture>]
module XRoadHeader20Tests =
    let parseHeader xml = Util.parseHeader xml NamespaceConstants.XTEE

    let testHeader name value =
        let xhr,_,pr = parseHeader (sprintf "<xrd:%s>%s</xrd:%s>" name value name)
        xhr |> should not' (be Null)
        xhr |> should be instanceOfType<Header.IXRoadHeader20>
        pr |> should equal XRoadProtocol.Version20
        xhr, xhr :?> Header.IXRoadHeader20

    [<Test>]
    let ``can parse asutus value`` () =
        let xhr,xhr2 = testHeader "asutus" "12345"
        xhr.Client |> should not' (be Null)
        xhr.Client.MemberCode |> should equal "12345"
        xhr2.Asutus |> should equal "12345"

    [<Test>]
    let ``can parse andmekogu value`` () =
        let xhr,xhr2 = testHeader "andmekogu" "andmekogu"
        xhr.Service |> should not' (be Null)
        xhr.Service.SubsystemCode |> should equal "andmekogu"
        xhr2.Andmekogu |> should equal "andmekogu"

    [<Test>]
    let ``can parse isikukood value`` () =
        let xhr,xhr2 = testHeader "isikukood" "Kasutaja"
        xhr.UserId |> should equal "Kasutaja"
        xhr2.Isikukood |> should equal "Kasutaja"

    [<Test>]
    let ``can parse ametnik value`` () =
        let _,xhr2 = testHeader "ametnik" "Kasutaja"
        xhr2.Ametnik |> should equal "Kasutaja"

    [<Test>]
    let ``can parse id value`` () =
        let xhr,_ = testHeader "id" "hash"
        xhr.Id |> should equal "hash"

    [<Test>]
    let ``can parse nimi value`` () =
        let xhr,xhr2 = testHeader "nimi" "producer.testService.v5"
        xhr.Service |> should not' (be Null)
        xhr.Service.SubsystemCode |> should be Null
        xhr.Service.ServiceCode |> should equal "testService"
        xhr.Service.ServiceVersion |> should equal "v5"
        xhr.Service.Version |> should equal (System.Nullable(5u))
        xhr2.Nimi |> should equal "testService.v5"

    [<Test>]
    let ``can parse toimik value`` () =
        let xhr,xhr2 = testHeader "toimik" "toimik"
        xhr.Issue |> should equal "toimik"
        xhr2.Toimik |> should equal "toimik"

    [<Test>]
    let ``can parse allasutus value`` () =
        let _,xhr2 = testHeader "allasutus" "yksus"
        xhr2.Allasutus |> should equal "yksus"

    [<Test>]
    let ``can parse amet value`` () =
        let _,xhr2 = testHeader "amet" "ametikoht"
        xhr2.Amet |> should equal "ametikoht"

    [<Test>]
    let ``can parse ametniknimi value`` () =
        let _,xhr2 = testHeader "ametniknimi" "Kuldar"
        xhr2.AmetnikNimi |> should equal "Kuldar"

    [<Test>]
    let ``can parse asynkroonne "1" value`` () =
        let _,xhr2 = testHeader "asynkroonne" "1"
        xhr2.Asünkroonne |> should be True

    [<Test>]
    let ``can parse asynkroonne "true" value`` () =
        let _,xhr2 = testHeader "asynkroonne" "true"
        xhr2.Asünkroonne |> should be True

    [<Test>]
    let ``can parse asynkroonne "false" value`` () =
        let _,xhr2 = testHeader "asynkroonne" "false"
        xhr2.Asünkroonne |> should be False

    [<Test>]
    let ``can parse asynkroonne "0" value`` () =
        let _,xhr2 = testHeader "asynkroonne" "0"
        xhr2.Asünkroonne |> should be False

    [<Test>]
    let ``can parse asynkroonne "" value`` () =
        let _,xhr2 = testHeader "asynkroonne" ""
        xhr2.Asünkroonne |> should be False

    [<Test>]
    let ``can parse autentija value`` () =
        let _,xhr2 = testHeader "autentija" "Juss"
        xhr2.Autentija |> should equal "Juss"

    [<Test>]
    let ``can parse makstud value`` () =
        let _,xhr2 = testHeader "makstud" "just"
        xhr2.Makstud |> should equal "just"

    [<Test>]
    let ``can parse salastada value`` () =
        let _,xhr2 = testHeader "salastada" "sha1"
        xhr2.Salastada |> should equal "sha1"

    [<Test>]
    let ``can parse salastada_sertifikaadiga value`` () =
        let _,xhr2 = testHeader "salastada_sertifikaadiga" "bibopp"
        xhr2.SalastadaSertifikaadiga |> should equal "bibopp"

    [<Test>]
    let ``can parse salastatud value`` () =
        let _,xhr2 = testHeader "salastatud" "sha1"
        xhr2.Salastatud |> should equal "sha1"

    [<Test>]
    let ``can parse salastatud_sertifikaadiga value`` () =
        let _,xhr2 = testHeader "salastatud_sertifikaadiga" "bibopp"
        xhr2.SalastatudSertifikaadiga |> should equal "bibopp"

[<TestFixture>]
module XRoadHeader31Tests =
    let parseHeader xml = Util.parseHeader xml NamespaceConstants.XROAD

    let testHeader name value =
        let xhr,_,pr = parseHeader (sprintf "<xrd:%s>%s</xrd:%s>" name value name)
        xhr |> should not' (be Null)
        xhr |> should be instanceOfType<Header.IXRoadHeader31>
        pr |> should equal XRoadProtocol.Version31
        xhr, xhr :?> Header.IXRoadHeader31

    [<Test>]
    let ``can parse empty element`` () =
        let xhr,_,pr = parseHeader "<xrd:userId />"
        xhr |> should not' (be Null)
        xhr.UserId |> should equal ""

    [<Test>]
    let ``can parse consumer value`` () =
        let xhr,xhr3 = testHeader "consumer" "12345"
        xhr.Client |> should not' (be Null)
        xhr.Client.MemberCode |> should equal "12345"
        xhr3.Consumer |> should equal "12345"

    [<Test>]
    let ``can parse producer value`` () =
        let xhr,xhr3 = testHeader "producer" "andmekogu"
        xhr.Service |> should not' (be Null)
        xhr.Service.SubsystemCode |> should equal "andmekogu"
        xhr3.Producer |> should equal "andmekogu"

    [<Test>]
    let ``can parse userId value`` () =
        let xhr,_ = testHeader "userId" "Kasutaja"
        xhr.UserId |> should equal "Kasutaja"

    [<Test>]
    let ``can parse id value`` () =
        let xhr,_ = testHeader "id" "hash"
        xhr.Id |> should equal "hash"

    [<Test>]
    let ``can parse service value`` () =
        let xhr,xhr3 = testHeader "service" "producer.testService.v5"
        xhr.Service |> should not' (be Null)
        xhr.Service.SubsystemCode |> should be Null
        xhr.Service.ServiceCode |> should equal "testService"
        xhr.Service.ServiceVersion |> should equal "v5"
        xhr.Service.Version |> should equal (System.Nullable(5u))
        xhr3.Service |> should equal "testService.v5"

    [<Test>]
    let ``can parse issue value`` () =
        let xhr,_ = testHeader "issue" "toimik"
        xhr.Issue |> should equal "toimik"

    [<Test>]
    let ``can parse unit value`` () =
        let _,xhr3 = testHeader "unit" "yksus"
        xhr3.Unit |> should equal "yksus"

    [<Test>]
    let ``can parse position value`` () =
        let _,xhr3 = testHeader "position" "ametikoht"
        xhr3.Position |> should equal "ametikoht"

    [<Test>]
    let ``can parse userName value`` () =
        let _,xhr3 = testHeader "userName" "Kuldar"
        xhr3.UserName |> should equal "Kuldar"

    [<Test>]
    let ``can parse async "1" value`` () =
        let _,xhr3 = testHeader "async" "1"
        xhr3.Async |> should be True

    [<Test>]
    let ``can parse async "true" value`` () =
        let _,xhr3 = testHeader "async" "true"
        xhr3.Async |> should be True

    [<Test>]
    let ``can parse async "false" value`` () =
        let _,xhr3 = testHeader "async" "false"
        xhr3.Async |> should be False

    [<Test>]
    let ``can parse async "0" value`` () =
        let _,xhr3 = testHeader "async" "0"
        xhr3.Async |> should be False

    [<Test>]
    let ``can parse async "" value`` () =
        let _,xhr3 = testHeader "async" ""
        xhr3.Async |> should be False

    [<Test>]
    let ``can parse authenticator value`` () =
        let _,xhr3 = testHeader "authenticator" "Juss"
        xhr3.Authenticator |> should equal "Juss"

    [<Test>]
    let ``can parse paid value`` () =
        let _,xhr3 = testHeader "paid" "just"
        xhr3.Paid |> should equal "just"

    [<Test>]
    let ``can parse encrypt value`` () =
        let _,xhr3 = testHeader "encrypt" "sha1"
        xhr3.Encrypt |> should equal "sha1"

    [<Test>]
    let ``can parse encryptCert value`` () =
        let _,xhr3 = testHeader "encryptCert" "bibopp"
        xhr3.EncryptCert |> should equal "bibopp"

    [<Test>]
    let ``can parse encrypted value`` () =
        let _,xhr3 = testHeader "encrypted" "sha1"
        xhr3.Encrypted |> should equal "sha1"

    [<Test>]
    let ``can parse encryptedCert value`` () =
        let _,xhr3 = testHeader "encryptedCert" "bibopp"
        xhr3.EncryptedCert |> should equal "bibopp"

[<TestFixture>]
module XRoadHeader40Tests =
    let parseHeader xml = Util.parseHeader xml NamespaceConstants.XROAD_V4

    [<Test>]
    let ``no header`` () =
        let hdr,uhs,pr = parseHeader ""
        hdr |> should be Null
        uhs.Count |> should equal 0
        pr |> should equal XRoadProtocol.Undefined

    [<Test>]
    let ``validates presence of client element`` () =
        TestDelegate(fun _ -> parseHeader "<xrd:id>test</xrd:id>" |> ignore)
        |> should (throwWithMessage "X-Road header `client` element is mandatory.") typeof<XRoadException>

    [<Test>]
    let ``validates content of client element`` () =
        TestDelegate(fun _ -> parseHeader "<xrd:client />" |> ignore)
        |> should (throwWithMessage "Element `http://x-road.eu/xsd/xroad.xsd:client` cannot be empty.") typeof<XRoadException>

    [<Test>]
    let ``validates objectType attribute of client element`` () =
        TestDelegate(fun _ -> parseHeader @"<xrd:client></xrd:client>" |> ignore)
        |> should (throwWithMessage "Element `http://x-road.eu/xsd/xroad.xsd:client` must have attribute `http://x-road.eu/xsd/identifiers:objectType` value.") typeof<XRoadException>

    [<Test>]
    let ``validates xRoadInstance subelement of client element`` () =
        TestDelegate(fun _ -> parseHeader @"<xrd:client id:objectType=""MEMBER""></xrd:client>" |> ignore)
        |> should (throwWithMessage "Element `http://x-road.eu/xsd/xroad.xsd:client` must have child element `http://x-road.eu/xsd/identifiers:xRoadInstance`.") typeof<XRoadException>

    [<Test>]
    let ``validates xRoadInstance subelement of client element with unknown element`` () =
        TestDelegate(fun _ -> parseHeader @"<xrd:client id:objectType=""MEMBER""><x /></xrd:client>" |> ignore)
        |> should (throwWithMessage "Element `http://x-road.eu/xsd/xroad.xsd:client` must have child element `http://x-road.eu/xsd/identifiers:xRoadInstance`.") typeof<XRoadException>

    [<Test>]
    let ``validates memberClass subelement of client element`` () =
        TestDelegate(fun _ -> parseHeader @"<xrd:client id:objectType=""MEMBER""><id:xRoadInstance /></xrd:client>" |> ignore)
        |> should (throwWithMessage "Element `http://x-road.eu/xsd/xroad.xsd:client` must have child element `http://x-road.eu/xsd/identifiers:memberClass`.") typeof<XRoadException>

    [<Test>]
    let ``validates memberCode subelement of client element`` () =
        TestDelegate(fun _ -> parseHeader @"<xrd:client id:objectType=""MEMBER""><id:xRoadInstance /><id:memberClass /></xrd:client>" |> ignore)
        |> should (throwWithMessage "Element `http://x-road.eu/xsd/xroad.xsd:client` must have child element `http://x-road.eu/xsd/identifiers:memberCode`.") typeof<XRoadException>

    [<Test>]
    let ``validates invalid subelement of client element`` () =
        TestDelegate(fun _ -> parseHeader @"<xrd:client id:objectType=""MEMBER""><id:xRoadInstance /><id:memberClass /><id:memberCode /><x /></xrd:client>" |> ignore)
        |> should (throwWithMessage "Unexpected element `http://schemas.xmlsoap.org/soap/envelope/:x` in element `http://x-road.eu/xsd/xroad.xsd:client`.") typeof<XRoadException>

    [<Test>]
    let ``validates presence of id element`` () =
        TestDelegate(fun _ -> parseHeader @"<xrd:client id:objectType=""MEMBER""><id:xRoadInstance /><id:memberClass /><id:memberCode /></xrd:client>" |> ignore)
        |> should (throwWithMessage "X-Road header `id` element is mandatory.") typeof<XRoadException>

    [<Test>]
    let ``validates presence of protocolVersion element`` () =
        TestDelegate(fun _ -> parseHeader @"<xrd:client id:objectType=""MEMBER""><id:xRoadInstance /><id:memberClass /><id:memberCode /></xrd:client><xrd:id />" |> ignore)
        |> should (throwWithMessage "X-Road header `protocolVersion` element is mandatory.") typeof<XRoadException>

    [<Test>]
    let ``validates value of protocolVersion element`` () =
        TestDelegate(fun _ -> parseHeader @"<xrd:client id:objectType=""MEMBER""><id:xRoadInstance /><id:memberClass /><id:memberCode /></xrd:client><xrd:id /><xrd:protocolVersion />" |> ignore)
        |> should (throwWithMessage "Unsupported X-Road v6 protocol version value ``.") typeof<XRoadException>

    let minimalValidHeader = sprintf @"<xrd:client id:objectType=""MEMBER""><id:xRoadInstance>EE</id:xRoadInstance><id:memberClass>GOV</id:memberClass><id:memberCode>12345</id:memberCode></xrd:client><xrd:id>ABCDE</xrd:id><xrd:protocolVersion>4.0</xrd:protocolVersion>%s"

    [<Test>]
    let ``collects client mandatory values`` () =
        let xrh,_,pr = parseHeader (minimalValidHeader "")
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

    [<Test>]
    let ``collects unqualified soap header elements, but cannot detect protocol version`` () =
        let xrh,uhs,pr = parseHeader @"<x><test>bla</test></x><y /><z />"
        xrh |> should be Null
        pr |> should equal XRoadProtocol.Undefined
        uhs.Count |> should equal 3
        let el_x = uhs |> Seq.tryFind (fun x -> x.Name.LocalName = "x")
        el_x.IsSome |> should be True
        el_x |> should not' (be Null)
        let el_x_test = el_x.Value.Element(XName.Get("test", NamespaceConstants.SOAP_ENV))
        el_x_test |> should not' (be Null)
        el_x_test.Value |> should equal "bla"
        uhs |> Seq.tryFind (fun x -> x.Name.LocalName = "y") |> Option.isSome |> should be True
        uhs |> Seq.tryFind (fun x -> x.Name.LocalName = "z") |> Option.isSome |> should be True

    [<Test>]
    let ``collects unqualified soap headers that are mixed with X-Road elements`` () =
        let xrh,uhs,pr = parseHeader @"<x /><xrd:client id:objectType=""MEMBER""><id:xRoadInstance>EE</id:xRoadInstance><id:memberClass>GOV</id:memberClass><id:memberCode>12345</id:memberCode></xrd:client><y /><xrd:id>ABCDE</xrd:id><xrd:protocolVersion>4.0</xrd:protocolVersion><z />"
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
        uhs.Count |> should equal 3
        uhs |> Seq.tryFind (fun x -> x.Name.LocalName = "x") |> Option.isSome |> should be True
        uhs |> Seq.tryFind (fun x -> x.Name.LocalName = "y") |> Option.isSome |> should be True
        uhs |> Seq.tryFind (fun x -> x.Name.LocalName = "z") |> Option.isSome |> should be True

    [<Test>]
    let ``unrecognized X-Road header element`` () =
        TestDelegate(fun _ -> parseHeader @"<xrd:x />" |> ignore)
        |> should (throwWithMessage "Unexpected X-Road header element `http://x-road.eu/xsd/xroad.xsd:x`.") typeof<XRoadException>

    [<Test>]
    let ``empty service element`` () =
        TestDelegate(fun _ -> parseHeader (minimalValidHeader @"<xrd:service />") |> ignore)
        |> should (throwWithMessage "Element `http://x-road.eu/xsd/xroad.xsd:service` cannot be empty.") typeof<XRoadException>

    [<Test>]
    let ``missing objectType attribute for service element`` () =
        TestDelegate(fun _ -> parseHeader (minimalValidHeader @"<xrd:service></xrd:service>") |> ignore)
        |> should (throwWithMessage "Element `http://x-road.eu/xsd/xroad.xsd:service` must have attribute `http://x-road.eu/xsd/identifiers:objectType` value.") typeof<XRoadException>

    [<Test>]
    let ``missing xRoadInstance element for service element`` () =
        TestDelegate(fun _ -> parseHeader (minimalValidHeader @"<xrd:service id:objectType=""SERVICE""></xrd:service>") |> ignore)
        |> should (throwWithMessage "Element `http://x-road.eu/xsd/xroad.xsd:service` must have child element `http://x-road.eu/xsd/identifiers:xRoadInstance`.") typeof<XRoadException>

    [<Test>]
    let ``invalid element in service element`` () =
        TestDelegate(fun _ -> parseHeader (minimalValidHeader @"<xrd:service id:objectType=""SERVICE""><x /></xrd:service>") |> ignore)
        |> should (throwWithMessage "Element `http://x-road.eu/xsd/xroad.xsd:service` must have child element `http://x-road.eu/xsd/identifiers:xRoadInstance`.") typeof<XRoadException>

    [<Test>]
    let ``missing memberClass element for service element`` () =
        TestDelegate(fun _ -> parseHeader (minimalValidHeader @"<xrd:service id:objectType=""SERVICE""><id:xRoadInstance /></xrd:service>") |> ignore)
        |> should (throwWithMessage "Element `http://x-road.eu/xsd/xroad.xsd:service` must have child element `http://x-road.eu/xsd/identifiers:memberClass`.") typeof<XRoadException>

    [<Test>]
    let ``missing memberCode element for service element`` () =
        TestDelegate(fun _ -> parseHeader (minimalValidHeader @"<xrd:service id:objectType=""SERVICE""><id:xRoadInstance /><id:memberClass /></xrd:service>") |> ignore)
        |> should (throwWithMessage "Element `http://x-road.eu/xsd/xroad.xsd:service` must have child element `http://x-road.eu/xsd/identifiers:memberCode`.") typeof<XRoadException>

    [<Test>]
    let ``missing serviceCode element for service element`` () =
        TestDelegate(fun _ -> parseHeader (minimalValidHeader @"<xrd:service id:objectType=""SERVICE""><id:xRoadInstance /><id:memberClass /><id:memberCode /></xrd:service>") |> ignore)
        |> should (throwWithMessage "Element `http://x-road.eu/xsd/xroad.xsd:service` must have child element `http://x-road.eu/xsd/identifiers:serviceCode`.") typeof<XRoadException>

    [<Test>]
    let ``read minimal group of elements for service element`` () =
        let xhr,_,_ = parseHeader (minimalValidHeader @"<xrd:service id:objectType=""SERVICE""><id:xRoadInstance /><id:memberClass /><id:memberCode /><id:serviceCode /></xrd:service>")
        xhr |> should not' (be Null)
        xhr.Service |> should not' (be Null)
        xhr.Service.XRoadInstance |> should equal ""
        xhr.Service.MemberClass |> should equal ""
        xhr.Service.MemberCode |> should equal ""
        xhr.Service.SubsystemCode |> should be Null
        xhr.Service.ServiceCode |> should equal ""
        xhr.Service.ServiceVersion |> should be Null

    [<Test>]
    let ``read all elements for service element`` () =
        let xhr,_,_ = parseHeader (minimalValidHeader @"<xrd:service id:objectType=""SERVICE""><id:xRoadInstance /><id:memberClass /><id:memberCode /><id:subsystemCode /><id:serviceCode /><id:serviceVersion /></xrd:service>")
        xhr |> should not' (be Null)
        xhr.Service |> should not' (be Null)
        xhr.Service.XRoadInstance |> should equal ""
        xhr.Service.MemberClass |> should equal ""
        xhr.Service.MemberCode |> should equal ""
        xhr.Service.SubsystemCode |> should equal ""
        xhr.Service.ServiceCode |> should equal ""
        xhr.Service.ServiceVersion |> should equal ""

    [<Test>]
    let ``optional parameter at wrong position for service element`` () =
        TestDelegate(fun _ -> parseHeader (minimalValidHeader @"<xrd:service id:objectType=""SERVICE""><id:xRoadInstance /><id:memberClass /><id:memberCode /><id:serviceCode /><id:subsystemCode /></xrd:service>") |> ignore)
        |> should (throwWithMessage "Unexpected element `http://x-road.eu/xsd/identifiers:subsystemCode` in element `http://x-road.eu/xsd/xroad.xsd:service`.") typeof<XRoadException>

    [<Test>]
    let ``read simple optional element values`` () =
        let xhr,_,_ = parseHeader (minimalValidHeader @"<xrd:userId>Kalle</xrd:userId><xrd:issue>TOIMIK</xrd:issue>")
        xhr |> should not' (be Null)
        xhr.UserId |> should equal "Kalle"
        xhr.Issue |> should equal "TOIMIK"

    [<Test>]
    let ``empty centralService element`` () =
        TestDelegate(fun _ -> parseHeader (minimalValidHeader @"<xrd:centralService />") |> ignore)
        |> should (throwWithMessage "Element `http://x-road.eu/xsd/xroad.xsd:centralService` cannot be empty.") typeof<XRoadException>

    [<Test>]
    let ``missing objectType attribute for centralService element`` () =
        TestDelegate(fun _ -> parseHeader (minimalValidHeader @"<xrd:centralService></xrd:centralService>") |> ignore)
        |> should (throwWithMessage "Element `http://x-road.eu/xsd/xroad.xsd:centralService` must have attribute `http://x-road.eu/xsd/identifiers:objectType` value.") typeof<XRoadException>

    [<Test>]
    let ``missing xRoadInstance element for centralService element`` () =
        TestDelegate(fun _ -> parseHeader (minimalValidHeader @"<xrd:centralService id:objectType=""CENTRALSERVICE""></xrd:centralService>") |> ignore)
        |> should (throwWithMessage "Element `http://x-road.eu/xsd/xroad.xsd:centralService` must have child element `http://x-road.eu/xsd/identifiers:xRoadInstance`.") typeof<XRoadException>

    [<Test>]
    let ``missing serviceCode element for centralService element`` () =
        TestDelegate(fun _ -> parseHeader (minimalValidHeader @"<xrd:centralService id:objectType=""CENTRALSERVICE""><id:xRoadInstance /></xrd:centralService>") |> ignore)
        |> should (throwWithMessage "Element `http://x-road.eu/xsd/xroad.xsd:centralService` must have child element `http://x-road.eu/xsd/identifiers:serviceCode`.") typeof<XRoadException>

    [<Test>]
    let ``valid centralService element`` () =
        let xhr,_,_ = parseHeader (minimalValidHeader @"<xrd:centralService id:objectType=""CENTRALSERVICE""><id:xRoadInstance>FI</id:xRoadInstance><id:serviceCode>fun</id:serviceCode></xrd:centralService>")
        xhr |> should not' (be Null)
        xhr |> should be instanceOfType<XRoadLib.Header.IXRoadHeader40>
        let xhr4 = xhr :?> XRoadLib.Header.IXRoadHeader40
        xhr4.CentralService |> should not' (be Null)
        xhr4.CentralService.ObjectType |> should equal XRoadLib.Header.XRoadObjectType.CentralService
        xhr4.CentralService.XRoadInstance |> should equal "FI"
        xhr4.CentralService.ServiceCode |> should equal "fun"

    [<Test>]
    let ``emtpy representedParty element`` () =
        TestDelegate(fun _ -> parseHeader (minimalValidHeader @"<repr:representedParty />") |> ignore)
        |> should (throwWithMessage "Element `http://x-road.eu/xsd/representation.xsd:representedParty` cannot be empty.") typeof<XRoadException>

    [<Test>]
    let ``element partyCode is required for representedParty element`` () =
        TestDelegate(fun _ -> parseHeader (minimalValidHeader @"<repr:representedParty></repr:representedParty>") |> ignore)
        |> should (throwWithMessage "Element `http://x-road.eu/xsd/representation.xsd:representedParty` must have child element `http://x-road.eu/xsd/representation.xsd:partyCode`.") typeof<XRoadException>

    [<Test>]
    let ``wrong element order for representedParty element`` () =
        TestDelegate(fun _ -> parseHeader (minimalValidHeader @"<repr:representedParty><repr:partyCode /><repr:partyClass /></repr:representedParty>") |> ignore)
        |> should (throwWithMessage "Unexpected element `http://x-road.eu/xsd/representation.xsd:partyClass` in element `http://x-road.eu/xsd/representation.xsd:representedParty`.") typeof<XRoadException>

    [<Test>]
    let ``can handle missing optional element for representedParty element`` () =
        let xhr,_,_ = parseHeader (minimalValidHeader @"<repr:representedParty><repr:partyCode /></repr:representedParty>")
        xhr |> should not' (be Null)
        xhr |> should be instanceOfType<XRoadLib.Header.IXRoadHeader40>
        let xhr4 = xhr :?> XRoadLib.Header.IXRoadHeader40
        xhr4.RepresentedParty |> should not' (be Null)
        xhr4.RepresentedParty.Class |> should be Null
        xhr4.RepresentedParty.Code |> should equal ""

    [<Test>]
    let ``can handle optional element value for representedParty element`` () =
        let xhr,_,_ = parseHeader (minimalValidHeader @"<repr:representedParty><repr:partyClass>CLS</repr:partyClass><repr:partyCode>COD</repr:partyCode></repr:representedParty>")
        xhr |> should not' (be Null)
        xhr |> should be instanceOfType<XRoadLib.Header.IXRoadHeader40>
        let xhr4 = xhr :?> XRoadLib.Header.IXRoadHeader40
        xhr4.RepresentedParty |> should not' (be Null)
        xhr4.RepresentedParty.Class |> should equal "CLS"
        xhr4.RepresentedParty.Code |> should equal "COD"

    [<Test>]
    let ``recognizes v4.0 protocol from representedParty element`` () =
        TestDelegate(fun _ -> parseHeader @"<repr:representedParty><repr:partyClass>CLS</repr:partyClass><repr:partyCode>COD</repr:partyCode></repr:representedParty>" |> ignore)
        |> should (throwWithMessage "X-Road header `client` element is mandatory.") typeof<XRoadException>

    [<Test>]
    let ``wrong protocol is left unresolved`` () =
        let xhr,uhs,pr = parseHeader (minimalValidHeader @"<x:userId xmlns:x=""http://x-road.ee/xsd/x-road.xsd"">Mr. X</x:userId>")
        xhr |> should not' (be Null)
        pr |> should equal XRoadProtocol.Version40
        uhs.Count |> should equal 1
        uhs.[0].Name.LocalName |> should equal "userId"
        uhs.[0].Name.NamespaceName |> should equal NamespaceConstants.XROAD

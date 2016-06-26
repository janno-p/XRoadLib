namespace XRoadLib.Tests.Serialization
{
/*

[<TestFixture>]
module XRoadHeader40Tests =
    let parseHeader xml = Util.parseHeader xml NamespaceConstants.XROAD_V4

    [<Test>]
    let ``no header`` () =
        let hdr,uhs,pr = parseHeader ""
        hdr |> should be Null
        uhs.Count |> should equal 0
        pr |> should be Null

    [<Test>]
    let ``validates presence of client element`` () =
        TestDelegate(fun _ -> parseHeader "<xrd:id>test</xrd:id>" |> ignore)
        |> should (throwWithMessage "X-Road header `client` element is mandatory.") typeof<XRoadException>

    [<Test>]
    let ``validates content of client element`` () =
        TestDelegate(fun _ -> parseHeader "<xrd:client />" |> ignore)
        |> should (throwWithMessage "Element `{http://x-road.eu/xsd/xroad.xsd}client` cannot be empty.") typeof<XRoadException>

    [<Test>]
    let ``validates objectType attribute of client element`` () =
        TestDelegate(fun _ -> parseHeader @"<xrd:client></xrd:client>" |> ignore)
        |> should (throwWithMessage "Element `{http://x-road.eu/xsd/xroad.xsd}client` must have attribute `{http://x-road.eu/xsd/identifiers}objectType` value.") typeof<XRoadException>

    [<Test>]
    let ``validates xRoadInstance subelement of client element`` () =
        TestDelegate(fun _ -> parseHeader @"<xrd:client id:objectType=""MEMBER""></xrd:client>" |> ignore)
        |> should (throwWithMessage "Element `{http://x-road.eu/xsd/xroad.xsd}client` must have child element `{http://x-road.eu/xsd/identifiers}xRoadInstance`.") typeof<XRoadException>

    [<Test>]
    let ``validates xRoadInstance subelement of client element with unknown element`` () =
        TestDelegate(fun _ -> parseHeader @"<xrd:client id:objectType=""MEMBER""><x /></xrd:client>" |> ignore)
        |> should (throwWithMessage "Element `{http://x-road.eu/xsd/xroad.xsd}client` must have child element `{http://x-road.eu/xsd/identifiers}xRoadInstance`.") typeof<XRoadException>

    [<Test>]
    let ``validates memberClass subelement of client element`` () =
        TestDelegate(fun _ -> parseHeader @"<xrd:client id:objectType=""MEMBER""><id:xRoadInstance /></xrd:client>" |> ignore)
        |> should (throwWithMessage "Element `{http://x-road.eu/xsd/xroad.xsd}client` must have child element `{http://x-road.eu/xsd/identifiers}memberClass`.") typeof<XRoadException>

    [<Test>]
    let ``validates memberCode subelement of client element`` () =
        TestDelegate(fun _ -> parseHeader @"<xrd:client id:objectType=""MEMBER""><id:xRoadInstance /><id:memberClass /></xrd:client>" |> ignore)
        |> should (throwWithMessage "Element `{http://x-road.eu/xsd/xroad.xsd}client` must have child element `{http://x-road.eu/xsd/identifiers}memberCode`.") typeof<XRoadException>

    [<Test>]
    let ``validates invalid subelement of client element`` () =
        TestDelegate(fun _ -> parseHeader @"<xrd:client id:objectType=""MEMBER""><id:xRoadInstance /><id:memberClass /><id:memberCode /><x /></xrd:client>" |> ignore)
        |> should (throwWithMessage "Unexpected element `{http://schemas.xmlsoap.org/soap/envelope/}x` in element `{http://x-road.eu/xsd/xroad.xsd}client`.") typeof<XRoadException>

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
        pr |> should be (sameAs Globals.XRoadProtocol40)
        xrh.Client |> should not' (be Null)
        xrh.Client.XRoadInstance |> should equal "EE"
        xrh.Client.MemberClass |> should equal "GOV"
        xrh.Client.MemberCode |> should equal "12345"
        xrh.Client.ObjectType |> should equal XRoadObjectType.Member
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
        pr |> should be Null
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
        pr |> should be (sameAs Globals.XRoadProtocol40)
        xrh.Client |> should not' (be Null)
        xrh.Client.XRoadInstance |> should equal "EE"
        xrh.Client.MemberClass |> should equal "GOV"
        xrh.Client.MemberCode |> should equal "12345"
        xrh.Client.ObjectType |> should equal XRoadObjectType.Member
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
        |> should (throwWithMessage "Unexpected X-Road header element `{http://x-road.eu/xsd/xroad.xsd}x`.") typeof<XRoadException>

    [<Test>]
    let ``empty service element`` () =
        TestDelegate(fun _ -> parseHeader (minimalValidHeader @"<xrd:service />") |> ignore)
        |> should (throwWithMessage "Element `{http://x-road.eu/xsd/xroad.xsd}service` cannot be empty.") typeof<XRoadException>

    [<Test>]
    let ``missing objectType attribute for service element`` () =
        TestDelegate(fun _ -> parseHeader (minimalValidHeader @"<xrd:service></xrd:service>") |> ignore)
        |> should (throwWithMessage "Element `{http://x-road.eu/xsd/xroad.xsd}service` must have attribute `{http://x-road.eu/xsd/identifiers}objectType` value.") typeof<XRoadException>

    [<Test>]
    let ``missing xRoadInstance element for service element`` () =
        TestDelegate(fun _ -> parseHeader (minimalValidHeader @"<xrd:service id:objectType=""SERVICE""></xrd:service>") |> ignore)
        |> should (throwWithMessage "Element `{http://x-road.eu/xsd/xroad.xsd}service` must have child element `{http://x-road.eu/xsd/identifiers}xRoadInstance`.") typeof<XRoadException>

    [<Test>]
    let ``invalid element in service element`` () =
        TestDelegate(fun _ -> parseHeader (minimalValidHeader @"<xrd:service id:objectType=""SERVICE""><x /></xrd:service>") |> ignore)
        |> should (throwWithMessage "Element `{http://x-road.eu/xsd/xroad.xsd}service` must have child element `{http://x-road.eu/xsd/identifiers}xRoadInstance`.") typeof<XRoadException>

    [<Test>]
    let ``missing memberClass element for service element`` () =
        TestDelegate(fun _ -> parseHeader (minimalValidHeader @"<xrd:service id:objectType=""SERVICE""><id:xRoadInstance /></xrd:service>") |> ignore)
        |> should (throwWithMessage "Element `{http://x-road.eu/xsd/xroad.xsd}service` must have child element `{http://x-road.eu/xsd/identifiers}memberClass`.") typeof<XRoadException>

    [<Test>]
    let ``missing memberCode element for service element`` () =
        TestDelegate(fun _ -> parseHeader (minimalValidHeader @"<xrd:service id:objectType=""SERVICE""><id:xRoadInstance /><id:memberClass /></xrd:service>") |> ignore)
        |> should (throwWithMessage "Element `{http://x-road.eu/xsd/xroad.xsd}service` must have child element `{http://x-road.eu/xsd/identifiers}memberCode`.") typeof<XRoadException>

    [<Test>]
    let ``missing serviceCode element for service element`` () =
        TestDelegate(fun _ -> parseHeader (minimalValidHeader @"<xrd:service id:objectType=""SERVICE""><id:xRoadInstance /><id:memberClass /><id:memberCode /></xrd:service>") |> ignore)
        |> should (throwWithMessage "Element `{http://x-road.eu/xsd/xroad.xsd}service` must have child element `{http://x-road.eu/xsd/identifiers}serviceCode`.") typeof<XRoadException>

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
        |> should (throwWithMessage "Unexpected element `{http://x-road.eu/xsd/identifiers}subsystemCode` in element `{http://x-road.eu/xsd/xroad.xsd}service`.") typeof<XRoadException>

    [<Test>]
    let ``read simple optional element values`` () =
        let xhr,_,_ = parseHeader (minimalValidHeader @"<xrd:userId>Kalle</xrd:userId><xrd:issue>TOIMIK</xrd:issue>")
        xhr |> should not' (be Null)
        xhr.UserId |> should equal "Kalle"
        xhr.Issue |> should equal "TOIMIK"

    [<Test>]
    let ``empty centralService element`` () =
        TestDelegate(fun _ -> parseHeader (minimalValidHeader @"<xrd:centralService />") |> ignore)
        |> should (throwWithMessage "Element `{http://x-road.eu/xsd/xroad.xsd}centralService` cannot be empty.") typeof<XRoadException>

    [<Test>]
    let ``missing objectType attribute for centralService element`` () =
        TestDelegate(fun _ -> parseHeader (minimalValidHeader @"<xrd:centralService></xrd:centralService>") |> ignore)
        |> should (throwWithMessage "Element `{http://x-road.eu/xsd/xroad.xsd}centralService` must have attribute `{http://x-road.eu/xsd/identifiers}objectType` value.") typeof<XRoadException>

    [<Test>]
    let ``missing xRoadInstance element for centralService element`` () =
        TestDelegate(fun _ -> parseHeader (minimalValidHeader @"<xrd:centralService id:objectType=""CENTRALSERVICE""></xrd:centralService>") |> ignore)
        |> should (throwWithMessage "Element `{http://x-road.eu/xsd/xroad.xsd}centralService` must have child element `{http://x-road.eu/xsd/identifiers}xRoadInstance`.") typeof<XRoadException>

    [<Test>]
    let ``missing serviceCode element for centralService element`` () =
        TestDelegate(fun _ -> parseHeader (minimalValidHeader @"<xrd:centralService id:objectType=""CENTRALSERVICE""><id:xRoadInstance /></xrd:centralService>") |> ignore)
        |> should (throwWithMessage "Element `{http://x-road.eu/xsd/xroad.xsd}centralService` must have child element `{http://x-road.eu/xsd/identifiers}serviceCode`.") typeof<XRoadException>

    [<Test>]
    let ``valid centralService element`` () =
        let xhr,_,_ = parseHeader (minimalValidHeader @"<xrd:centralService id:objectType=""CENTRALSERVICE""><id:xRoadInstance>FI</id:xRoadInstance><id:serviceCode>fun</id:serviceCode></xrd:centralService>")
        xhr |> should not' (be Null)
        xhr |> should be instanceOfType<IXRoadHeader40>
        let xhr4 = xhr :?> IXRoadHeader40
        xhr4.CentralService |> should not' (be Null)
        xhr4.CentralService.ObjectType |> should equal XRoadObjectType.CentralService
        xhr4.CentralService.XRoadInstance |> should equal "FI"
        xhr4.CentralService.ServiceCode |> should equal "fun"

    [<Test>]
    let ``emtpy representedParty element`` () =
        TestDelegate(fun _ -> parseHeader (minimalValidHeader @"<repr:representedParty />") |> ignore)
        |> should (throwWithMessage "Element `{http://x-road.eu/xsd/representation.xsd}representedParty` cannot be empty.") typeof<XRoadException>

    [<Test>]
    let ``element partyCode is required for representedParty element`` () =
        TestDelegate(fun _ -> parseHeader (minimalValidHeader @"<repr:representedParty></repr:representedParty>") |> ignore)
        |> should (throwWithMessage "Element `{http://x-road.eu/xsd/representation.xsd}representedParty` must have child element `{http://x-road.eu/xsd/representation.xsd}partyCode`.") typeof<XRoadException>

    [<Test>]
    let ``wrong element order for representedParty element`` () =
        TestDelegate(fun _ -> parseHeader (minimalValidHeader @"<repr:representedParty><repr:partyCode /><repr:partyClass /></repr:representedParty>") |> ignore)
        |> should (throwWithMessage "Unexpected element `{http://x-road.eu/xsd/representation.xsd}partyClass` in element `{http://x-road.eu/xsd/representation.xsd}representedParty`.") typeof<XRoadException>

    [<Test>]
    let ``can handle missing optional element for representedParty element`` () =
        let xhr,_,_ = parseHeader (minimalValidHeader @"<repr:representedParty><repr:partyCode /></repr:representedParty>")
        xhr |> should not' (be Null)
        xhr |> should be instanceOfType<IXRoadHeader40>
        let xhr4 = xhr :?> IXRoadHeader40
        xhr4.RepresentedParty |> should not' (be Null)
        xhr4.RepresentedParty.Class |> should be Null
        xhr4.RepresentedParty.Code |> should equal ""

    [<Test>]
    let ``can handle optional element value for representedParty element`` () =
        let xhr,_,_ = parseHeader (minimalValidHeader @"<repr:representedParty><repr:partyClass>CLS</repr:partyClass><repr:partyCode>COD</repr:partyCode></repr:representedParty>")
        xhr |> should not' (be Null)
        xhr |> should be instanceOfType<IXRoadHeader40>
        let xhr4 = xhr :?> IXRoadHeader40
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
        pr |> should be (sameAs Globals.XRoadProtocol40)
        uhs.Count |> should equal 1
        uhs.[0].Name.LocalName |> should equal "userId"
        uhs.[0].Name.NamespaceName |> should equal NamespaceConstants.XROAD
*/
}
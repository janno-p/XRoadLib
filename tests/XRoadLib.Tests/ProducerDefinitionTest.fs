namespace XRoadLib.Tests

open FsUnit
open NUnit.Framework
open System
open System.IO
open System.Linq
open System.Xml.Linq
open XRoadLib

[<TestFixture>]
module ProducerDefinitionTest =
    let contractAssembly = typeof<XRoadLib.Tests.Contract.Class1>.Assembly

    let xn nm = XName.Get(nm)
    let wsdl nm = XName.Get(nm, NamespaceConstants.WSDL)
    let soap nm = XName.Get(nm, NamespaceConstants.SOAP)
    let xrd nm = XName.Get(nm, NamespaceConstants.XROAD)
    let xtee nm = XName.Get(nm, NamespaceConstants.XTEE)
    let xml nm = XName.Get(nm, NamespaceConstants.XML)

    let attributeValueShouldEqual value (a: XAttribute) =
        a |> should not' (be Null)
        a.Value |> should equal value

    let shouldMatchInCommonParts (doc: XContainer) (xrdns: string -> XName) =
        let root = doc.Elements(wsdl "definitions").SingleOrDefault()
        root |> should not' (be Null)

        let service = root.Elements(wsdl "service").SingleOrDefault()
        service |> should not' (be Null)
        service.Attributes().Count() |> should equal 1
        service.Attribute(xn "name") |> attributeValueShouldEqual "TestService"
        service.Elements().Count() |> should equal 1

        let port = service.Elements(wsdl "port").SingleOrDefault()
        port |> should not' (be Null)
        port.Attributes().Count() |> should equal 2
        port.Attribute(xn "name") |> attributeValueShouldEqual "TestPort"
        port.Attribute(xn "binding") |> attributeValueShouldEqual "TestBinding"

        let okValues = [soap "address"; xrdns "address"; xrdns "title"]
        port.Elements() |> Seq.filter (fun e -> okValues |> List.exists((=) e.Name) |> not) |> should be Empty

        let soapAddress = port.Elements(soap "address").SingleOrDefault()
        soapAddress |> should not' (be Null)
        soapAddress.IsEmpty |> should equal true
        soapAddress.Attributes().Count() |> should equal 1

        port

    let getDocument (definition: ProducerDefinition) =
        use stream = new MemoryStream()
        definition.SaveTo(stream)
        stream.Position <- 0L
        XDocument.Load(stream)

    [<Test>]
    let ``empty service description`` () =
        let doc = ProducerDefinition(contractAssembly, XRoadProtocol.Version31, Nullable(1u)) |> getDocument
        let port = shouldMatchInCommonParts doc xrd

        let address = port.Elements(xrd "address").SingleOrDefault()
        address |> should not' (be Null)
        address.IsEmpty |> should equal true
        address.Attributes().Count() |> should equal 1
        address.Attribute(xn "producer") |> attributeValueShouldEqual "test-producer"

        port.Elements(soap "address").Single().Attribute(xn "location") |> attributeValueShouldEqual "http://TURVASERVER/cgi-bin/consumer_proxy"

    [<Test>]
    let ``empty legacy format service description`` () =
        let doc = ProducerDefinition(contractAssembly, XRoadProtocol.Version20, Nullable(1u), "test") |> getDocument
        let port = shouldMatchInCommonParts doc xtee

        let address = port.Elements(xtee "address").SingleOrDefault()
        address |> should not' (be Null)
        address.IsEmpty |> should equal true
        address.Attributes().Count() |> should equal 1
        address.Attribute(xn "producer") |> attributeValueShouldEqual "test"

        port.Elements(soap "address").Single().Attribute(xn "location") |> attributeValueShouldEqual "http://TURVASERVER/cgi-bin/consumer_proxy"

    [<Test>]
    let ``should define service location if given`` () =
        let url = "http://securityserveruri"
        let definition = ProducerDefinition(contractAssembly, XRoadProtocol.Version31, Nullable(1u))
        definition.set_Location(url)
        let doc = definition |> getDocument
        let port = shouldMatchInCommonParts doc xrd
        port.Elements(soap "address").Single().Attribute(xn "location") |> attributeValueShouldEqual url

    [<Test>]
    let ``should define service title`` () =
        let definition = ProducerDefinition(contractAssembly, XRoadProtocol.Version31, Nullable(1u))

        let doc = definition |> getDocument
        let port = shouldMatchInCommonParts doc xrd

        let titleElements = port.Elements(xrd "title") |> List.ofSeq
        titleElements.Length |> should equal 4

        let yesCode, noCode = titleElements |> List.partition (fun x -> x.Attributes().Any())

        yesCode.Length |> should equal 3
        match yesCode with
        | [ en; et; pt ] ->
            yesCode |> List.iter (fun x -> x |> should not' (be Null))
            en.Value |> should equal "XRoadLib test producer"
            en.Attribute(xml "lang") |> attributeValueShouldEqual "en"
            et.Value |> should equal "XRoadLib test andmekogu"
            et.Attribute(xml "lang") |> attributeValueShouldEqual "et"
            pt.Value |> should equal "Portugalikeelne loba ..."
            pt.Attribute(xml "lang") |> attributeValueShouldEqual "pt"
        | _ -> failwith "never"

        noCode.Length |> should equal 1
        noCode.Head |> should not' (be Null)
        noCode.Head.Value |> should equal "Ilma keeleta palun"

    [<Test>]
    let ``can define service title for legacy service`` () =
        let definition = ProducerDefinition(contractAssembly, XRoadProtocol.Version20, Nullable(1u))

        let doc = definition |> getDocument
        let port = shouldMatchInCommonParts doc xtee

        let titleElements = port.Elements(xtee "title") |> List.ofSeq
        titleElements.Length |> should equal 4

        let yesCode, noCode = titleElements |> List.partition (fun x -> x.Attributes().Any())

        yesCode.Length |> should equal 3
        match yesCode with
        | [ en; et; pt ] ->
            yesCode |> List.iter (fun x -> x |> should not' (be Null))
            en.Value |> should equal "XRoadLib test producer"
            en.Attribute(xml "lang") |> attributeValueShouldEqual "en"
            et.Value |> should equal "XRoadLib test andmekogu"
            et.Attribute(xml "lang") |> attributeValueShouldEqual "et"
            pt.Value |> should equal "Portugalikeelne loba ..."
            pt.Attribute(xml "lang") |> attributeValueShouldEqual "pt"
        | _ -> failwith "never"

        noCode.Length |> should equal 1
        noCode.Head |> should not' (be Null)
        noCode.Head.Value |> should equal "Ilma keeleta palun"

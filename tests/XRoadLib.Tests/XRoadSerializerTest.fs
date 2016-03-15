namespace XRoadLib.Tests

open FsUnit
open NUnit.Framework
open System
open System.IO
open System.Text
open System.Xml
open XRoadLib
open XRoadLib.Protocols.Headers
open XRoadLib.Schema
open XRoadLib.Serialization
open XRoadLib.Serialization.Mapping
open XRoadLib.Serialization.Template
open XRoadLib.Tests.Contract

[<TestFixture>]
module XRoadSerializerTest =
    type X<'T>() =
        member __.Method(t: 'X) =
            ()

    let serializeWithContext<'T> elementName (value: 'T) dtoVersion addEnvelope isMultipart f =
        use message = new XRoadMessage(Globals.XRoadProtocol20, XRoadHeader20(), IsMultipartContainer=true, BinaryMode=BinaryMode.Attachment)

        use stream = new MemoryStream()
        use writer = new XmlTextWriter(stream, Encoding.UTF8)

        if addEnvelope then
            writer.WriteStartElement("Envelope")
            writer.WriteAttributeString("xmlns", PrefixConstants.SOAP_ENC, NamespaceConstants.XMLNS, NamespaceConstants.SOAP_ENC)
            writer.WriteAttributeString("xmlns", PrefixConstants.XSI, NamespaceConstants.XMLNS, NamespaceConstants.XSI)
            writer.WriteAttributeString("xmlns", PrefixConstants.XSD, NamespaceConstants.XMLNS, NamespaceConstants.XSD)
            writer.WriteAttributeString("xmlns", "tns", NamespaceConstants.XMLNS, Globals.XRoadProtocol20.ProducerNamespace)

        writer.WriteStartElement(elementName)

        let propType = typedefof<X<_>>.MakeGenericType(typeof<'T>)
        let paramInfo = propType.GetMethod("Method").GetParameters().[0]

        let arrayItemDefinition = if typeof<'T>.IsArray then ArrayItemDefinition(Name = System.Xml.Linq.XName.Get("item")) else null

        let typeMap = Globals.XRoadProtocol20.GetSerializerCache(Nullable(dtoVersion)).GetTypeMap(typeof<'T>)
        typeMap.Serialize(writer, XRoadXmlTemplate.EmptyNode, value, RequestValueDefinition(paramInfo, null, ArrayItemDefinition = arrayItemDefinition), message)

        writer.WriteEndElement()

        if addEnvelope then writer.WriteEndElement()

        writer.Flush()

        stream.Position <- 0L
        use reader = new StreamReader(stream)
        let xml = reader.ReadToEnd()

        f message (if addEnvelope then
                       let start = xml.IndexOf('>') + 1
                       xml.Substring(start, xml.LastIndexOf('<') - start)
                   else xml)

    let shouldSerializeMultipartTo attachmentCount expected v =
        serializeWithContext "keha" v 2u false true (fun msg xml ->
            xml |> should equal expected
            msg.AllAttachments.Count |> should equal attachmentCount)

    let shouldSerializeTo expected v =
        serializeWithContext "keha" v 2u false false (fun msg xml ->
            xml |> should equal expected
            msg.AllAttachments.Count |> should equal 0)

    let shouldSerializeToV1 expected v =
        serializeWithContext "keha" v 1u false false (fun msg xml ->
            xml |> should equal expected
            msg.AllAttachments.Count |> should equal 0)

    [<Test>]
    let ``can serialize array content in envelope`` () =
        let data = [| 5; 4; 3 |]
        serializeWithContext "keha" data 1u true false (fun msg xml ->
            xml |> should equal """<keha><item xsi:type="xsd:int">5</item><item xsi:type="xsd:int">4</item><item xsi:type="xsd:int">3</item></keha>"""
            msg.AllAttachments.Count |> should equal 0)

    [<Test>]
    let ``can serialize array content`` () =
        [| 5; 4; 3 |]
        |> shouldSerializeTo """<keha><item d2p1:type="d2p2:int" xmlns:d2p2="http://www.w3.org/2001/XMLSchema" xmlns:d2p1="http://www.w3.org/2001/XMLSchema-instance">5</item><item d2p1:type="d2p2:int" xmlns:d2p2="http://www.w3.org/2001/XMLSchema" xmlns:d2p1="http://www.w3.org/2001/XMLSchema-instance">4</item><item d2p1:type="d2p2:int" xmlns:d2p2="http://www.w3.org/2001/XMLSchema" xmlns:d2p1="http://www.w3.org/2001/XMLSchema-instance">3</item></keha>"""

    [<Test>]
    let ``can serialize boolean 'false' value`` () =
        false
        |> shouldSerializeTo """<keha>false</keha>"""

    [<Test>]
    let ``can serialize boolean 'true' value`` () =
        true
        |> shouldSerializeTo """<keha>true</keha>"""

    [<Test>]
    let ``can serialize dateTime value`` () =
        DateTime(2000, 10, 12, 4, 14, 55, 989)
        |> shouldSerializeTo """<keha>2000-10-12T04:14:55.989</keha>"""

    [<Test>]
    let ``can serialize decimal value`` () =
        0.4M
        |> shouldSerializeTo """<keha>0.4</keha>"""

    [<Test>]
    let ``can serialize float value`` () =
        0.1f
        |> shouldSerializeTo """<keha>0.1</keha>"""

    [<Test>]
    let ``can serialize int value`` () =
        44345
        |> shouldSerializeTo """<keha>44345</keha>"""

    [<Test>]
    let ``can serialize short value`` () =
        445s
        |> shouldSerializeTo """<keha>445</keha>"""

    [<Test>]
    let ``can serialize long value`` () =
        44345L
        |> shouldSerializeTo """<keha>44345</keha>"""

    [<Test>]
    let ``can serialize long array value`` () =
        [| 5L; 4L; 3L; |]
        |> shouldSerializeTo """<keha><item d2p1:type="d2p2:long" xmlns:d2p2="http://www.w3.org/2001/XMLSchema" xmlns:d2p1="http://www.w3.org/2001/XMLSchema-instance">5</item><item d2p1:type="d2p2:long" xmlns:d2p2="http://www.w3.org/2001/XMLSchema" xmlns:d2p1="http://www.w3.org/2001/XMLSchema-instance">4</item><item d2p1:type="d2p2:long" xmlns:d2p2="http://www.w3.org/2001/XMLSchema" xmlns:d2p1="http://www.w3.org/2001/XMLSchema-instance">3</item></keha>"""

    [<Test>]
    let ``can serialize short dateTime value`` () =
        DateTime(2000, 10, 12)
        |> shouldSerializeTo """<keha>2000-10-12T00:00:00</keha>"""

    [<Test>]
    let ``can serialize string value`` () =
        "someString"
        |> shouldSerializeTo """<keha>someString</keha>"""

    [<Test>]
    let ``can serialize string value with special characters`` () =
        "&<>"
        |> shouldSerializeTo """<keha><![CDATA[&<>]]></keha>"""

    [<Test>]
    let ``can serialize struct value`` () =
        TestDto(Nimi = "Mauno", Kood = "1235", Loodud = DateTime(2000, 12, 12, 12, 12, 12))
        |> shouldSerializeTo """<keha><Nimi d2p1:type="d2p2:string" xmlns:d2p2="http://www.w3.org/2001/XMLSchema" xmlns:d2p1="http://www.w3.org/2001/XMLSchema-instance">Mauno</Nimi><Kood d2p1:type="d2p2:string" xmlns:d2p2="http://www.w3.org/2001/XMLSchema" xmlns:d2p1="http://www.w3.org/2001/XMLSchema-instance">1235</Kood><Loodud d2p1:type="d2p2:dateTime" xmlns:d2p2="http://www.w3.org/2001/XMLSchema" xmlns:d2p1="http://www.w3.org/2001/XMLSchema-instance">2000-12-12T12:12:12</Loodud></keha>"""

    [<Test>]
    let ``can serialize binary value`` () =
        use stream = new MemoryStream()
        XRoadBinaryTestDto(Sisu = stream)
        |> shouldSerializeMultipartTo 1 """<keha><Sisu d2p1:type="d2p2:base64Binary" href="cid:1B2M2Y8AsgTpgAmY7PhCfg==" xmlns:d2p2="http://schemas.xmlsoap.org/soap/encoding/" xmlns:d2p1="http://www.w3.org/2001/XMLSchema-instance" /></keha>"""

    [<Test>]
    let ``can serialize hex binary value`` () =
        use stream = new MemoryStream()
        XRoadHexTestDto(Sisu = stream)
        |> shouldSerializeMultipartTo 1 """<keha><Sisu d2p1:type="d2p2:hexBinary" href="cid:1B2M2Y8AsgTpgAmY7PhCfg==" xmlns:d2p2="http://schemas.xmlsoap.org/soap/encoding/" xmlns:d2p1="http://www.w3.org/2001/XMLSchema-instance" /></keha>"""

    [<Test>]
    let ``can serialize date type with custom name`` () =
        DateTestDto(Synniaeg = Nullable(DateTime(2012, 11, 26, 16, 29, 13)))
        |> shouldSerializeTo """<keha><ttIsik.dSyn d2p1:type="d2p2:date" xmlns:d2p2="http://www.w3.org/2001/XMLSchema" xmlns:d2p1="http://www.w3.org/2001/XMLSchema-instance">2012-11-26</ttIsik.dSyn></keha>"""

    [<Test>]
    let ``serialize default DTO version`` () =
        WsdlChangesTestDto(AddedProperty = Nullable(1L),
                           ChangedTypeProperty = Nullable(2L),
                           RemovedProperty = Nullable(3L),
                           RenamedToProperty = Nullable(4L),
                           RenamedFromProperty = Nullable(5L),
                           StaticProperty = Nullable(6L),
                           SingleProperty = Nullable(7L),
                           MultipleProperty = [| 8L; 9L |])
        |> shouldSerializeTo ([ @"<keha>"
                                @"<AddedProperty d2p1:type=""d2p2:long"" xmlns:d2p2=""http://www.w3.org/2001/XMLSchema"" xmlns:d2p1=""http://www.w3.org/2001/XMLSchema-instance"">1</AddedProperty>"
                                @"<ChangedTypeProperty d2p1:type=""d2p2:long"" xmlns:d2p2=""http://www.w3.org/2001/XMLSchema"" xmlns:d2p1=""http://www.w3.org/2001/XMLSchema-instance"">2</ChangedTypeProperty>"
                                @"<MultipleProperty d2p1:type=""d2p2:Array"" d2p2:arrayType=""d2p3:long[2]"" xmlns:d2p3=""http://www.w3.org/2001/XMLSchema"" xmlns:d2p2=""http://schemas.xmlsoap.org/soap/encoding/"" xmlns:d2p1=""http://www.w3.org/2001/XMLSchema-instance"">"
                                @"<item d2p1:type=""d2p3:long"">8</item>"
                                @"<item d2p1:type=""d2p3:long"">9</item>"
                                @"</MultipleProperty>"
                                @"<RenamedToProperty d2p1:type=""d2p2:long"" xmlns:d2p2=""http://www.w3.org/2001/XMLSchema"" xmlns:d2p1=""http://www.w3.org/2001/XMLSchema-instance"">4</RenamedToProperty>"
                                @"<StaticProperty d2p1:type=""d2p2:long"" xmlns:d2p2=""http://www.w3.org/2001/XMLSchema"" xmlns:d2p1=""http://www.w3.org/2001/XMLSchema-instance"">6</StaticProperty>"
                                @"</keha>" ] |> String.concat "")

    [<Test>]
    let ``serialize v1 DTO version`` () =
        WsdlChangesTestDto(AddedProperty = Nullable(1L),
                           ChangedTypeProperty = Nullable(2L),
                           RemovedProperty = Nullable(3L),
                           RenamedToProperty = Nullable(4L),
                           RenamedFromProperty = Nullable(5L),
                           StaticProperty = Nullable(6L),
                           SingleProperty = Nullable(7L),
                           MultipleProperty = [| 8L; 9L |])
        |> shouldSerializeToV1 ([ @"<keha>"
                                  @"<ChangedTypeProperty d2p1:type=""d2p2:string"" xmlns:d2p2=""http://www.w3.org/2001/XMLSchema"" xmlns:d2p1=""http://www.w3.org/2001/XMLSchema-instance"">2</ChangedTypeProperty>"
                                  @"<RemovedProperty d2p1:type=""d2p2:long"" xmlns:d2p2=""http://www.w3.org/2001/XMLSchema"" xmlns:d2p1=""http://www.w3.org/2001/XMLSchema-instance"">3</RemovedProperty>"
                                  @"<RenamedFromProperty d2p1:type=""d2p2:long"" xmlns:d2p2=""http://www.w3.org/2001/XMLSchema"" xmlns:d2p1=""http://www.w3.org/2001/XMLSchema-instance"">4</RenamedFromProperty>"
                                  @"<SingleProperty d2p1:type=""d2p2:long"" xmlns:d2p2=""http://www.w3.org/2001/XMLSchema"" xmlns:d2p1=""http://www.w3.org/2001/XMLSchema-instance"">8</SingleProperty>"
                                  @"<StaticProperty d2p1:type=""d2p2:long"" xmlns:d2p2=""http://www.w3.org/2001/XMLSchema"" xmlns:d2p1=""http://www.w3.org/2001/XMLSchema-instance"">6</StaticProperty>"
                                  @"</keha>" ] |> String.concat "")

    let [<Test>] ``can serialize anonymous type`` () =
        let entity = Wsdl.ContainerType()
        entity.KnownProperty <- "value"
        entity.AnonymousProperty <- Wsdl.AnonymousType(Property1 = "1", Property2 = "2", Property3 = "3")
        entity |> shouldSerializeTo ([ @"<keha>"
                                       @"<AnonymousProperty>"
                                       @"<Property1 d3p1:type=""d3p2:string"" xmlns:d3p2=""http://www.w3.org/2001/XMLSchema"" xmlns:d3p1=""http://www.w3.org/2001/XMLSchema-instance"">1</Property1>"
                                       @"<Property2 d3p1:type=""d3p2:string"" xmlns:d3p2=""http://www.w3.org/2001/XMLSchema"" xmlns:d3p1=""http://www.w3.org/2001/XMLSchema-instance"">2</Property2>"
                                       @"<Property3 d3p1:type=""d3p2:string"" xmlns:d3p2=""http://www.w3.org/2001/XMLSchema"" xmlns:d3p1=""http://www.w3.org/2001/XMLSchema-instance"">3</Property3>"
                                       @"</AnonymousProperty>"
                                       @"<KnownProperty d2p1:type=""d2p2:string"" xmlns:d2p2=""http://www.w3.org/2001/XMLSchema"" xmlns:d2p1=""http://www.w3.org/2001/XMLSchema-instance"">value</KnownProperty>"
                                       @"</keha>" ] |> String.concat "")

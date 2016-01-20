namespace XRoadLib.Tests

open FsUnit
open NUnit.Framework
open System
open System.IO
open System.Reflection
open System.Text
open System.Xml
open XRoadLib
open XRoadLib.Extensions
open XRoadLib.Serialization
open XRoadLib.Serialization.Mapping
open XRoadLib.Serialization.Template
open XRoadLib.Tests.Contract

[<TestFixture>]
module XRoadSerializerTest =
    let serializerCache = SerializerCache(typeof<Class1>.Assembly, XRoadProtocol.Version20)

    let serializeWithContext<'T> elementName (value: 'T) dtoVersion addEnvelope isMultipart f =
        use message = new XRoadMessage(XRoadProtocol.Version20, IsMultipart = isMultipart)

        use stream = new MemoryStream()
        use writer = new XmlTextWriter(stream, Encoding.UTF8)

        if addEnvelope then
            writer.WriteStartElement("Envelope")
            writer.WriteAttributeString("xmlns", PrefixConstants.SOAP_ENC, NamespaceConstants.XMLNS, NamespaceConstants.SOAP_ENC)
            writer.WriteAttributeString("xmlns", PrefixConstants.XSI, NamespaceConstants.XMLNS, NamespaceConstants.XSI)
            writer.WriteAttributeString("xmlns", PrefixConstants.XSD, NamespaceConstants.XMLNS, NamespaceConstants.XSD)
            writer.WriteAttributeString("xmlns", "tns", NamespaceConstants.XMLNS, XRoadProtocol.Version20.GetProducerNamespace("test-producer"))

        writer.WriteStartElement(elementName)

        let context = SerializationContext(message, dtoVersion)
        let typeMap = serializerCache.GetTypeMap(typeof<'T>, context.DtoVersion)
        typeMap.Serialize(writer, XRoadXmlTemplate.EmptyNode, value, typeof<'T>, context)

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
            xml |> should equal """<keha xsi:type="SOAP-ENC:Array" SOAP-ENC:arrayType="xsd:int[3]"><item xsi:type="xsd:int">5</item><item xsi:type="xsd:int">4</item><item xsi:type="xsd:int">3</item></keha>"""
            msg.AllAttachments.Count |> should equal 0)

    [<Test>]
    let ``can serialize array content`` () =
        [| 5; 4; 3 |]
        |> shouldSerializeTo """<keha d1p1:type="d1p2:Array" d1p2:arrayType="d1p3:int[3]" xmlns:d1p3="http://www.w3.org/2001/XMLSchema" xmlns:d1p2="http://schemas.xmlsoap.org/soap/encoding/" xmlns:d1p1="http://www.w3.org/2001/XMLSchema-instance"><item d1p1:type="d1p3:int">5</item><item d1p1:type="d1p3:int">4</item><item d1p1:type="d1p3:int">3</item></keha>"""

    [<Test>]
    let ``can serialize boolean 'false' value`` () =
        false
        |> shouldSerializeTo """<keha d1p1:type="d1p2:boolean" xmlns:d1p2="http://www.w3.org/2001/XMLSchema" xmlns:d1p1="http://www.w3.org/2001/XMLSchema-instance">false</keha>"""

    [<Test>]
    let ``can serialize boolean 'true' value`` () =
        true
        |> shouldSerializeTo """<keha d1p1:type="d1p2:boolean" xmlns:d1p2="http://www.w3.org/2001/XMLSchema" xmlns:d1p1="http://www.w3.org/2001/XMLSchema-instance">true</keha>"""

    [<Test>]
    let ``can serialize dateTime value`` () =
        DateTime(2000, 10, 12, 4, 14, 55, 989)
        |> shouldSerializeTo """<keha d1p1:type="d1p2:dateTime" xmlns:d1p2="http://www.w3.org/2001/XMLSchema" xmlns:d1p1="http://www.w3.org/2001/XMLSchema-instance">2000-10-12T04:14:55.989</keha>"""

    [<Test>]
    let ``can serialize decimal value`` () =
        0.4M
        |> shouldSerializeTo """<keha d1p1:type="d1p2:decimal" xmlns:d1p2="http://www.w3.org/2001/XMLSchema" xmlns:d1p1="http://www.w3.org/2001/XMLSchema-instance">0.4</keha>"""

    [<Test>]
    let ``can serialize float value`` () =
        0.1f
        |> shouldSerializeTo """<keha d1p1:type="d1p2:float" xmlns:d1p2="http://www.w3.org/2001/XMLSchema" xmlns:d1p1="http://www.w3.org/2001/XMLSchema-instance">0.1</keha>"""

    [<Test>]
    let ``can serialize int value`` () =
        44345
        |> shouldSerializeTo """<keha d1p1:type="d1p2:int" xmlns:d1p2="http://www.w3.org/2001/XMLSchema" xmlns:d1p1="http://www.w3.org/2001/XMLSchema-instance">44345</keha>"""

    [<Test>]
    let ``can serialize short value`` () =
        445s
        |> shouldSerializeTo """<keha d1p1:type="d1p2:int" xmlns:d1p2="http://www.w3.org/2001/XMLSchema" xmlns:d1p1="http://www.w3.org/2001/XMLSchema-instance">445</keha>"""

    [<Test>]
    let ``can serialize long value`` () =
        44345L
        |> shouldSerializeTo """<keha d1p1:type="d1p2:long" xmlns:d1p2="http://www.w3.org/2001/XMLSchema" xmlns:d1p1="http://www.w3.org/2001/XMLSchema-instance">44345</keha>"""

    [<Test>]
    let ``can serialize long array value`` () =
        [| 5L; 4L; 3L; |]
        |> shouldSerializeTo """<keha d1p1:type="d1p2:Array" d1p2:arrayType="d1p3:long[3]" xmlns:d1p3="http://www.w3.org/2001/XMLSchema" xmlns:d1p2="http://schemas.xmlsoap.org/soap/encoding/" xmlns:d1p1="http://www.w3.org/2001/XMLSchema-instance"><item d1p1:type="d1p3:long">5</item><item d1p1:type="d1p3:long">4</item><item d1p1:type="d1p3:long">3</item></keha>"""

    [<Test>]
    let ``can serialize short dateTime value`` () =
        DateTime(2000, 10, 12)
        |> shouldSerializeTo """<keha d1p1:type="d1p2:dateTime" xmlns:d1p2="http://www.w3.org/2001/XMLSchema" xmlns:d1p1="http://www.w3.org/2001/XMLSchema-instance">2000-10-12T00:00:00</keha>"""

    [<Test>]
    let ``can serialize string value`` () =
        "someString"
        |> shouldSerializeTo """<keha d1p1:type="d1p2:string" xmlns:d1p2="http://www.w3.org/2001/XMLSchema" xmlns:d1p1="http://www.w3.org/2001/XMLSchema-instance">someString</keha>"""

    [<Test>]
    let ``can serialize string value with special characters`` () =
        "&<>"
        |> shouldSerializeTo """<keha d1p1:type="d1p2:string" xmlns:d1p2="http://www.w3.org/2001/XMLSchema" xmlns:d1p1="http://www.w3.org/2001/XMLSchema-instance"><![CDATA[&<>]]></keha>"""

    [<Test>]
    let ``can serialize struct value`` () =
        TestDto(Nimi = "Mauno", Kood = "1235", Loodud = DateTime(2000, 12, 12, 12, 12, 12))
        |> shouldSerializeTo """<keha d1p1:type="d1p2:TestDto" xmlns:d1p2="http://producers.test-producer.xtee.riik.ee/producer/test-producer" xmlns:d1p1="http://www.w3.org/2001/XMLSchema-instance"><Nimi d1p1:type="d2p1:string" xmlns:d2p1="http://www.w3.org/2001/XMLSchema">Mauno</Nimi><Kood d1p1:type="d2p1:string" xmlns:d2p1="http://www.w3.org/2001/XMLSchema">1235</Kood><Loodud d1p1:type="d2p1:dateTime" xmlns:d2p1="http://www.w3.org/2001/XMLSchema">2000-12-12T12:12:12</Loodud></keha>"""

    [<Test>]
    let ``can serialize binary value`` () =
        use stream = new MemoryStream()
        XRoadBinaryTestDto(Sisu = stream)
        |> shouldSerializeMultipartTo 1 """<keha d1p1:type="d1p2:XRoadBinaryTestDto" xmlns:d1p2="http://producers.test-producer.xtee.riik.ee/producer/test-producer" xmlns:d1p1="http://www.w3.org/2001/XMLSchema-instance"><Sisu d1p1:type="d2p1:base64Binary" href="cid:1B2M2Y8AsgTpgAmY7PhCfg==" xmlns:d2p1="http://schemas.xmlsoap.org/soap/encoding/" /></keha>"""

    [<Test>]
    let ``can serialize hex binary value`` () =
        use stream = new MemoryStream()
        XRoadHexTestDto(Sisu = stream)
        |> shouldSerializeMultipartTo 1 """<keha d1p1:type="d1p2:XRoadHexTestDto" xmlns:d1p2="http://producers.test-producer.xtee.riik.ee/producer/test-producer" xmlns:d1p1="http://www.w3.org/2001/XMLSchema-instance"><Sisu d1p1:type="d2p1:hexBinary" href="cid:1B2M2Y8AsgTpgAmY7PhCfg==" xmlns:d2p1="http://schemas.xmlsoap.org/soap/encoding/" /></keha>"""

    [<Test>]
    let ``can serialize date type with custom name`` () =
        DateTestDto(Synniaeg = Nullable(DateTime(2012, 11, 26, 16, 29, 13)))
        |> shouldSerializeTo """<keha d1p1:type="d1p2:DateTestDto" xmlns:d1p2="http://producers.test-producer.xtee.riik.ee/producer/test-producer" xmlns:d1p1="http://www.w3.org/2001/XMLSchema-instance"><ttIsik.dSyn d1p1:type="d2p1:date" xmlns:d2p1="http://www.w3.org/2001/XMLSchema">2012-11-26</ttIsik.dSyn></keha>"""

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
        |> shouldSerializeTo ([ @"<keha d1p1:type=""d1p2:WsdlChangesTestDto"" xmlns:d1p2=""http://producers.test-producer.xtee.riik.ee/producer/test-producer"" xmlns:d1p1=""http://www.w3.org/2001/XMLSchema-instance"">"
                                @"<AddedProperty d1p1:type=""d2p1:long"" xmlns:d2p1=""http://www.w3.org/2001/XMLSchema"">1</AddedProperty>"
                                @"<ChangedTypeProperty d1p1:type=""d2p1:long"" xmlns:d2p1=""http://www.w3.org/2001/XMLSchema"">2</ChangedTypeProperty>"
                                @"<MultipleProperty d1p1:type=""d2p1:Array"" d2p1:arrayType=""d2p2:long[2]"" xmlns:d2p2=""http://www.w3.org/2001/XMLSchema"" xmlns:d2p1=""http://schemas.xmlsoap.org/soap/encoding/"">"
                                @"<item d1p1:type=""d2p2:long"">8</item>"
                                @"<item d1p1:type=""d2p2:long"">9</item>"
                                @"</MultipleProperty>"
                                @"<RenamedToProperty d1p1:type=""d2p1:long"" xmlns:d2p1=""http://www.w3.org/2001/XMLSchema"">4</RenamedToProperty>"
                                @"<StaticProperty d1p1:type=""d2p1:long"" xmlns:d2p1=""http://www.w3.org/2001/XMLSchema"">6</StaticProperty>"
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
        |> shouldSerializeToV1 ([ @"<keha d1p1:type=""d1p2:WsdlChangesTestDto"" xmlns:d1p2=""http://producers.test-producer.xtee.riik.ee/producer/test-producer"" xmlns:d1p1=""http://www.w3.org/2001/XMLSchema-instance"">"
                                  @"<ChangedTypeProperty d1p1:type=""d2p1:string"" xmlns:d2p1=""http://www.w3.org/2001/XMLSchema"">2</ChangedTypeProperty>"
                                  @"<RemovedProperty d1p1:type=""d2p1:long"" xmlns:d2p1=""http://www.w3.org/2001/XMLSchema"">3</RemovedProperty>"
                                  @"<RenamedFromProperty d1p1:type=""d2p1:long"" xmlns:d2p1=""http://www.w3.org/2001/XMLSchema"">4</RenamedFromProperty>"
                                  @"<SingleProperty d1p1:type=""d2p1:long"" xmlns:d2p1=""http://www.w3.org/2001/XMLSchema"">8</SingleProperty>"
                                  @"<StaticProperty d1p1:type=""d2p1:long"" xmlns:d2p1=""http://www.w3.org/2001/XMLSchema"">6</StaticProperty>"
                                  @"</keha>" ] |> String.concat "")

    let [<Test>] ``can serialize anonymous type`` () =
        let entity = Wsdl.ContainerType()
        entity.KnownProperty <- "value"
        entity.AnonymousProperty <- Wsdl.AnonymousType(Property1 = "1", Property2 = "2", Property3 = "3")
        entity |> shouldSerializeTo ([ @"<keha d1p1:type=""d1p2:ContainerType"" xmlns:d1p2=""http://producers.test-producer.xtee.riik.ee/producer/test-producer"" xmlns:d1p1=""http://www.w3.org/2001/XMLSchema-instance"">"
                                       @"<AnonymousProperty>"
                                       @"<Property1 d1p1:type=""d3p1:string"" xmlns:d3p1=""http://www.w3.org/2001/XMLSchema"">1</Property1>"
                                       @"<Property2 d1p1:type=""d3p1:string"" xmlns:d3p1=""http://www.w3.org/2001/XMLSchema"">2</Property2>"
                                       @"<Property3 d1p1:type=""d3p1:string"" xmlns:d3p1=""http://www.w3.org/2001/XMLSchema"">3</Property3>"
                                       @"</AnonymousProperty>"
                                       @"<KnownProperty d1p1:type=""d2p1:string"" xmlns:d2p1=""http://www.w3.org/2001/XMLSchema"">value</KnownProperty>"
                                       @"</keha>" ] |> String.concat "")

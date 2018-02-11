using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using XRoadLib.Extensions;
using XRoadLib.Schema;
using XRoadLib.Serialization;
using XRoadLib.Serialization.Template;
using XRoadLib.Tests.Contract;
using XRoadLib.Tests.Contract.Wsdl;
using Xunit;

namespace XRoadLib.Tests.Serialization
{
    public class XRoadSerializerTest
    {
        private class X<T>
        {
            // ReSharper disable once UnusedMember.Local, UnusedParameter.Local
            public void Method(T t)
            { }
        }

        private static void SerializeWithContext<T>(string elementName, T value, uint dtoVersion, bool addEnvelope, bool isMultipart, Action<XRoadMessage, string> f)
        {
            var message = Globals.XRoadProtocol20.CreateMessage();
            message.IsMultipartContainer = true;
            message.BinaryMode = BinaryMode.Attachment;

            using (message)
            using (var stream = new MemoryStream())
            using (var tw = new StreamWriter(stream, Encoding.UTF8))
            using (var writer = XmlWriter.Create(tw))
            {
                if (addEnvelope)
                {
                    writer.WriteStartElement("Envelope");
                    writer.WriteAttributeString("xmlns", PrefixConstants.SOAP_ENC, NamespaceConstants.XMLNS, NamespaceConstants.SOAP_ENC);
                    writer.WriteAttributeString("xmlns", PrefixConstants.XSI, NamespaceConstants.XMLNS, NamespaceConstants.XSI);
                    writer.WriteAttributeString("xmlns", PrefixConstants.XSD, NamespaceConstants.XMLNS, NamespaceConstants.XSD);
                    writer.WriteAttributeString("xmlns", "tns", NamespaceConstants.XMLNS, Globals.XRoadProtocol20.ProducerNamespace);
                }

                writer.WriteStartElement(elementName);

                var propType = typeof(X<>).MakeGenericType(typeof(T));
                var methodInfo = propType.GetTypeInfo().GetMethod("Method");

                var operationDefinition = new OperationDefinition("Method", null, methodInfo);
                var requestDefinition = new RequestDefinition(operationDefinition);

                var typeMap = Globals.XRoadProtocol20.GetSerializerCache(dtoVersion).GetTypeMap(typeof(T));
                typeMap.Serialize(writer, XRoadXmlTemplate.EmptyNode, value, requestDefinition.Content, message);

                writer.WriteEndElement();

                if (addEnvelope)
                    writer.WriteEndElement();

                writer.Flush();

                stream.Position = 0;
                using (var reader = new StreamReader(stream))
                {
                    if (!addEnvelope)
                    {
                        f(message, reader.ReadToEnd());
                        return;
                    }

                    using (var xmlReader = XmlReader.Create(reader))
                    {
                        xmlReader.MoveToElement(0, "Envelope");
                        f(message, xmlReader.ReadInnerXml());
                    }
                }
            }
        }

        [Fact]
        public void CanSerializeArrayContentInEnvelope()
        {
            var value = new[] { 5, 4, 3 };

            var expected = "<keha>"
                         + "<item xsi:type=\"xsd:int\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">5</item>"
                         + "<item xsi:type=\"xsd:int\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">4</item>"
                         + "<item xsi:type=\"xsd:int\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">3</item>"
                         + "</keha>";

            SerializeWithContext("keha", value, 1u, true, false, (msg, xml) =>
            {
                Assert.Equal(expected, xml);
                Assert.Equal(0, msg.AllAttachments.Count);
            });
        }

        [Fact]
        public void CanSerializeArrayContent()
        {
            var value = new[] { 5, 4, 3 };

            var expected = "<?xml version=\"1.0\" encoding=\"utf-8\"?>"
                         + "<keha>"
                         + "<item p2:type=\"p3:int\" xmlns:p3=\"http://www.w3.org/2001/XMLSchema\" xmlns:p2=\"http://www.w3.org/2001/XMLSchema-instance\">5</item>"
                         + "<item p2:type=\"p3:int\" xmlns:p3=\"http://www.w3.org/2001/XMLSchema\" xmlns:p2=\"http://www.w3.org/2001/XMLSchema-instance\">4</item>"
                         + "<item p2:type=\"p3:int\" xmlns:p3=\"http://www.w3.org/2001/XMLSchema\" xmlns:p2=\"http://www.w3.org/2001/XMLSchema-instance\">3</item>"
                         + "</keha>";

            SerializeWithContext("keha", value, 2u, false, false, (msg, xml) =>
            {
                Assert.Equal(expected, xml);
                Assert.Equal(0, msg.AllAttachments.Count);
            });
        }

        [Fact]
        public void CanSerializeBooleanFalseValue()
        {
            SerializeWithContext("keha", false, 2u, false, false, (msg, xml) =>
            {
                Assert.Equal("<?xml version=\"1.0\" encoding=\"utf-8\"?><keha>false</keha>", xml);
                Assert.Equal(0, msg.AllAttachments.Count);
            });
        }

        [Fact]
        public void CanSerializeBooleanTrueValue()
        {
            SerializeWithContext("keha", true, 2u, false, false, (msg, xml) =>
            {
                Assert.Equal("<?xml version=\"1.0\" encoding=\"utf-8\"?><keha>true</keha>", xml);
                Assert.Equal(0, msg.AllAttachments.Count);
            });
        }

        [Fact]
        public void CanSerializeDateTimeValue()
        {
            SerializeWithContext("keha", new DateTime(2000, 10, 12, 4, 14, 55, 989), 2u, false, false, (msg, xml) =>
            {
                Assert.Equal("<?xml version=\"1.0\" encoding=\"utf-8\"?><keha>2000-10-12T04:14:55.989</keha>", xml);
                Assert.Equal(0, msg.AllAttachments.Count);
            });
        }

        [Fact]
        public void CanSerializeDecimalValue()
        {
            SerializeWithContext("keha", 0.4M, 2u, false, false, (msg, xml) =>
            {
                Assert.Equal("<?xml version=\"1.0\" encoding=\"utf-8\"?><keha>0.4</keha>", xml);
                Assert.Equal(0, msg.AllAttachments.Count);
            });
        }

        [Fact]
        public void CanSerializeFloatValue()
        {
            SerializeWithContext("keha", 0.1f, 2u, false, false, (msg, xml) =>
            {
                Assert.Equal("<?xml version=\"1.0\" encoding=\"utf-8\"?><keha>0.1</keha>", xml);
                Assert.Equal(0, msg.AllAttachments.Count);
            });
        }

        [Fact]
        public void CanSerializeIntValue()
        {
            SerializeWithContext("keha", 44345, 2u, false, false, (msg, xml) =>
            {
                Assert.Equal("<?xml version=\"1.0\" encoding=\"utf-8\"?><keha>44345</keha>", xml);
                Assert.Equal(0, msg.AllAttachments.Count);
            });
        }

        [Fact]
        public void CanSerializeShortValue()
        {
            SerializeWithContext("keha", (short)445, 2u, false, false, (msg, xml) =>
            {
                Assert.Equal("<?xml version=\"1.0\" encoding=\"utf-8\"?><keha>445</keha>", xml);
                Assert.Equal(0, msg.AllAttachments.Count);
            });
        }

        [Fact]
        public void CanSerializeLongValue()
        {
            SerializeWithContext("keha", 44345L, 2u, false, false, (msg, xml) =>
            {
                Assert.Equal("<?xml version=\"1.0\" encoding=\"utf-8\"?><keha>44345</keha>", xml);
                Assert.Equal(0, msg.AllAttachments.Count);
            });
        }

        [Fact]
        public void CanSerializeLongArrayValue()
        {
            var value = new[] { 5L, 4L, 3L };

            var expected = "<?xml version=\"1.0\" encoding=\"utf-8\"?>"
                         + "<keha>"
                         + "<item p2:type=\"p3:long\" xmlns:p3=\"http://www.w3.org/2001/XMLSchema\" xmlns:p2=\"http://www.w3.org/2001/XMLSchema-instance\">5</item>"
                         + "<item p2:type=\"p3:long\" xmlns:p3=\"http://www.w3.org/2001/XMLSchema\" xmlns:p2=\"http://www.w3.org/2001/XMLSchema-instance\">4</item>"
                         + "<item p2:type=\"p3:long\" xmlns:p3=\"http://www.w3.org/2001/XMLSchema\" xmlns:p2=\"http://www.w3.org/2001/XMLSchema-instance\">3</item>"
                         + "</keha>";

            SerializeWithContext("keha", value, 2u, false, false, (msg, xml) =>
            {
                Assert.Equal(expected, xml);
                Assert.Equal(0, msg.AllAttachments.Count);
            });
        }

        [Fact]
        public void CanSerializeShortDateTimeValue()
        {
            SerializeWithContext("keha", new DateTime(2000, 10, 12), 2u, false, false, (msg, xml) =>
            {
                Assert.Equal("<?xml version=\"1.0\" encoding=\"utf-8\"?><keha>2000-10-12T00:00:00</keha>", xml);
                Assert.Equal(0, msg.AllAttachments.Count);
            });
        }

        [Fact]
        public void CanSerializeStringValue()
        {
            SerializeWithContext("keha", "someString", 2u, false, false, (msg, xml) =>
            {
                Assert.Equal("<?xml version=\"1.0\" encoding=\"utf-8\"?><keha>someString</keha>", xml);
                Assert.Equal(0, msg.AllAttachments.Count);
            });
        }

        [Fact]
        public void CanSerializeStructValue()
        {
            var value = new TestDto
            {
                Nimi = "Mauno",
                Kood = "1235",
                Loodud = new DateTime(2000, 12, 12, 12, 12, 12)
            };

            var expected = "<?xml version=\"1.0\" encoding=\"utf-8\"?>"
                         + "<keha>"
                         + "<Nimi p2:type=\"p3:string\" xmlns:p3=\"http://www.w3.org/2001/XMLSchema\" xmlns:p2=\"http://www.w3.org/2001/XMLSchema-instance\">Mauno</Nimi>"
                         + "<Kood p2:type=\"p3:string\" xmlns:p3=\"http://www.w3.org/2001/XMLSchema\" xmlns:p2=\"http://www.w3.org/2001/XMLSchema-instance\">1235</Kood>"
                         + "<Loodud p2:type=\"p3:dateTime\" xmlns:p3=\"http://www.w3.org/2001/XMLSchema\" xmlns:p2=\"http://www.w3.org/2001/XMLSchema-instance\">2000-12-12T12:12:12</Loodud>"
                         + "</keha>";

            SerializeWithContext("keha", value, 2u, false, false, (msg, xml) =>
            {
                Assert.Equal(expected, xml);
                Assert.Equal(0, msg.AllAttachments.Count);
            });
        }

        [Fact]
        public void CanSerializeBinaryValue()
        {
            using (var stream = new MemoryStream())
            {
                var value = new XRoadBinaryTestDto { Sisu = stream };

                var expected = "<?xml version=\"1.0\" encoding=\"utf-8\"?>"
                             + "<keha>"
                             + "<Sisu p2:type=\"p3:base64Binary\" href=\"cid:1B2M2Y8AsgTpgAmY7PhCfg==\" xmlns:p3=\"http://schemas.xmlsoap.org/soap/encoding/\" xmlns:p2=\"http://www.w3.org/2001/XMLSchema-instance\" />"
                             + "</keha>";

                SerializeWithContext("keha", value, 2u, false, true, (msg, xml) =>
                {
                    Assert.Equal(expected, xml);
                    Assert.Equal(1, msg.AllAttachments.Count);
                });
            }
        }

        [Fact]
        public void CanSerializeHexBinaryValue()
        {
            using (var stream = new MemoryStream())
            {
                var value = new XRoadHexTestDto { Sisu = stream };

                var expected = "<?xml version=\"1.0\" encoding=\"utf-8\"?>"
                             + "<keha>"
                             + "<Sisu p2:type=\"p3:hexBinary\" href=\"cid:1B2M2Y8AsgTpgAmY7PhCfg==\" xmlns:p3=\"http://schemas.xmlsoap.org/soap/encoding/\" xmlns:p2=\"http://www.w3.org/2001/XMLSchema-instance\" />"
                             + "</keha>";

                SerializeWithContext("keha", value, 2u, false, true, (msg, xml) =>
                {
                    Assert.Equal(expected, xml);
                    Assert.Equal(1, msg.AllAttachments.Count);
                });
            }
        }

        [Fact]
        public void CanSerializeDateTypeWithCustomName()
        {
            var value = new DateTestDto { Synniaeg = new DateTime(2012, 11, 26, 16, 29, 13) };

            var expected = "<?xml version=\"1.0\" encoding=\"utf-8\"?>"
                         + "<keha>"
                         + "<ttIsik.dSyn p2:type=\"p3:date\" xmlns:p3=\"http://www.w3.org/2001/XMLSchema\" xmlns:p2=\"http://www.w3.org/2001/XMLSchema-instance\">2012-11-26</ttIsik.dSyn>"
                         + "</keha>";

            SerializeWithContext("keha", value, 2u, false, false, (msg, xml) =>
            {
                Assert.Equal(expected, xml);
                Assert.Equal(0, msg.AllAttachments.Count);
            });
        }

        [Fact]
        public void SerializeDefaultDtoVersion()
        {
            var value = new WsdlChangesTestDto
            {
                AddedProperty = 1L,
                ChangedTypeProperty = 2L,
                RemovedProperty = 3L,
                RenamedToProperty = 4L,
                RenamedFromProperty = 5L,
                StaticProperty = 6L,
                SingleProperty = 7L,
                MultipleProperty = new [] { 8L, 9L }
            };

            var expected = "<?xml version=\"1.0\" encoding=\"utf-8\"?>"
                         + "<keha>"
                         + "<AddedProperty p2:type=\"p3:long\" xmlns:p3=\"http://www.w3.org/2001/XMLSchema\" xmlns:p2=\"http://www.w3.org/2001/XMLSchema-instance\">1</AddedProperty>"
                         + "<ChangedTypeProperty p2:type=\"p3:long\" xmlns:p3=\"http://www.w3.org/2001/XMLSchema\" xmlns:p2=\"http://www.w3.org/2001/XMLSchema-instance\">2</ChangedTypeProperty>"
                         + "<MultipleProperty p2:type=\"p3:Array\" p3:arrayType=\"p4:long[2]\" xmlns:p4=\"http://www.w3.org/2001/XMLSchema\" xmlns:p3=\"http://schemas.xmlsoap.org/soap/encoding/\" xmlns:p2=\"http://www.w3.org/2001/XMLSchema-instance\">"
                         + "<item p2:type=\"p4:long\">8</item>"
                         + "<item p2:type=\"p4:long\">9</item>"
                         + "</MultipleProperty>"
                         + "<RenamedToProperty p2:type=\"p3:long\" xmlns:p3=\"http://www.w3.org/2001/XMLSchema\" xmlns:p2=\"http://www.w3.org/2001/XMLSchema-instance\">4</RenamedToProperty>"
                         + "<StaticProperty p2:type=\"p3:long\" xmlns:p3=\"http://www.w3.org/2001/XMLSchema\" xmlns:p2=\"http://www.w3.org/2001/XMLSchema-instance\">6</StaticProperty>"
                         + "</keha>";

            SerializeWithContext("keha", value, 2u, false, false, (msg, xml) =>
            {
                Assert.Equal(expected, xml);
                Assert.Equal(0, msg.AllAttachments.Count);
            });
        }

        [Fact]
        public void SerializeV1DtoVersion()
        {
            var value = new WsdlChangesTestDto
            {
                AddedProperty = 1L,
                ChangedTypeProperty = 2L,
                RemovedProperty = 3L,
                RenamedToProperty = 4L,
                RenamedFromProperty = 5L,
                StaticProperty = 6L,
                SingleProperty = 7L,
                MultipleProperty = new[] { 8L, 9L }
            };

            var expected = "<?xml version=\"1.0\" encoding=\"utf-8\"?>"
                         + "<keha>"
                         + "<ChangedTypeProperty p2:type=\"p3:string\" xmlns:p3=\"http://www.w3.org/2001/XMLSchema\" xmlns:p2=\"http://www.w3.org/2001/XMLSchema-instance\">2</ChangedTypeProperty>"
                         + "<RemovedProperty p2:type=\"p3:long\" xmlns:p3=\"http://www.w3.org/2001/XMLSchema\" xmlns:p2=\"http://www.w3.org/2001/XMLSchema-instance\">3</RemovedProperty>"
                         + "<RenamedFromProperty p2:type=\"p3:long\" xmlns:p3=\"http://www.w3.org/2001/XMLSchema\" xmlns:p2=\"http://www.w3.org/2001/XMLSchema-instance\">4</RenamedFromProperty>"
                         + "<SingleProperty p2:type=\"p3:long\" xmlns:p3=\"http://www.w3.org/2001/XMLSchema\" xmlns:p2=\"http://www.w3.org/2001/XMLSchema-instance\">8</SingleProperty>"
                         + "<StaticProperty p2:type=\"p3:long\" xmlns:p3=\"http://www.w3.org/2001/XMLSchema\" xmlns:p2=\"http://www.w3.org/2001/XMLSchema-instance\">6</StaticProperty>"
                         + "</keha>";

            SerializeWithContext("keha", value, 1u, false, false, (msg, xml) =>
            {
                Assert.Equal(expected, xml);
                Assert.Equal(0, msg.AllAttachments.Count);
            });
        }

        [Fact]
        public void CanSerializeAnonymousType()
        {
            var entity = new ContainerType
            {
                KnownProperty = "value",
                AnonymousProperty = new AnonymousType
                {
                    Property1 = "1",
                    Property2 = "2",
                    Property3 = "3"
                }
            };

            var expected = "<?xml version=\"1.0\" encoding=\"utf-8\"?>"
                         + "<keha>"
                         + "<AnonymousProperty>"
                         + "<Property1 p3:type=\"p4:string\" xmlns:p4=\"http://www.w3.org/2001/XMLSchema\" xmlns:p3=\"http://www.w3.org/2001/XMLSchema-instance\">1</Property1>"
                         + "<Property2 p3:type=\"p4:string\" xmlns:p4=\"http://www.w3.org/2001/XMLSchema\" xmlns:p3=\"http://www.w3.org/2001/XMLSchema-instance\">2</Property2>"
                         + "<Property3 p3:type=\"p4:string\" xmlns:p4=\"http://www.w3.org/2001/XMLSchema\" xmlns:p3=\"http://www.w3.org/2001/XMLSchema-instance\">3</Property3>"
                         + "</AnonymousProperty>"
                         + "<KnownProperty p2:type=\"p3:string\" xmlns:p3=\"http://www.w3.org/2001/XMLSchema\" xmlns:p2=\"http://www.w3.org/2001/XMLSchema-instance\">value</KnownProperty>"
                         + "</keha>";

            SerializeWithContext("keha", entity, 2u, false, false, (msg, xml) =>
            {
                Assert.Equal(expected, xml);
                Assert.Equal(0, msg.AllAttachments.Count);
            });
        }
    }
}
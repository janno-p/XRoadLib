using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
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

        private static void SerializeWithContext<T>(string elementName, T value, uint dtoVersion, bool addEnvelope, Action<XRoadMessage, string> f)
        {
            var message = Globals.ServiceManager.CreateMessage();
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
                    writer.WriteAttributeString(PrefixConstants.Xmlns, PrefixConstants.SoapEnc, NamespaceConstants.Xmlns, NamespaceConstants.SoapEnc);
                    writer.WriteAttributeString(PrefixConstants.Xmlns, PrefixConstants.Xsi, NamespaceConstants.Xmlns, NamespaceConstants.Xsi);
                    writer.WriteAttributeString(PrefixConstants.Xmlns, PrefixConstants.Xsd, NamespaceConstants.Xmlns, NamespaceConstants.Xsd);
                    writer.WriteAttributeString(PrefixConstants.Xmlns, PrefixConstants.Target, NamespaceConstants.Xmlns, Globals.ServiceManager.ProducerNamespace);
                }

                writer.WriteStartElement(elementName);

                var propType = typeof(X<>).MakeGenericType(typeof(T));
                var methodInfo = propType.GetTypeInfo().GetMethod("Method");

                var operationDefinition = new OperationDefinition("Method", null, methodInfo);
                var requestDefinition = new RequestDefinition(operationDefinition, _ => false);

                var typeMap = Globals.ServiceManager.GetSerializer(dtoVersion).GetTypeMap(typeof(T));
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
        public void CanSerializeTypeWithIdenticalPropertyNamesWhenCaseIgnored()
        {
            var cls = new IgnoreCaseClass
            {
                ObjektID = 3,
                Objektid = new [] { 1L, 2, 3 }
            };

            var expected = "<keha>" +
                           "<Objektid>" +
                           "<item>1</item>" +
                           "<item>2</item>" +
                           "<item>3</item>" +
                           "</Objektid>" +
                           "<ObjektID>3</ObjektID>" +
                           "</keha>";

            SerializeWithContext("keha", cls, 1u, true, (msg, xml) =>
            {
                Assert.Equal(expected, xml);
                Assert.Equal(0, msg.AllAttachments.Count);
            });
        }

        [Fact]
        public void CanSerializeArrayContentInEnvelope()
        {
            var value = new[] { 5, 4, 3 };

            var expected = "<keha>"
                         + "<item>5</item>"
                         + "<item>4</item>"
                         + "<item>3</item>"
                         + "</keha>";

            SerializeWithContext("keha", value, 1u, true, (msg, xml) =>
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
                         + "<item>5</item>"
                         + "<item>4</item>"
                         + "<item>3</item>"
                         + "</keha>";

            SerializeWithContext("keha", value, 2u, false, (msg, xml) =>
            {
                Assert.Equal(expected, xml);
                Assert.Equal(0, msg.AllAttachments.Count);
            });
        }

        [Fact]
        public void CanSerializeBooleanFalseValue()
        {
            SerializeWithContext("keha", false, 2u, false, (msg, xml) =>
            {
                Assert.Equal("<?xml version=\"1.0\" encoding=\"utf-8\"?><keha>false</keha>", xml);
                Assert.Equal(0, msg.AllAttachments.Count);
            });
        }

        [Fact]
        public void CanSerializeBooleanTrueValue()
        {
            SerializeWithContext("keha", true, 2u, false, (msg, xml) =>
            {
                Assert.Equal("<?xml version=\"1.0\" encoding=\"utf-8\"?><keha>true</keha>", xml);
                Assert.Equal(0, msg.AllAttachments.Count);
            });
        }

        [Fact]
        public void CanSerializeDateTimeValue()
        {
            SerializeWithContext("keha", new DateTime(2000, 10, 12, 4, 14, 55, 989), 2u, false, (msg, xml) =>
            {
                Assert.Equal("<?xml version=\"1.0\" encoding=\"utf-8\"?><keha>2000-10-12T04:14:55.989</keha>", xml);
                Assert.Equal(0, msg.AllAttachments.Count);
            });
        }

        [Fact]
        public void CanSerializeDecimalValue()
        {
            SerializeWithContext("keha", 0.4M, 2u, false, (msg, xml) =>
            {
                Assert.Equal("<?xml version=\"1.0\" encoding=\"utf-8\"?><keha>0.4</keha>", xml);
                Assert.Equal(0, msg.AllAttachments.Count);
            });
        }

        [Fact]
        public void CanSerializeFloatValue()
        {
            SerializeWithContext("keha", 0.1f, 2u, false, (msg, xml) =>
            {
                Assert.Equal("<?xml version=\"1.0\" encoding=\"utf-8\"?><keha>0.1</keha>", xml);
                Assert.Equal(0, msg.AllAttachments.Count);
            });
        }

        [Fact]
        public void CanSerializeIntValue()
        {
            SerializeWithContext("keha", 44345, 2u, false, (msg, xml) =>
            {
                Assert.Equal("<?xml version=\"1.0\" encoding=\"utf-8\"?><keha>44345</keha>", xml);
                Assert.Equal(0, msg.AllAttachments.Count);
            });
        }

        [Fact]
        public void CanSerializeShortValue()
        {
            SerializeWithContext("keha", (short)445, 2u, false, (msg, xml) =>
            {
                Assert.Equal("<?xml version=\"1.0\" encoding=\"utf-8\"?><keha>445</keha>", xml);
                Assert.Equal(0, msg.AllAttachments.Count);
            });
        }

        [Fact]
        public void CanSerializeLongValue()
        {
            SerializeWithContext("keha", 44345L, 2u, false, (msg, xml) =>
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
                         + "<item>5</item>"
                         + "<item>4</item>"
                         + "<item>3</item>"
                         + "</keha>";

            SerializeWithContext("keha", value, 2u, false, (msg, xml) =>
            {
                Assert.Equal(expected, xml);
                Assert.Equal(0, msg.AllAttachments.Count);
            });
        }

        [Fact]
        public void CanSerializeShortDateTimeValue()
        {
            SerializeWithContext("keha", new DateTime(2000, 10, 12), 2u, false, (msg, xml) =>
            {
                Assert.Equal("<?xml version=\"1.0\" encoding=\"utf-8\"?><keha>2000-10-12T00:00:00</keha>", xml);
                Assert.Equal(0, msg.AllAttachments.Count);
            });
        }

        [Fact]
        public void CanSerializeStringValue()
        {
            SerializeWithContext("keha", "someString", 2u, false, (msg, xml) =>
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
                         + "<Nimi>Mauno</Nimi>"
                         + "<Kood>1235</Kood>"
                         + "<Loodud>2000-12-12T12:12:12</Loodud>"
                         + "</keha>";

            SerializeWithContext("keha", value, 2u, false, (msg, xml) =>
            {
                Assert.Equal(expected, xml);
                Assert.Equal(0, msg.AllAttachments.Count);
            });
        }

        [Fact]
        public void CanSerializeBinaryValue()
        {
            using var stream = new MemoryStream();

            var value = new XRoadBinaryTestDto { Sisu = stream };

            const string expected = "<?xml version=\"1.0\" encoding=\"utf-8\"?>"
                                    + "<keha>"
                                    + "<Sisu href=\"cid:1B2M2Y8AsgTpgAmY7PhCfg==\" />"
                                    + "</keha>";

            SerializeWithContext("keha", value, 2u, false, (msg, xml) =>
            {
                Assert.Equal(expected, xml);
                Assert.Equal(1, msg.AllAttachments.Count);
            });
        }

        [Fact]
        public void CanSerializeHexBinaryValue()
        {
            using var stream = new MemoryStream();

            var value = new XRoadHexTestDto { Sisu = stream };

            const string expected = "<?xml version=\"1.0\" encoding=\"utf-8\"?>"
                                    + "<keha>"
                                    + "<Sisu href=\"cid:1B2M2Y8AsgTpgAmY7PhCfg==\" />"
                                    + "</keha>";

            SerializeWithContext("keha", value, 2u, false, (msg, xml) =>
            {
                Assert.Equal(expected, xml);
                Assert.Equal(1, msg.AllAttachments.Count);
            });
        }

        [Fact]
        public void CanSerializeDateTypeWithCustomName()
        {
            var value = new DateTestDto { Synniaeg = new DateTime(2012, 11, 26, 16, 29, 13) };

            const string expected = "<?xml version=\"1.0\" encoding=\"utf-8\"?>"
                                    + "<keha>"
                                    + "<ttIsik.dSyn>2012-11-26</ttIsik.dSyn>"
                                    + "</keha>";

            SerializeWithContext("keha", value, 2u, false, (msg, xml) =>
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

            const string expected = "<?xml version=\"1.0\" encoding=\"utf-8\"?>"
                                    + "<keha>"
                                    + "<AddedProperty>1</AddedProperty>"
                                    + "<ChangedTypeProperty>2</ChangedTypeProperty>"
                                    + "<MultipleProperty>"
                                    + "<item>8</item>"
                                    + "<item>9</item>"
                                    + "</MultipleProperty>"
                                    + "<RenamedToProperty>4</RenamedToProperty>"
                                    + "<StaticProperty>6</StaticProperty>"
                                    + "</keha>";

            SerializeWithContext("keha", value, 2u, false, (msg, xml) =>
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

            const string expected = "<?xml version=\"1.0\" encoding=\"utf-8\"?>"
                                    + "<keha>"
                                    + "<ChangedTypeProperty>2</ChangedTypeProperty>"
                                    + "<RemovedProperty>3</RemovedProperty>"
                                    + "<RenamedFromProperty>4</RenamedFromProperty>"
                                    + "<SingleProperty>8</SingleProperty>"
                                    + "<StaticProperty>6</StaticProperty>"
                                    + "</keha>";

            SerializeWithContext("keha", value, 1u, false, (msg, xml) =>
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

            const string expected = "<?xml version=\"1.0\" encoding=\"utf-8\"?>"
                                    + "<keha>"
                                    + "<AnonymousProperty>"
                                    + "<Property1>1</Property1>"
                                    + "<Property2>2</Property2>"
                                    + "<Property3>3</Property3>"
                                    + "</AnonymousProperty>"
                                    + "<KnownProperty>value</KnownProperty>"
                                    + "</keha>";

            SerializeWithContext("keha", entity, 2u, false, (msg, xml) =>
            {
                Assert.Equal(expected, xml);
                Assert.Equal(0, msg.AllAttachments.Count);
            });
        }

        [Fact]
        public void CanSerializeMergedArrayContent()
        {
            var entity = new TestMergedArrayContent
            {
                Value = "Song",
                Codes = new[] { "One", "Two", "Three" },
                Value2 = "Joy"
            };

            const string expected =
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>"
                + "<keha>"
                + "<Value>Song</Value>"
                + "<Code>One</Code>"
                + "<Code>Two</Code>"
                + "<Code>Three</Code>"
                + "<Value2>Joy</Value2>"
                + "</keha>";

            SerializeWithContext("keha", entity, 2u, false, (msg, xml) =>
            {
                Assert.Equal(expected, xml);
                Assert.Equal(0, msg.AllAttachments.Count);
            });
        }
    }
}
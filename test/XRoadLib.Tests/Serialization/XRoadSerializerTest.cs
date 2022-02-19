﻿using System.Reflection;
using System.Xml;
using JetBrains.Annotations;
using XRoadLib.Extensions;
using XRoadLib.Schema;
using XRoadLib.Serialization;
using XRoadLib.Serialization.Template;
using XRoadLib.Tests.Contract;
using XRoadLib.Tests.Contract.Wsdl;

namespace XRoadLib.Tests.Serialization;

[UsedImplicitly]
public class XRoadSerializerTest
{
    private static class X<T>
    {
        [UsedImplicitly]
        public static void Method([UsedImplicitly] T t)
        {
            Console.WriteLine($"t: {t}");
        }
    }

    private static async Task SerializeWithContextAsync<T>(string elementName, T value, uint dtoVersion, bool addEnvelope, Action<XRoadMessage, string> f) where T : notnull
    {
        var message = Globals.ServiceManager.CreateMessage();
        message.IsMultipartContainer = true;
        message.BinaryMode = BinaryMode.Attachment;

        using (message)
#if NET5_0
            await
#endif
        using (var stream = new MemoryStream())

#if NET5_0
            await
#endif
        using (var tw = new StreamWriter(stream, XRoadEncoding.Utf8))
    
#if NET5_0
            await
#endif
        using (var writer = XmlWriter.Create(tw, new XmlWriterSettings { Async = true, Encoding = XRoadEncoding.Utf8 }))
        {
            if (addEnvelope)
            {
                await writer.WriteStartElementAsync("Envelope");
                await writer.WriteAttributeStringAsync(PrefixConstants.Xmlns, PrefixConstants.SoapEnc, NamespaceConstants.Xmlns, NamespaceConstants.SoapEnc);
                await writer.WriteAttributeStringAsync(PrefixConstants.Xmlns, PrefixConstants.Xsi, NamespaceConstants.Xmlns, NamespaceConstants.Xsi);
                await writer.WriteAttributeStringAsync(PrefixConstants.Xmlns, PrefixConstants.Xsd, NamespaceConstants.Xmlns, NamespaceConstants.Xsd);
                await writer.WriteAttributeStringAsync(PrefixConstants.Xmlns, PrefixConstants.Target, NamespaceConstants.Xmlns, Globals.ServiceManager.ProducerNamespace);
            }

            await writer.WriteStartElementAsync(elementName);

            var propType = typeof(X<>).MakeGenericType(typeof(T));
            var methodInfo = propType.GetTypeInfo().GetMethod(nameof(X<object>.Method))!;

            var operationDefinition = new OperationDefinition(nameof(X<object>.Method), null, methodInfo);
            var requestDefinition = new RequestDefinition(operationDefinition, _ => false);

            var typeMap = Globals.ServiceManager.GetSerializer(dtoVersion).GetTypeMap(typeof(T))!;
            await typeMap.SerializeAsync(writer, XRoadXmlTemplate.EmptyNode, value, requestDefinition.Content, message);

            await writer.WriteEndElementAsync();

            if (addEnvelope)
                await writer.WriteEndElementAsync();

            await writer.FlushAsync();

            stream.Position = 0;
            using var reader = new StreamReader(stream);

            if (!addEnvelope)
            {
                f(message, await reader.ReadToEndAsync());
                return;
            }

            using var xmlReader = XmlReader.Create(reader, new XmlReaderSettings { Async = true });

            await xmlReader.MoveToElementAsync(0, "Envelope");
            f(message, await xmlReader.ReadInnerXmlAsync());
        }
    }

    [Fact]
    public async Task CanSerializeTypeWithIdenticalPropertyNamesWhenCaseIgnored()
    {
        var cls = new IgnoreCaseClass
        {
            ObjektID = 3,
            Objektid = new [] { 1L, 2, 3 }
        };

        const string expected = "<keha>" +
                                "<Objektid>" +
                                "<item>1</item>" +
                                "<item>2</item>" +
                                "<item>3</item>" +
                                "</Objektid>" +
                                "<ObjektID>3</ObjektID>" +
                                "</keha>";

        await SerializeWithContextAsync("keha", cls, 1u, true, [AssertionMethod] (msg, xml) =>
        {
            Assert.Equal(expected, xml);
            Assert.Equal(0, msg.AllAttachments.Count);
        });
    }

    [Fact]
    public async Task CanSerializeArrayContentInEnvelope()
    {
        var value = new[] { 5, 4, 3 };

        const string expected = "<keha>"
                                + "<item>5</item>"
                                + "<item>4</item>"
                                + "<item>3</item>"
                                + "</keha>";

        await SerializeWithContextAsync("keha", value, 1u, true, [AssertionMethod] (msg, xml) =>
        {
            Assert.Equal(expected, xml);
            Assert.Equal(0, msg.AllAttachments.Count);
        });
    }

    [Fact]
    public async Task CanSerializeArrayContent()
    {
        var value = new[] { 5, 4, 3 };

        const string expected = "<?xml version=\"1.0\" encoding=\"utf-8\"?>"
                                + "<keha>"
                                + "<item>5</item>"
                                + "<item>4</item>"
                                + "<item>3</item>"
                                + "</keha>";

        await SerializeWithContextAsync("keha", value, 2u, false, [AssertionMethod] (msg, xml) =>
        {
            Assert.Equal(expected, xml);
            Assert.Equal(0, msg.AllAttachments.Count);
        });
    }

    [Fact]
    public async Task CanSerializeBooleanFalseValue()
    {
        await SerializeWithContextAsync("keha", false, 2u, false, [AssertionMethod] (msg, xml) =>
        {
            Assert.Equal("<?xml version=\"1.0\" encoding=\"utf-8\"?><keha>false</keha>", xml);
            Assert.Equal(0, msg.AllAttachments.Count);
        });
    }

    [Fact]
    public async Task CanSerializeBooleanTrueValue()
    {
        await SerializeWithContextAsync("keha", true, 2u, false, [AssertionMethod] (msg, xml) =>
        {
            Assert.Equal("<?xml version=\"1.0\" encoding=\"utf-8\"?><keha>true</keha>", xml);
            Assert.Equal(0, msg.AllAttachments.Count);
        });
    }

    [Fact]
    public async Task CanSerializeDateTimeValue()
    {
        await SerializeWithContextAsync("keha", new DateTime(2000, 10, 12, 4, 14, 55, 989), 2u, false, [AssertionMethod] (msg, xml) =>
        {
            Assert.Equal("<?xml version=\"1.0\" encoding=\"utf-8\"?><keha>2000-10-12T04:14:55.989</keha>", xml);
            Assert.Equal(0, msg.AllAttachments.Count);
        });
    }

    [Fact]
    public async Task CanSerializeDecimalValue()
    {
        await SerializeWithContextAsync("keha", 0.4M, 2u, false, [AssertionMethod] (msg, xml) =>
        {
            Assert.Equal("<?xml version=\"1.0\" encoding=\"utf-8\"?><keha>0.4</keha>", xml);
            Assert.Equal(0, msg.AllAttachments.Count);
        });
    }

    [Fact]
    public async Task CanSerializeFloatValue()
    {
        await SerializeWithContextAsync("keha", 0.1f, 2u, false, [AssertionMethod] (msg, xml) =>
        {
            Assert.Equal("<?xml version=\"1.0\" encoding=\"utf-8\"?><keha>0.1</keha>", xml);
            Assert.Equal(0, msg.AllAttachments.Count);
        });
    }

    [Fact]
    public async Task CanSerializeIntValue()
    {
        await SerializeWithContextAsync("keha", 44345, 2u, false, [AssertionMethod] (msg, xml) =>
        {
            Assert.Equal("<?xml version=\"1.0\" encoding=\"utf-8\"?><keha>44345</keha>", xml);
            Assert.Equal(0, msg.AllAttachments.Count);
        });
    }

    [Fact]
    public async Task CanSerializeShortValue()
    {
        await SerializeWithContextAsync("keha", (short)445, 2u, false, [AssertionMethod] (msg, xml) =>
        {
            Assert.Equal("<?xml version=\"1.0\" encoding=\"utf-8\"?><keha>445</keha>", xml);
            Assert.Equal(0, msg.AllAttachments.Count);
        });
    }

    [Fact]
    public async Task CanSerializeLongValue()
    {
        await SerializeWithContextAsync("keha", 44345L, 2u, false, [AssertionMethod] (msg, xml) =>
        {
            Assert.Equal("<?xml version=\"1.0\" encoding=\"utf-8\"?><keha>44345</keha>", xml);
            Assert.Equal(0, msg.AllAttachments.Count);
        });
    }

    [Fact]
    public async Task CanSerializeLongArrayValue()
    {
        var value = new[] { 5L, 4L, 3L };

        const string expected = "<?xml version=\"1.0\" encoding=\"utf-8\"?>"
                                + "<keha>"
                                + "<item>5</item>"
                                + "<item>4</item>"
                                + "<item>3</item>"
                                + "</keha>";

        await SerializeWithContextAsync("keha", value, 2u, false, [AssertionMethod] (msg, xml) =>
        {
            Assert.Equal(expected, xml);
            Assert.Equal(0, msg.AllAttachments.Count);
        });
    }

    [Fact]
    public async Task CanSerializeShortDateTimeValue()
    {
        await SerializeWithContextAsync("keha", new DateTime(2000, 10, 12), 2u, false, [AssertionMethod] (msg, xml) =>
        {
            Assert.Equal("<?xml version=\"1.0\" encoding=\"utf-8\"?><keha>2000-10-12T00:00:00</keha>", xml);
            Assert.Equal(0, msg.AllAttachments.Count);
        });
    }

    [Fact]
    public async Task CanSerializeStringValue()
    {
        await SerializeWithContextAsync("keha", "someString", 2u, false, [AssertionMethod] (msg, xml) =>
        {
            Assert.Equal("<?xml version=\"1.0\" encoding=\"utf-8\"?><keha>someString</keha>", xml);
            Assert.Equal(0, msg.AllAttachments.Count);
        });
    }

    [Fact]
    public async Task CanSerializeStructValue()
    {
        var value = new TestDto
        {
            Nimi = "Mauno",
            Kood = "1235",
            Loodud = new DateTime(2000, 12, 12, 12, 12, 12)
        };

        const string expected = "<?xml version=\"1.0\" encoding=\"utf-8\"?>"
                                + "<keha>"
                                + "<Nimi>Mauno</Nimi>"
                                + "<Kood>1235</Kood>"
                                + "<Loodud>2000-12-12T12:12:12</Loodud>"
                                + "</keha>";

        await SerializeWithContextAsync("keha", value, 2u, false, [AssertionMethod] (msg, xml) =>
        {
            Assert.Equal(expected, xml);
            Assert.Equal(0, msg.AllAttachments.Count);
        });
    }

    [Fact]
    public async Task CanSerializeBinaryValue()
    {
#if NET5_0
            await
#endif
        using var stream = new MemoryStream();

        var value = new XRoadBinaryTestDto { Sisu = stream };

        const string expected = "<?xml version=\"1.0\" encoding=\"utf-8\"?>"
                                + "<keha>"
                                + "<Sisu href=\"cid:1B2M2Y8AsgTpgAmY7PhCfg==\" />"
                                + "</keha>";

        await SerializeWithContextAsync("keha", value, 2u, false, [AssertionMethod] (msg, xml) =>
        {
            Assert.Equal(expected, xml);
            Assert.Equal(1, msg.AllAttachments.Count);
        });
    }

    [Fact]
    public async Task CanSerializeHexBinaryValue()
    {
#if NET5_0
            await
#endif
        using var stream = new MemoryStream();

        var value = new XRoadHexTestDto { Sisu = stream };

        const string expected = "<?xml version=\"1.0\" encoding=\"utf-8\"?>"
                                + "<keha>"
                                + "<Sisu href=\"cid:1B2M2Y8AsgTpgAmY7PhCfg==\" />"
                                + "</keha>";

        await SerializeWithContextAsync("keha", value, 2u, false, [AssertionMethod] (msg, xml) =>
        {
            Assert.Equal(expected, xml);
            Assert.Equal(1, msg.AllAttachments.Count);
        });
    }

    [Fact]
    public async Task CanSerializeDateTypeWithCustomName()
    {
        var value = new DateTestDto { Synniaeg = new DateTime(2012, 11, 26, 16, 29, 13) };

        const string expected = "<?xml version=\"1.0\" encoding=\"utf-8\"?>"
                                + "<keha>"
                                + "<ttIsik.dSyn>2012-11-26</ttIsik.dSyn>"
                                + "</keha>";

        await SerializeWithContextAsync("keha", value, 2u, false, [AssertionMethod] (msg, xml) =>
        {
            Assert.Equal(expected, xml);
            Assert.Equal(0, msg.AllAttachments.Count);
        });
    }

    [Fact]
    public async Task SerializeDefaultDtoVersion()
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

        await SerializeWithContextAsync("keha", value, 2u, false, [AssertionMethod] (msg, xml) =>
        {
            Assert.Equal(expected, xml);
            Assert.Equal(0, msg.AllAttachments.Count);
        });
    }

    [Fact]
    public async Task SerializeV1DtoVersion()
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

        await SerializeWithContextAsync("keha", value, 1u, false, [AssertionMethod] (msg, xml) =>
        {
            Assert.Equal(expected, xml);
            Assert.Equal(0, msg.AllAttachments.Count);
        });
    }

    [Fact]
    public async Task CanSerializeAnonymousType()
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

        await SerializeWithContextAsync("keha", entity, 2u, false, [AssertionMethod] (msg, xml) =>
        {
            Assert.Equal(expected, xml);
            Assert.Equal(0, msg.AllAttachments.Count);
        });
    }

    [Fact]
    public async Task CanSerializeMergedArrayContent()
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

        await SerializeWithContextAsync("keha", entity, 2u, false, [AssertionMethod] (msg, xml) =>
        {
            Assert.Equal(expected, xml);
            Assert.Equal(0, msg.AllAttachments.Count);
        });
    }
}
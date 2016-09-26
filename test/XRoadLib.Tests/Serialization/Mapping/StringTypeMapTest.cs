using System;
using System.IO;
using System.Text;
using System.Xml;
using XRoadLib.Protocols.Headers;
using XRoadLib.Serialization;
using XRoadLib.Serialization.Mapping;
using XRoadLib.Tests.Contract.Configuration;
using Xunit;

namespace XRoadLib.Tests.Serialization.Mapping
{
    public class StringTypeMapTest : TypeMapTestBase
    {
        private static readonly ITypeMap typeMap = new StringTypeMap(schemaDefinitionReader.GetSimpleTypeDefinition<string>("string"));
        private readonly Func<string, object> deserializeValue = x => DeserializeValue(typeMap, x);
        private readonly Func<object, string> serializeValue = x => SerializeValue(typeMap, x);

        [Fact]
        public void CanEscapeSpecialCharactersWithHtmlEncoding()
        {
            var xmlValue = serializeValue("&<>");
            Assert.Equal("&amp;&lt;&gt;", xmlValue);
        }

        [Fact]
        public void CanReadStringWithHtmlEncodedCharacters()
        {
            var value = deserializeValue("&amp;&lt;&gt;");
            Assert.Equal("&<>", value);
        }

        [Fact]
        public void CanEscapeCDataStringWithHtmlEncoding()
        {
            var xmlValue = serializeValue("<![CDATA[&<>]]>");
            Assert.Equal("&lt;![CDATA[&amp;&lt;&gt;]]&gt;", xmlValue);
        }

        [Fact]
        public void CanReadCDataStringWithHtmlEncodedCharacters()
        {
            var value = deserializeValue("&lt;![CDATA[&amp;&lt;&gt;]]&gt;");
            Assert.Equal("<![CDATA[&<>]]>", value);
        }

        [Fact]
        public void CanEscapeSpecialCharactersWithCDataBlock()
        {
            var xmlValue = SerializeValue(typeMap, "&<>", StringSerializationMode.WrappedInCData);
            Assert.Equal("<![CDATA[&<>]]>", xmlValue);
        }

        [Fact]
        public void CanReadStringFromCDataBlock()
        {
            var xmlValue = deserializeValue("<![CDATA[&<>]]>");
            Assert.Equal("&<>", xmlValue);
        }

        [Fact]
        public void CanEscapeCDataStringWithCDataBlock()
        {
            var xmlValue = SerializeValue(typeMap, "<![CDATA[&<>]]>", StringSerializationMode.WrappedInCData);
            Assert.Equal("<![CDATA[<![CDATA[&<>]]>]]<![CDATA[>]]>", xmlValue);
        }

        [Fact]
        public void CanReadCDataStringFromCDataBlock()
        {
            var xmlValue = deserializeValue("<![CDATA[<![CDATA[&<>]]>]]<![CDATA[>]]>");
            Assert.Equal("<![CDATA[&<>]]>", xmlValue);
        }

        private static string SerializeValue(ITypeMap typeMap, object value, StringSerializationMode mode = StringSerializationMode.HtmlEncoded)
        {
            var stream = new StringBuilder();

            var protocol = new CustomXRoad20Protocol();
            protocol.SetStringSerializationMode(mode);

            using (var textWriter = new StringWriter(stream))
            using (var writer = XmlWriter.Create(textWriter))
            using (var message = protocol.NewMessage())
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("value");
                typeMap.Serialize(writer, null, value, Globals.GetTestDefinition(typeMap.Definition.Type), message);
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }

            using (var textReader = new StringReader(stream.ToString()))
            using (var reader = XmlReader.Create(textReader))
            {
                while (reader.Read() && reader.LocalName != "value") { }
                return reader.ReadInnerXml();
            }
        }
    }
}
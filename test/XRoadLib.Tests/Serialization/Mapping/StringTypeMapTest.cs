using System;
using System.IO;
using System.Text;
using System.Xml;
using XRoadLib.Headers;
using XRoadLib.Serialization;
using XRoadLib.Serialization.Mapping;
using XRoadLib.Tests.Contract.Configuration;
using Xunit;

namespace XRoadLib.Tests.Serialization.Mapping
{
    public class StringTypeMapTest : TypeMapTestBase
    {
        private static readonly ITypeMap TypeMap = new StringTypeMap(SchemaDefinitionProvider.GetSimpleTypeDefinition<string>("string"));

        private readonly Func<string, object> _deserializeValue = x => DeserializeValue(TypeMap, x);
        private readonly Func<object, string> _serializeValue = x => SerializeValue(TypeMap, x);

        [Fact]
        public void CanEscapeSpecialCharactersWithHtmlEncoding()
        {
            var xmlValue = _serializeValue("&<>");
            Assert.Equal("&amp;&lt;&gt;", xmlValue);
        }

        [Fact]
        public void CanReadStringWithHtmlEncodedCharacters()
        {
            var value = _deserializeValue("&amp;&lt;&gt;");
            Assert.Equal("&<>", value);
        }

        [Fact]
        public void CanEscapeCDataStringWithHtmlEncoding()
        {
            var xmlValue = _serializeValue("<![CDATA[&<>]]>");
            Assert.Equal("&lt;![CDATA[&amp;&lt;&gt;]]&gt;", xmlValue);
        }

        [Fact]
        public void CanReadCDataStringWithHtmlEncodedCharacters()
        {
            var value = _deserializeValue("&lt;![CDATA[&amp;&lt;&gt;]]&gt;");
            Assert.Equal("<![CDATA[&<>]]>", value);
        }

        [Fact]
        public void CanEscapeSpecialCharactersWithCDataBlock()
        {
            var xmlValue = SerializeValue(TypeMap, "&<>", StringSerializationMode.WrappedInCData);
            Assert.Equal("<![CDATA[&<>]]>", xmlValue);
        }

        [Fact]
        public void CanReadStringFromCDataBlock()
        {
            var xmlValue = _deserializeValue("<![CDATA[&<>]]>");
            Assert.Equal("&<>", xmlValue);
        }

        [Fact]
        public void CanEscapeCDataStringWithCDataBlock()
        {
            var xmlValue = SerializeValue(TypeMap, "<![CDATA[&<>]]>", StringSerializationMode.WrappedInCData);
            Assert.Equal("<![CDATA[<![CDATA[&<>]]>]]<![CDATA[>]]>", xmlValue);
        }

        [Fact]
        public void CanReadCDataStringFromCDataBlock()
        {
            var xmlValue = _deserializeValue("<![CDATA[<![CDATA[&<>]]>]]<![CDATA[>]]>");
            Assert.Equal("<![CDATA[&<>]]>", xmlValue);
        }

        private static string SerializeValue(ITypeMap typeMap, object value, StringSerializationMode mode = StringSerializationMode.HtmlEncoded)
        {
            var stream = new StringBuilder();

            var protocol = new ServiceManager<XRoadHeader20>("2.0", new CustomSchemaExporterXRoad20(mode));

            using (var textWriter = new StringWriter(stream))
            using (var writer = XmlWriter.Create(textWriter))
            using (var message = protocol.CreateMessage())
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
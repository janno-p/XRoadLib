using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using XRoadLib.Schema;
using XRoadLib.Serialization;
using XRoadLib.Serialization.Mapping;
using Xunit;

namespace XRoadLib.Tests.Serialization.Mapping
{
    public class StringTypeMapTest : TypeMapTestBase
    {
        private static readonly ITypeMap TypeMap = new StringTypeMap(SchemaDefinitionProvider.GetSimpleTypeDefinition<string>("string"));

        private readonly Func<string, Task<object>> _deserializeValueAsync = x => DeserializeValueAsync(TypeMap, x);
        private readonly Func<object, Task<string>> _serializeValueAsync = x => SerializeValueAsync(TypeMap, x);

        [Fact]
        public async Task CanEscapeSpecialCharactersWithHtmlEncoding()
        {
            var xmlValue = await _serializeValueAsync("&<>");
            Assert.Equal("&amp;&lt;&gt;", xmlValue);
        }

        [Fact]
        public async Task CanReadStringWithHtmlEncodedCharacters()
        {
            var value = await _deserializeValueAsync("&amp;&lt;&gt;");
            Assert.Equal("&<>", value);
        }

        [Fact]
        public async Task CanEscapeCDataStringWithHtmlEncoding()
        {
            var xmlValue = await _serializeValueAsync("<![CDATA[&<>]]>");
            Assert.Equal("&lt;![CDATA[&amp;&lt;&gt;]]&gt;", xmlValue);
        }

        [Fact]
        public async Task CanReadCDataStringWithHtmlEncodedCharacters()
        {
            var value = await _deserializeValueAsync("&lt;![CDATA[&amp;&lt;&gt;]]&gt;");
            Assert.Equal("<![CDATA[&<>]]>", value);
        }

        [Fact]
        public async Task CanReadStringFromCDataBlock()
        {
            var xmlValue = await _deserializeValueAsync("<![CDATA[&<>]]>");
            Assert.Equal("&<>", xmlValue);
        }

        [Fact]
        public async Task CanReadCDataStringFromCDataBlock()
        {
            var xmlValue = await _deserializeValueAsync("<![CDATA[<![CDATA[&<>]]>]]<![CDATA[>]]>");
            Assert.Equal("<![CDATA[&<>]]>", xmlValue);
        }

        private static async Task<string> SerializeValueAsync(ITypeMap typeMap, object value)
        {
            var stream = new StringBuilder();

            var protocol = new ServiceManager("4.0", new DefaultSchemaExporter("urn:some-namespace", typeof(Contract.Class1).Assembly));

            using (var textWriter = new StringWriter(stream))
            using (var writer = XmlWriter.Create(textWriter, new XmlWriterSettings { Async = true, Encoding = XRoadEncoding.Utf8 }))
            using (var message = protocol.CreateMessage())
            {
                await writer.WriteStartDocumentAsync();
                await writer.WriteStartElementAsync(null, "value", "");
                await typeMap.SerializeAsync(writer, null, value, Globals.GetTestDefinition(typeMap.Definition.Type), message);
                await writer.WriteEndElementAsync();
                await writer.WriteEndDocumentAsync();
            }

            using var textReader = new StringReader(stream.ToString());
            using var reader = XmlReader.Create(textReader, new XmlReaderSettings { Async = true });

            while (await reader.ReadAsync() && reader.LocalName != "value") { }

            return await reader.ReadInnerXmlAsync();
        }
    }
}
using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using XRoadLib.Serialization;
using XRoadLib.Serialization.Mapping;
using Xunit;

namespace XRoadLib.Tests.Serialization.Mapping
{
    public class OptimizedContentTypeMapTest : TypeMapTestBase
    {
        private static readonly OptimizedContentTypeMap optimizedContentTypeMap = new OptimizedContentTypeMap(new ContentTypeMap(schemaDefinitionProvider.GetSimpleTypeDefinition<Stream>("base64")));
        private static readonly Func<string, object> deserializeValue = x => DeserializeValue(optimizedContentTypeMap, x);

        [Fact]
        public void CanDeserializeXopIncludeReference()
        {
            using (var attachment = new XRoadAttachment(Encoding.UTF8.GetBytes("Test")))
            using (var message = Globals.ServiceManager20.CreateMessage())
            {
                message.AllAttachments.Add(attachment);

                var instance = DeserializeValue(
                    new XElement(
                        "value",
                        new XElement(
                            XName.Get("Include", NamespaceConstants.XOP),
                            new XAttribute("href", attachment.ContentID)
                        )
                    ),
                    message
                );

                Assert.NotNull(instance);
                Assert.IsAssignableFrom<Stream>(instance);
                Assert.Same(instance, attachment.ContentStream);
            }
        }

        [Fact]
        public void CanDeserializeBase64Content()
        {
            var instance = deserializeValue("VsOkaWtl");
            Assert.NotNull(instance);
            var stream = Assert.IsAssignableFrom<MemoryStream>(instance);
            var sisu = Encoding.UTF8.GetString(stream.ToArray());
            Assert.Equal("Väike", sisu);
        }

        [Fact]
        public void CanDeserializeBase64ContentWithSpaces()
        {
            var instance = deserializeValue("\r\n\t   VsOkaWtl\n");
            Assert.NotNull(instance);
            var stream = Assert.IsAssignableFrom<MemoryStream>(instance);
            var sisu = Encoding.UTF8.GetString(stream.ToArray());
            Assert.Equal("Väike", sisu);
        }

        [Fact]
        public void CanDeserializeBase64ContentWithCData()
        {
            var instance = deserializeValue("<![CDATA[VsOkaWtl]]>");
            Assert.NotNull(instance);
            var stream = Assert.IsAssignableFrom<MemoryStream>(instance);
            var sisu = Encoding.UTF8.GetString(stream.ToArray());
            Assert.Equal("Väike", sisu);
        }

        [Fact]
        public void CanDeserializeEmptyBase64Content()
        {
            var instance = deserializeValue("");
            Assert.NotNull(instance);
            var stream = Assert.IsAssignableFrom<MemoryStream>(instance);
            var sisu = Encoding.UTF8.GetString(stream.ToArray());
            Assert.Equal("", sisu);
        }

        [Fact]
        public void CanDeserializeEmptySelfClosingBase64Content()
        {
            using (var message = Globals.ServiceManager20.CreateMessage())
            {
                var instance = DeserializeValue(
                    new XElement("value"),
                    message
                );

                Assert.NotNull(instance);
                var stream = Assert.IsAssignableFrom<MemoryStream>(instance);
                var sisu = Encoding.UTF8.GetString(stream.ToArray());
                Assert.Equal("", sisu);
            }
        }

        private static object DeserializeValue(XElement rootElement, XRoadMessage message)
        {
            var document = new XDocument();
            document.Add(rootElement);

            using (var reader = document.CreateReader())
            {
                while (reader.Read() && reader.NodeType != XmlNodeType.Element) { }

                return optimizedContentTypeMap.Deserialize(
                    reader,
                    null,
                    Globals.GetTestDefinition(optimizedContentTypeMap.Definition.Type),
                    message
                );
            }
        }
    }
}
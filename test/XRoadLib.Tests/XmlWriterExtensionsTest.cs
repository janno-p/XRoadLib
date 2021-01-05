using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using XRoadLib.Extensions;
using XRoadLib.Serialization;
using Xunit;

namespace XRoadLib.Tests
{
    public class XmlWriterExtensionsTest
    {
        [Fact]
        public async Task WriteQualifiedAttributeTest()
        {
#if NET5_0
            await
#endif
                using var stream = new MemoryStream();
            
#if NET5_0
            await
#endif
            using var writer = XmlWriter.Create(stream, new XmlWriterSettings { Async = true, Encoding = XRoadEncoding.Utf8 });

            await writer.WriteStartDocumentAsync();
            await writer.WriteStartElementAsync(null, "test", null);
            await writer.WriteAttributeStringAsync(PrefixConstants.Xmlns, "ppp", NamespaceConstants.Xmlns, "urn:qualifiedNamespace");
            await writer.WriteQualifiedAttributeAsync("attributeName", new XmlQualifiedName("qualifiedName", "urn:qualifiedNamespace"));
            await writer.WriteEndElementAsync();
            await writer.WriteEndDocumentAsync();
            await writer.FlushAsync();

            stream.Seek(0, SeekOrigin.Begin);

            var document = XDocument.Load(stream);

            var attribute = document.Element(XName.Get("test"))?.Attribute("attributeName");
            
            Assert.NotNull(attribute);
            Assert.Equal("ppp:qualifiedName", attribute.Value);
        }
        
        [Fact]
        public async Task WriteTypeAttributeTest()
        {
#if NET5_0
            await
#endif
                using var stream = new MemoryStream();
            
#if NET5_0
            await
#endif
                using var writer = XmlWriter.Create(stream, new XmlWriterSettings { Async = true, Encoding = XRoadEncoding.Utf8 });

            await writer.WriteStartDocumentAsync();
            await writer.WriteStartElementAsync(null, "test", null);
            await writer.WriteAttributeStringAsync(PrefixConstants.Xmlns, "ppp", NamespaceConstants.Xmlns, "urn:qualifiedNamespace");
            await writer.WriteTypeAttributeAsync("typeName", "urn:qualifiedNamespace");
            await writer.WriteEndElementAsync();
            await writer.WriteEndDocumentAsync();
            await writer.FlushAsync();

            stream.Seek(0, SeekOrigin.Begin);

            var document = XDocument.Load(stream);

            var attribute = document.Element(XName.Get("test"))?.Attribute(XName.Get("type", NamespaceConstants.Xsi));
            
            Assert.NotNull(attribute);
            Assert.Equal("ppp:typeName", attribute.Value);
        }
        
        [Fact]
        public async Task WriteArrayTypeAttributeTest()
        {
#if NET5_0
            await
#endif
                using var stream = new MemoryStream();
            
#if NET5_0
            await
#endif
                using var writer = XmlWriter.Create(stream, new XmlWriterSettings { Async = true, Encoding = XRoadEncoding.Utf8 });

            await writer.WriteStartDocumentAsync();
            await writer.WriteStartElementAsync(null, "test", null);
            await writer.WriteAttributeStringAsync(PrefixConstants.Xmlns, "ppp", NamespaceConstants.Xmlns, "urn:qualifiedNamespace");
            await writer.WriteArrayTypeAttributeAsync(XName.Get("typeName", "urn:qualifiedNamespace"), 73);
            await writer.WriteEndElementAsync();
            await writer.WriteEndDocumentAsync();
            await writer.FlushAsync();

            stream.Seek(0, SeekOrigin.Begin);

            var document = XDocument.Load(stream);

            var attribute = document.Element(XName.Get("test"))?.Attribute(XName.Get("arrayType", NamespaceConstants.SoapEnc));
            
            Assert.NotNull(attribute);
            Assert.Equal("ppp:typeName[73]", attribute.Value);
        }
    }
}
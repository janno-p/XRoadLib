using System.Xml;
using System.Xml.Linq;
using XRoadLib.Serialization;
using XRoadLib.Serialization.Mapping;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Tests.Serialization.Mapping;

public class OptimizedContentTypeMapTest : TypeMapTestBase
{
    private static readonly OptimizedContentTypeMap OptimizedContentTypeMap = new(new ContentTypeMap(SchemaDefinitionProvider.GetSimpleTypeDefinition<Stream>(XmlTypeConstants.Base64.LocalName)));
    private static readonly Func<string, Task<object>> DeserializeOptimizedContentValueAsync = x => DeserializeValueAsync(OptimizedContentTypeMap, x);

    [Fact]
    public async Task CanDeserializeXopIncludeReference()
    {
        using var attachment = new XRoadAttachment(XRoadEncoding.Utf8.GetBytes("Test"));
        using var message = Globals.ServiceManager.CreateMessage();

        message.AllAttachments.Add(attachment);

        var instance = await DeserializeValueAsync(
            new XElement(
                "value",
                new XElement(
                    XName.Get("Include", NamespaceConstants.Xop),
                    new XAttribute("href", attachment.ContentId)
                )
            ),
            message
        );

        Assert.NotNull(instance);
        Assert.IsAssignableFrom<Stream>(instance);
        Assert.Same(instance, attachment.ContentStream);
    }

    [Fact]
    public async Task CanDeserializeBase64Content()
    {
        var instance = await DeserializeOptimizedContentValueAsync("VsOkaWtl");
        Assert.NotNull(instance);
        var stream = Assert.IsAssignableFrom<MemoryStream>(instance);
        var sisu = XRoadEncoding.Utf8.GetString(stream.ToArray());
        Assert.Equal("Väike", sisu);
    }

    [Fact]
    public async Task CanDeserializeBase64ContentWithSpaces()
    {
        var instance = await DeserializeOptimizedContentValueAsync("\r\n\t   VsOkaWtl\n");
        Assert.NotNull(instance);
        var stream = Assert.IsAssignableFrom<MemoryStream>(instance);
        var sisu = XRoadEncoding.Utf8.GetString(stream.ToArray());
        Assert.Equal("Väike", sisu);
    }

    [Fact]
    public async Task CanDeserializeBase64ContentWithCData()
    {
        var instance = await DeserializeOptimizedContentValueAsync("<![CDATA[VsOkaWtl]]>");
        Assert.NotNull(instance);
        var stream = Assert.IsAssignableFrom<MemoryStream>(instance);
        var sisu = XRoadEncoding.Utf8.GetString(stream.ToArray());
        Assert.Equal("Väike", sisu);
    }

    [Fact]
    public async Task CanDeserializeEmptyBase64Content()
    {
        var instance = await DeserializeOptimizedContentValueAsync("");
        Assert.NotNull(instance);
        var stream = Assert.IsAssignableFrom<MemoryStream>(instance);
        var sisu = XRoadEncoding.Utf8.GetString(stream.ToArray());
        Assert.Equal("", sisu);
    }

    [Fact]
    public async Task CanDeserializeEmptySelfClosingBase64Content()
    {
        using var message = Globals.ServiceManager.CreateMessage();

        var instance = await DeserializeValueAsync(
            new XElement("value"),
            message
        );

        Assert.NotNull(instance);
        var stream = Assert.IsAssignableFrom<MemoryStream>(instance);
        var sisu = XRoadEncoding.Utf8.GetString(stream.ToArray());
        Assert.Equal("", sisu);
    }

    private static async Task<object> DeserializeValueAsync(XElement rootElement, XRoadMessage message)
    {
        var document = new XDocument();
        document.Add(rootElement);

        using var reader = document.CreateAsyncReader();

        while (await reader.ReadAsync() && reader.NodeType != XmlNodeType.Element)
        {
            // Do nothing
        }

        return await OptimizedContentTypeMap.DeserializeAsync(
            reader,
            XRoadXmlTemplate.EmptyNode,
            Globals.GetTestDefinition(OptimizedContentTypeMap.Definition.Type),
            message
        );
    }
}
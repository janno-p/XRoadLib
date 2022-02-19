using System.Xml;
using System.Xml.Linq;
using XRoadLib.Extensions;
using XRoadLib.Headers;
using XRoadLib.Serialization;
using XRoadLib.Tests.Contract.Mapping;

namespace XRoadLib.Tests.Serialization;

public class ArrayTest
{
    private static readonly IServiceManager ServiceManager = Globals.ServiceManager;

    private readonly ISerializer _serializer = ServiceManager.GetSerializer(1u);

    private readonly MergeArrayContentRequest _request = new()
    {
        StartDate = new DateTime(2016, 2, 10),
        EndDate = new DateTime(2016, 2, 11),
        Content = new[]
        {
            new WrapperType { Integers = new[] { 10 }, Strings = new[] { "test" } },
            new WrapperType(),
            new WrapperType { Integers = new[] { 1, 2, 3 }, Strings = new[] { "1", "2", "3" } }
        }
    };

    [Fact]
    public async Task CanSerializeMergedArrayContent()
    {
        var doc = await SerializeMessageAsync(_request);
        Assert.Equal(XName.Get("MergeArrayContent", Globals.ServiceManager.ProducerNamespace), doc.Root?.Name);

        doc.Root.Should().NotBeNull();
        doc.Root!.Elements().Should().NotBeNull().And.HaveCount(1);

        var req = doc.Root.Elements("request").Single();
        Assert.Equal(5, req.Elements().Count());

        var propertyNames = new[] { "StartDate", "EndDate", "Content", "Content", "Content" };
        propertyNames.Zip(req.Elements(), (name, element) => new { element, name })
                     .ToList()
                     .ForEach(x => Assert.Equal(XName.Get(x.name), x.element.Name));

        Assert.Equal("2016-02-10", req.Elements("StartDate").Single().Value);
        Assert.Equal("2016-02-11", req.Elements("EndDate").Single().Value);

        var contentElements = req.Elements("Content").ToList();
        Assert.Equal(3, contentElements.Count);

        var content1 = contentElements[0].Elements().ToList();
        Assert.Equal(2, content1.Count);
        Assert.Equal("Integer", content1[0].Name);
        Assert.Equal("10", content1[0].Value);
        Assert.Equal("String", content1[1].Name);
        Assert.Equal("test", content1[1].Value);

        Assert.Empty(contentElements[1].Elements());

        var content2 = contentElements[2].Elements().ToList();
        Assert.Equal(6, content2.Count);
        Assert.Equal("Integer", content2[0].Name);
        Assert.Equal("1", content2[0].Value);
        Assert.Equal("Integer", content2[1].Name);
        Assert.Equal("2", content2[1].Value);
        Assert.Equal("Integer", content2[2].Name);
        Assert.Equal("3", content2[2].Value);
        Assert.Equal("String", content2[3].Name);
        Assert.Equal("1", content2[3].Value);
        Assert.Equal("String", content2[4].Name);
        Assert.Equal("2", content2[4].Value);
        Assert.Equal("String", content2[5].Name);
        Assert.Equal("3", content2[5].Value);
    }

    [Fact]
    public async Task CanDeserializeMergedArrayContent()
    {
        var result = await DeserializeMessageAsync(await SerializeMessageAsync(_request));
        Assert.NotNull(result);
        Assert.IsType<MergeArrayContentRequest>(result);

        var req = Assert.IsType<MergeArrayContentRequest>(result);
        Assert.Equal(new DateTime(2016, 2, 10), req.StartDate);
        Assert.Equal(new DateTime(2016, 2, 11), req.EndDate);
        Assert.NotNull(req.Content);
        Assert.Equal(3, req.Content!.Length);
        Assert.NotNull(req.Content[0]);
        Assert.Collection(req.Content[0].Integers!, x => Assert.Equal(10, x));
        Assert.Collection(req.Content[0].Strings!, x => Assert.Equal("test", x));
        Assert.NotNull(req.Content[1]);
        Assert.Null(req.Content[1].Integers);
        Assert.Null(req.Content[1].Strings);
        Assert.NotNull(req.Content[2]);
        Assert.Collection(req.Content[2].Integers!, x => Assert.Equal(1, x), x => Assert.Equal(2, x), x => Assert.Equal(3, x));
        Assert.Collection(req.Content[2].Strings!, x => Assert.Equal("1", x), x => Assert.Equal("2", x), x => Assert.Equal("3", x));
    }

    private async Task<XDocument> SerializeMessageAsync(object request)
    {
        var serviceMap = _serializer.GetServiceMap("MergeArrayContent")!;

        using var message = new XRoadMessage(ServiceManager, new XRoadHeader());

#if NET5_0
            await
#endif
        using var stream = new MemoryStream();

#if NET5_0
            await
#endif
        using (var writer = XmlWriter.Create(stream, new XmlWriterSettings { Async = true, CloseOutput = false, Encoding = XRoadEncoding.Utf8 }))
        {
            await serviceMap.SerializeRequestAsync(writer, request, message);
            await writer.FlushAsync();
        }

        stream.Position = 0;

        return XDocument.Load(stream);
    }

    private async Task<object> DeserializeMessageAsync(XDocument document)
    {
        var serviceMap = _serializer.GetServiceMap("MergeArrayContent")!;
        var doc2 = new XDocument(new XElement("envelope", new XElement("body", document.Root)));

        using var message = new XRoadMessage(ServiceManager, new XRoadHeader());
        using var reader = doc2.CreateAsyncReader();

        for (var i = 0; i < 3; i++)
            await reader.MoveToElementAsync(i);

        return await serviceMap.DeserializeRequestAsync(reader, message);
    }
}
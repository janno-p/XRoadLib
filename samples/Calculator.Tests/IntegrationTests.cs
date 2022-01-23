using System.Net.Http.Headers;
using System.Xml;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc.Testing;
using XRoadLib;
using XRoadLib.Extensions;
using XRoadLib.Serialization;
using XRoadLib.Soap;
using Xunit;
using Xunit.Abstractions;

namespace Calculator.Tests;

public class IntegrationTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly WebApplicationFactory<Program> _factory;

    public IntegrationTests(ITestOutputHelper testOutputHelper)
    {
        _factory = new WebApplicationFactory<Program>();
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task GetWsdl()
    {
        using var client = _factory.CreateClient();
        using var response = await client.GetAsync("/");
        await using var responseStream = await response.Content.ReadAsStreamAsync();

        await using var copy = new MemoryStream();
        await responseStream.CopyToAsync(copy);

        copy.Seek(0, SeekOrigin.Begin);
        using var reader = XmlReader.Create(copy, new XmlReaderSettings { Async = true });

        var formatter = new SoapMessageFormatter();
        if (await formatter.TryMoveToEnvelopeAsync(reader) && await formatter.TryMoveToBodyAsync(reader) && await reader.MoveToElementAsync(2))
            await formatter.ThrowSoapFaultIfPresentAsync(reader);

        copy.Seek(0, SeekOrigin.Begin);
        var content = await XDocument.LoadAsync(copy, LoadOptions.None, default);

        _testOutputHelper.WriteLine(content.ToString());
    }

    [Fact]
    public async Task MissingContentType()
    {
        using var client = _factory.CreateClient();
        using var response = await client.PostAsync("/", new StringContent(""));
        await using var responseStream = await response.Content.ReadAsStreamAsync();
        var content = await XDocument.LoadAsync(responseStream, LoadOptions.None, default);

        _testOutputHelper.WriteLine(content.ToString());

        var fault = Fault.FromXml(content);
        Assert.Equal("Client.InvalidQuery", fault.Code);
        Assert.Equal("Unknown content type `text/plain` used for message payload.", fault.Value);
    }

    [Fact]
    public async Task EmptyRequestContent()
    {
        using var client = _factory.CreateClient();

        var doc = new XDocument();

        using var response = await client.PostAsync("/", new StringContent(doc.ToString())
        {
            Headers = { ContentType = MediaTypeHeaderValue.Parse(ContentTypes.Soap) }
        });

        await using var responseStream = await response.Content.ReadAsStreamAsync();
        var content = await XDocument.LoadAsync(responseStream, LoadOptions.None, default);

        _testOutputHelper.WriteLine(content.ToString());

        var fault = Fault.FromXml(content);
        Assert.Equal("Client.InvalidQuery", fault.Code);
        Assert.Equal("Empty request content", fault.Value);
    }

    [Fact]
    public async Task EmptyEnvelope()
    {
        using var client = _factory.CreateClient();

        var doc = new XDocument(
            new XElement(XName.Get("Envelope", NamespaceConstants.SoapEnv))
        );

        using var response = await client.PostAsync("/", new StringContent(doc.ToString())
        {
            Headers = { ContentType = MediaTypeHeaderValue.Parse(ContentTypes.Soap) }
        });

        await using var responseStream = await response.Content.ReadAsStreamAsync();
        var content = await XDocument.LoadAsync(responseStream, LoadOptions.None, default);

        _testOutputHelper.WriteLine(content.ToString());

        var fault = Fault.FromXml(content);
        Assert.Equal("Client.UnknownOperation", fault.Code);
        Assert.Equal("Could not resolve operation name from request.", fault.Value);
    }

    [Fact]
    public async Task UndefinedOperation()
    {
        using var client = _factory.CreateClient();

        var doc = new XDocument(
            new XElement(XName.Get("Envelope", NamespaceConstants.SoapEnv),
                new XElement(XName.Get("Body", NamespaceConstants.SoapEnv),
                    new XElement(XName.Get("rootNamespace"))
                )
            )
        );

        using var response = await client.PostAsync("/", new StringContent(doc.ToString())
        {
            Headers = { ContentType = MediaTypeHeaderValue.Parse(ContentTypes.Soap) }
        });

        await using var responseStream = await response.Content.ReadAsStreamAsync();
        var content = await XDocument.LoadAsync(responseStream, LoadOptions.None, default);

        _testOutputHelper.WriteLine(content.ToString());

        var fault = Fault.FromXml(content);
        Assert.Equal("Client.UnknownOperation", fault.Code);
        Assert.Equal("The operation `rootNamespace` is not defined by contract.", fault.Value);
    }

    [Fact]
    public async Task CallServiceWithoutRequest()
    {
        using var client = _factory.CreateClient();

        var doc = new XDocument(
            new XElement(XName.Get("Envelope", NamespaceConstants.SoapEnv),
                new XElement(XName.Get("Body", NamespaceConstants.SoapEnv),
                    new XElement(XName.Get("Calculate", "http://calculator.x-road.eu/"))
                )
            )
        );

        using var response = await client.PostAsync("/", new StringContent(doc.ToString())
        {
            Headers = { ContentType = MediaTypeHeaderValue.Parse(ContentTypes.Soap) }
        });

        await using var responseStream = await response.Content.ReadAsStreamAsync();
        var content = await XDocument.LoadAsync(responseStream, LoadOptions.None, default);

        _testOutputHelper.WriteLine(content.ToString());

        var fault = Fault.FromXml(content);
        Assert.Equal("Client.InvalidQuery", fault.Code);
        Assert.Equal("Request wrapper element `request` was not found in incoming SOAP message.", fault.Value);
    }

    [Fact]
    public async Task CallService()
    {
        using var client = _factory.CreateClient();

        var doc = new XDocument(
            new XElement(XName.Get("Envelope", NamespaceConstants.SoapEnv),
                new XElement(XName.Get("Body", NamespaceConstants.SoapEnv),
                    new XElement(XName.Get("Calculate", "http://calculator.x-road.eu/"),
                        new XElement(XName.Get("request"),
                            new XElement("X", 13),
                            new XElement("Y", 91),
                            new XElement("Operation", "Multiply")
                        )
                    )
                )
            )
        );

        using var response = await client.PostAsync("/", new StringContent(doc.ToString())
        {
            Headers = { ContentType = MediaTypeHeaderValue.Parse(ContentTypes.Soap) }
        });

        await using var responseStream = await response.Content.ReadAsStreamAsync();
        var content = await XDocument.LoadAsync(responseStream, LoadOptions.None, default);

        _testOutputHelper.WriteLine(content.ToString());

        var body = GetBodyElement(content);

        var wrapper = body.Element(XName.Get("CalculateResponse", "http://calculator.x-road.eu/"));
        wrapper.Should().NotBeNull();

        var result = wrapper!.Element(XName.Get("response"));
        result.Should().NotBeNull();

        result!.Value.Should().Be("1183");
    }

    [Fact]
    public async Task CallAsyncService()
    {
        using var client = _factory.CreateClient();

        var doc = new XDocument(
            new XElement(XName.Get("Envelope", NamespaceConstants.SoapEnv),
                new XElement(XName.Get("Body", NamespaceConstants.SoapEnv),
                    new XElement(XName.Get("CalculateAsync", "http://calculator.x-road.eu/"),
                        new XElement(XName.Get("request"),
                            new XElement("X", 13),
                            new XElement("Y", 91),
                            new XElement("Operation", "Multiply")
                        )
                    )
                )
            )
        );

        using var response = await client.PostAsync("/", new StringContent(doc.ToString())
        {
            Headers = { ContentType = MediaTypeHeaderValue.Parse(ContentTypes.Soap) }
        });

        await using var responseStream = await response.Content.ReadAsStreamAsync();
        var content = await XDocument.LoadAsync(responseStream, LoadOptions.None, default);

        _testOutputHelper.WriteLine(content.ToString());

        var body = GetBodyElement(content);

        var wrapper = body.Element(XName.Get("CalculateAsyncResponse", "http://calculator.x-road.eu/"));
        wrapper.Should().NotBeNull();

        var result = wrapper!.Element(XName.Get("response"));
        result.Should().NotBeNull();
        result!.Value.Should().Be("1183");
    }

    private static XElement GetBodyElement(XContainer container)
    {
        var envelope = container.Element(XName.Get("Envelope", NamespaceConstants.SoapEnv));
        envelope.Should().NotBeNull();

        var body = envelope!.Element(XName.Get("Body", NamespaceConstants.SoapEnv));
        body.Should().NotBeNull();

        return body!;
    }

    private class Fault
    {
        public string Code { get; }
        public string Value { get; }

        private Fault(string code, string value)
        {
            Code = code;
            Value = value;
        }

        public static Fault FromXml(XContainer container)
        {
            var body = GetBodyElement(container);

            var fault = body.Element(XName.Get("Fault", NamespaceConstants.SoapEnv));
            fault.Should().NotBeNull();

            var faultcode = fault!.Element(XName.Get("faultcode"));
            faultcode.Should().NotBeNull();

            var faultstring = fault.Element(XName.Get("faultstring"));
            faultstring.Should().NotBeNull();

            return new Fault(faultcode!.Value, faultstring!.Value);
        }
    }
}

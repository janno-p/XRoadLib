using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using XRoadLib;
using XRoadLib.Serialization;
using Xunit;
using Xunit.Abstractions;

namespace Calculator.Tests
{
    public class IntegrationTests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly TestServer _server;

        public IntegrationTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            var webHostBuilder = new WebHostBuilder()
                .UseStartup<Startup>();

            _server = new TestServer(webHostBuilder);
        }

        [Fact]
        public async Task GetWsdl()
        {
            using var client = _server.CreateClient();
            using var response = await client.GetAsync("/");
            await using var responseStream = await response.Content.ReadAsStreamAsync();
            var content = await XDocument.LoadAsync(responseStream, LoadOptions.None, default);

            _testOutputHelper.WriteLine(content.ToString());
        }

        [Fact]
        public async Task MissingContentType()
        {
            using var client = _server.CreateClient();
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
            using var client = _server.CreateClient();

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
            using var client = _server.CreateClient();

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
            using var client = _server.CreateClient();

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
            using var client = _server.CreateClient();

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
            using var client = _server.CreateClient();

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
            Assert.NotNull(wrapper);

            var result = wrapper.Element(XName.Get("response"));
            Assert.NotNull(result);
            Assert.Equal("1183", result.Value);
        }

        private static XElement GetBodyElement(XContainer container)
        {
            var envelope = container.Element(XName.Get("Envelope", NamespaceConstants.SoapEnv));
            Assert.NotNull(envelope);

            var body = envelope.Element(XName.Get("Body", NamespaceConstants.SoapEnv));
            Assert.NotNull(body);

            return body;
        }

        private class Fault
        {
            public string Code { get; private init; }
            public string Value { get; private init; }

            public static Fault FromXml(XContainer container)
            {
                var body = GetBodyElement(container);

                var fault = body.Element(XName.Get("Fault", NamespaceConstants.SoapEnv));
                Assert.NotNull(fault);

                var faultcode = fault.Element(XName.Get("faultcode"));
                Assert.NotNull(faultcode);

                var faultstring = fault.Element(XName.Get("faultstring"));
                Assert.NotNull(faultstring);

                return new Fault { Code = faultcode.Value, Value = faultstring.Value };
            }
        }
    }
}
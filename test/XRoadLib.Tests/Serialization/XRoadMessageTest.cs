using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using XRoadLib.Serialization;
using XRoadLib.Soap;
using Xunit;

namespace XRoadLib.Tests.Serialization
{
    public class XRoadMessageTest
    {
        private static readonly IMessageFormatter MessageFormatter = new SoapMessageFormatter();

        private readonly IServiceManager[] _serviceManagers = { Globals.ServiceManager };

        [Fact]
        public async Task CanWriteEmptyContent()
        {
            var contentLength = await WriteContentAsync();
            Assert.Equal(0L, contentLength);
        }

        [Fact]
        public async Task CanWriteContentWithoutAttachments()
        {
            var contentLength = await WriteContentAsync("test");
            Assert.Equal(4L, contentLength);
        }

        [Fact]
        public async Task CanWriteContentWithAttachments()
        {
            using var x = new XRoadAttachment(new MemoryStream(new byte[] { 1, 2, 3, 4 }));
            using var y = new XRoadAttachment(new MemoryStream(new byte[] { 5, 6, 7, 8, 9 }));
            var contentLength = await WriteContentAsync("test2", new[] { x, y });
            Assert.Equal(562L, contentLength);
        }

        [Fact]
        public async Task CanReadEmptyContentWithoutAttachments()
        {
            using var stream = new MemoryStream();
            using var reader = new XRoadMessageReader(new DataReader(stream), MessageFormatter, "text/xml; charset=UTF-8", Globals.StoragePath, _serviceManagers);
            using var message = new XRoadMessage();
            await reader.ReadAsync(message, false);
            Assert.Equal(0L, message.ContentLength);
        }

        [Fact]
        public async Task CanReadContentWithoutAttachments()
        {
            using var stream = new MemoryStream();
            using var streamWriter = new StreamWriter(stream, XRoadEncoding.Utf8);

            await streamWriter.WriteAsync("<Envelope xmlns=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:id=\"http://x-road.eu/xsd/identifiers\" xmlns:repr=\"http://x-road.eu/xsd/representation.xsd\">\r\n");
            await streamWriter.WriteAsync("  <Header xmlns:xrd=\"http://x-road.ee/xsd/x-road.xsd\">\r\n");
            await streamWriter.WriteAsync("  </Header>\r\n");
            await streamWriter.WriteAsync("  <Body />\r\n");
            await streamWriter.WriteAsync("</Envelope>");
            await streamWriter.FlushAsync();

            stream.Position = 0L;

            using var reader = new XRoadMessageReader(new DataReader(stream), MessageFormatter, "text/xml; charset=UTF-8", Globals.StoragePath, _serviceManagers);
            using var message = new XRoadMessage();

            await reader.ReadAsync(message, false);
            Assert.Equal(251L, message.ContentLength);
        }

        [Fact]
        public async Task CanReadContentWithAttachments()
        {
            using var stream = new MemoryStream();
            using var streamWriter = new StreamWriter(stream, XRoadEncoding.Utf8);

            await streamWriter.WriteAsync("\r\n");
            await streamWriter.WriteAsync("--5e7a8827-5850-45be-9a1e-8bbf6aff5da7\r\n");
            await streamWriter.WriteAsync("Content-Type: text/xml; charset=UTF-8\r\n");
            await streamWriter.WriteAsync("Content-Transfer-Encoding: 8bit\r\n");
            await streamWriter.WriteAsync("Content-ID: <rQI0gpIFuQMxlrqBj3qHKw==>\r\n");
            await streamWriter.WriteAsync("\r\n");
            await streamWriter.WriteAsync("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n");
            await streamWriter.WriteAsync("<Envelope xmlns=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:id=\"http://x-road.eu/xsd/identifiers\" xmlns:repr=\"http://x-road.eu/xsd/representation.xsd\">\r\n");
            await streamWriter.WriteAsync("  <Header xmlns:xrd=\"http://x-road.ee/xsd/x-road.xsd\">\r\n");
            await streamWriter.WriteAsync("  </Header>\r\n");
            await streamWriter.WriteAsync("  <Body />\r\n");
            await streamWriter.WriteAsync("</Envelope>\r\n");
            await streamWriter.WriteAsync("\r\n");
            await streamWriter.WriteAsync("--5e7a8827-5850-45be-9a1e-8bbf6aff5da7\r\n");
            await streamWriter.WriteAsync("Content-Type: application/octet-stream\r\n");
            await streamWriter.WriteAsync("Content-Transfer-Encoding: binary\r\n");
            await streamWriter.WriteAsync("Content-ID: <CNbAWiFRKnmh3+udKo8mLw==>\r\n");
            await streamWriter.WriteAsync("\r\n");
            await streamWriter.WriteAsync("proovikas\r\n");
            await streamWriter.WriteAsync("--5e7a8827-5850-45be-9a1e-8bbf6aff5da7\r\n");
            await streamWriter.WriteAsync("Content-Type: application/octet-stream\r\n");
            await streamWriter.WriteAsync("Content-Transfer-Encoding: binary\r\n");
            await streamWriter.WriteAsync("Content-ID: <qrOlKraewrdRAu86cFnqwg==>\r\n");
            await streamWriter.WriteAsync("\r\n");
            await streamWriter.WriteAsync("testikas sisu\r\n");
            await streamWriter.WriteAsync("--5e7a8827-5850-45be-9a1e-8bbf6aff5da7--");
            await streamWriter.FlushAsync();

            const string contentTypeHeader = "multipart/related; type=\"application/xml+xop\"; start=\"rQI0gpIFuQMxlrqBj3qHKw==\"; boundary=\"5e7a8827-5850-45be-9a1e-8bbf6aff5da7\"";

            stream.Position = 0L;

            using var reader = new XRoadMessageReader(new DataReader(stream), MessageFormatter, contentTypeHeader, Globals.StoragePath, _serviceManagers);
            using var message = new XRoadMessage();

            await reader.ReadAsync(message, false);
            Assert.Equal(835L, message.ContentLength);
        }

        private static async Task<long> WriteContentAsync(string content = null, IEnumerable<XRoadAttachment> attachments = null)
        {
            var messageFormatter = new SoapMessageFormatter();

            using var outputStream = new MemoryStream();
            using var contentStream = new MemoryStream();

            if (content != null)
            {
                var buffer = XRoadEncoding.Utf8.GetBytes(content);
                contentStream.Write(buffer, 0, buffer.Length);
            }

            using var message = new XRoadMessage(contentStream);

            (attachments ?? Enumerable.Empty<XRoadAttachment>()).ToList().ForEach(message.AllAttachments.Add);
            await message.SaveToAsync(outputStream, x => {}, (k, v) => {}, messageFormatter);

            return message.ContentLength;
        }
    }
}
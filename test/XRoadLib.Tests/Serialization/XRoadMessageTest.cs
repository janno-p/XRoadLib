using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using XRoadLib.Serialization;
using Xunit;

namespace XRoadLib.Tests.Serialization
{
    public class XRoadMessageTest
    {
        private readonly IXRoadProtocol[] supportedProtocols =
        {
            Globals.XRoadProtocol20,
            Globals.XRoadProtocol31,
            Globals.XRoadProtocol40
        };

        [Fact]
        public void CanWriteEmptyContent()
        {
            var contentLength = WriteContent();
            Assert.Equal(0L, contentLength);
        }

        [Fact]
        public void CanWriteContentWithoutAttachments()
        {
            var contentLength = WriteContent("test");
            Assert.Equal(4L, contentLength);
        }

        [Fact]
        public void CanWriteContentWithAttachments()
        {
            using (var x = new XRoadAttachment(new MemoryStream(new byte[] { 1, 2, 3, 4 })))
            using (var y = new XRoadAttachment(new MemoryStream(new byte[] { 5, 6, 7, 8, 9 })))
            {
                var contentLength = WriteContent("test2", new[] { x, y });
                Assert.Equal(562L, contentLength);
            }
        }

        [Fact]
        public void CanReadEmptyContentWithoutAttachments()
        {
            using (var stream = new MemoryStream())
            using (var reader = new XRoadMessageReader(stream, "text/xml; charset=UTF-8", Path.GetTempPath(), supportedProtocols))
            using (var message = new XRoadMessage())
            {
                reader.Read(message, false);
                Assert.Equal(0L, message.ContentLength);
            }
        }

        [Fact]
        public void CanReadContentWithoutAttachments()
        {
            using (var stream = new MemoryStream())
            using (var streamWriter = new StreamWriter(stream, Encoding.UTF8))
            {
                streamWriter.Write("<Envelope xmlns=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:id=\"http://x-road.eu/xsd/identifiers\" xmlns:repr=\"http://x-road.eu/xsd/representation.xsd\">\r\n");
                streamWriter.Write("  <Header xmlns:xrd=\"http://x-road.ee/xsd/x-road.xsd\">\r\n");
                streamWriter.Write("  </Header>\r\n");
                streamWriter.Write("  <Body />\r\n");
                streamWriter.Write("</Envelope>");
                streamWriter.Flush();

                stream.Position = 0L;
                using (var reader = new XRoadMessageReader(stream, "text/xml; charset=UTF-8", Path.GetTempPath(), supportedProtocols))
                using (var message = new XRoadMessage())
                {
                    reader.Read(message, false);
                    Assert.Equal(254L, message.ContentLength);
                }
            }
        }

        [Fact]
        public void CanReadContentWithAttachments()
        {
            using (var stream = new MemoryStream())
            using (var streamWriter = new StreamWriter(stream, Encoding.UTF8))
            {
                streamWriter.Write("\r\n");
                streamWriter.Write("--5e7a8827-5850-45be-9a1e-8bbf6aff5da7\r\n");
                streamWriter.Write("Content-Type: text/xml; charset=UTF-8\r\n");
                streamWriter.Write("Content-Transfer-Encoding: 8bit\r\n");
                streamWriter.Write("Content-ID: <rQI0gpIFuQMxlrqBj3qHKw==>\r\n");
                streamWriter.Write("\r\n");
                streamWriter.Write("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n");
                streamWriter.Write("<Envelope xmlns=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:id=\"http://x-road.eu/xsd/identifiers\" xmlns:repr=\"http://x-road.eu/xsd/representation.xsd\">\r\n");
                streamWriter.Write("  <Header xmlns:xrd=\"http://x-road.ee/xsd/x-road.xsd\">\r\n");
                streamWriter.Write("  </Header>\r\n");
                streamWriter.Write("  <Body />\r\n");
                streamWriter.Write("</Envelope>\r\n");
                streamWriter.Write("\r\n");
                streamWriter.Write("--5e7a8827-5850-45be-9a1e-8bbf6aff5da7\r\n");
                streamWriter.Write("Content-Type: application/octet-stream\r\n");
                streamWriter.Write("Content-Transfer-Encoding: binary\r\n");
                streamWriter.Write("Content-ID: <CNbAWiFRKnmh3+udKo8mLw==>\r\n");
                streamWriter.Write("\r\n");
                streamWriter.Write("proovikas\r\n");
                streamWriter.Write("--5e7a8827-5850-45be-9a1e-8bbf6aff5da7\r\n");
                streamWriter.Write("Content-Type: application/octet-stream\r\n");
                streamWriter.Write("Content-Transfer-Encoding: binary\r\n");
                streamWriter.Write("Content-ID: <qrOlKraewrdRAu86cFnqwg==>\r\n");
                streamWriter.Write("\r\n");
                streamWriter.Write("testikas sisu\r\n");
                streamWriter.Write("--5e7a8827-5850-45be-9a1e-8bbf6aff5da7--");
                streamWriter.Flush();

                var contentTypeHeader = "multipart/related; type=\"application/xml+xop\"; start=\"rQI0gpIFuQMxlrqBj3qHKw==\"; boundary=\"5e7a8827-5850-45be-9a1e-8bbf6aff5da7\"";

                stream.Position = 0L;
                using (var reader = new XRoadMessageReader(stream, contentTypeHeader, Path.GetTempPath(), supportedProtocols))
                using (var message = new XRoadMessage())
                {
                    reader.Read(message, false);
                    Assert.Equal(838L, message.ContentLength);
                }
            }
        }

        private static long WriteContent(string content = null, IEnumerable<XRoadAttachment> attachments = null)
        {
            using (var outputStream = new MemoryStream())
            using (var contentStream = new MemoryStream())
            {
                if (content != null)
                {
                    var buffer = Encoding.UTF8.GetBytes(content);
                    contentStream.Write(buffer, 0, buffer.Length);
                }

                using (var message = new XRoadMessage(contentStream))
                {
                    (attachments ?? Enumerable.Empty<XRoadAttachment>()).ToList().ForEach(message.AllAttachments.Add);
                    message.SaveTo(outputStream, x => {}, (k, v) => {});

                    return message.ContentLength;
                }
            }
        }
    }
}
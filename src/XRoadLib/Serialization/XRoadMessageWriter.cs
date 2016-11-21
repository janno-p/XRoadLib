using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using XRoadLib.Schema;

namespace XRoadLib.Serialization
{
    internal class XRoadMessageWriter : IDisposable
    {
        public const string NEW_LINE = "\r\n";

        private readonly CountingStream outputStream;

        private TextWriter writer;

        public XRoadMessageWriter(Stream outputStream)
        {
            this.outputStream = new CountingStream(outputStream);
            writer = new StreamWriter(this.outputStream);
        }

        public void Write(XRoadMessage source, Action<string> setContentType, Action<string, string> appendHeader)
        {
            source.ContentStream.Position = 0;

            if (!source.MultipartContentAttachments.Any())
            {
                WriteContent(source);
                writer.Flush();
                source.ContentLength = outputStream.WriteCount;
                return;
            }

            var boundaryMarker = Guid.NewGuid().ToString();

            var contentID = Convert.ToBase64String(MD5.Create().ComputeHash(source.ContentStream));

            var contentTypeType = XRoadMessage.MULTIPART_CONTENT_TYPE_SOAP;
            if (source.BinaryMode == BinaryMode.Xml)
                contentTypeType = XRoadMessage.MULTIPART_CONTENT_TYPE_XOP;

            setContentType(string.Format("multipart/related; type=\"{2}\"; start=\"{0}\"; boundary=\"{1}\"", contentID, boundaryMarker, contentTypeType));
            appendHeader("MIME-Version", "1.0");

            source.ContentStream.Position = 0;
            SerializeMessage(source, contentID, boundaryMarker);
            writer.Flush();

            foreach (var attachment in source.MultipartContentAttachments)
            {
                if (source.BinaryMode == BinaryMode.Xml)
                    SerializeXopAttachment(attachment, boundaryMarker);
                else SerializeAttachment(attachment, boundaryMarker);
            }

            writer.Write(NEW_LINE);
            writer.Write("--{0}--", boundaryMarker);
            writer.Write(NEW_LINE);
            writer.Flush();

            source.ContentLength = outputStream.WriteCount;
        }

        public void Dispose()
        {
            writer.Dispose();
            writer = null;
        }

        private void WriteContent(XRoadMessage source)
        {
            writer.Write(new StreamReader(source.ContentStream).ReadToEnd());
        }

        private void SerializeMessage(XRoadMessage source, string contentID, string boundaryMarker)
        {
            writer.Write(NEW_LINE);
            writer.Write("--{0}", boundaryMarker);
            writer.Write(NEW_LINE);
            writer.Write("Content-Type: text/xml; charset=UTF-8");
            writer.Write(NEW_LINE);
            writer.Write("Content-Transfer-Encoding: 8bit");
            writer.Write(NEW_LINE);
            writer.Write("Content-ID: <{0}>", contentID.Trim('<', '>', ' '));
            writer.Write(NEW_LINE);
            writer.Write(NEW_LINE);
            WriteContent(source);
            writer.Write(NEW_LINE);
        }

        private void SerializeAttachment(XRoadAttachment attachment, string boundaryMarker)
        {
            writer.Write(NEW_LINE);
            writer.Write("--{0}", boundaryMarker);
            writer.Write(NEW_LINE);
            writer.Write("Content-Disposition: attachment; filename=notAnswering");
            writer.Write(NEW_LINE);
            writer.Write("Content-Type: application/octet-stream");
            writer.Write(NEW_LINE);
            writer.Write("Content-Transfer-Encoding: base64");
            writer.Write(NEW_LINE);
            writer.Write("Content-ID: <{0}>", attachment.ContentID.Trim('<', '>', ' '));
            writer.Write(NEW_LINE);
            writer.Write(NEW_LINE);
            attachment.WriteAsBase64(writer);
        }

        private void SerializeXopAttachment(XRoadAttachment attachment, string boundaryMarker)
        {
            writer.Write(NEW_LINE);
            writer.Write("--{0}", boundaryMarker);
            writer.Write(NEW_LINE);
            writer.Write("Content-Type: application/octet-stream");
            writer.Write(NEW_LINE);
            writer.Write("Content-Transfer-Encoding: binary");
            writer.Write(NEW_LINE);
            writer.Write("Content-ID: <{0}>", attachment.ContentID.Trim('<', '>', ' '));
            writer.Write(NEW_LINE);
            writer.Write(NEW_LINE);
            writer.Flush();

            attachment.ContentStream.Position = 0;
            attachment.ContentStream.CopyTo(outputStream);
            outputStream.Flush();
        }
    }
}
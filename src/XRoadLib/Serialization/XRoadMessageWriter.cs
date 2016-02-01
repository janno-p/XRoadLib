using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using XRoadLib.Schema;

namespace XRoadLib.Serialization
{
    internal class XRoadMessageWriter : IDisposable
    {
        private readonly Stream outputStream;

        private TextWriter writer;

        public XRoadMessageWriter(TextWriter writer, Stream outputStream)
        {
            this.outputStream = outputStream;
            this.writer = writer;
        }

        public void Write(XRoadMessage source, Action<string> setContentType, Action<string, string> appendHeader)
        {
            source.ContentStream.Position = 0;

            if (!source.MultipartContentAttachments.Any())
            {
                WriteContent(source);
                return;
            }

            var boundaryMarker = Guid.NewGuid().ToString();
            var contentID = Convert.ToBase64String(new MD5CryptoServiceProvider().ComputeHash(source.ContentStream));

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

            writer.WriteLine();
            writer.WriteLine("--{0}--", boundaryMarker);
        }

        public void Dispose()
        {
            writer.Close();
            writer = null;
        }

        private void WriteContent(XRoadMessage source)
        {
            writer.WriteLine(new StreamReader(source.ContentStream).ReadToEnd());
        }

        private void SerializeMessage(XRoadMessage source, string contentID, string boundaryMarker)
        {
            writer.WriteLine();
            writer.WriteLine("--{0}", boundaryMarker);
            writer.WriteLine("Content-Type: text/xml; charset=UTF-8");
            writer.WriteLine("Content-Transfer-Encoding: 8bit");
            writer.WriteLine("Content-ID: <{0}>", contentID.Trim('<', '>', ' '));
            writer.WriteLine();
            WriteContent(source);
        }

        private void SerializeAttachment(XRoadAttachment attachment, string boundaryMarker)
        {
            writer.WriteLine();
            writer.WriteLine("--{0}", boundaryMarker);
            writer.WriteLine("Content-Disposition: attachment; filename=notAnswering");
            writer.WriteLine("Content-Type: application/octet-stream");
            writer.WriteLine("Content-Transfer-Encoding: base64");
            writer.WriteLine("Content-ID: <{0}>", attachment.ContentID.Trim('<', '>', ' '));
            writer.WriteLine();
            writer.WriteLine(attachment.ToBase64String());
        }

        private void SerializeXopAttachment(XRoadAttachment attachment, string boundaryMarker)
        {
            writer.WriteLine();
            writer.WriteLine("--{0}", boundaryMarker);
            writer.WriteLine("Content-Type: application/octet-stream");
            writer.WriteLine("Content-Transfer-Encoding: binary");
            writer.WriteLine("Content-ID: <{0}>", attachment.ContentID.Trim('<', '>', ' '));
            writer.WriteLine();

            attachment.ContentStream.Position = 0;
            attachment.ContentStream.CopyTo(outputStream);
            outputStream.Flush();

            writer.WriteLine(attachment.ToBase64String());
            writer.Flush();
        }
    }
}
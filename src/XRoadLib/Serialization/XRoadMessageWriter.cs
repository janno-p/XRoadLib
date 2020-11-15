using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using XRoadLib.Schema;
using XRoadLib.Soap;

namespace XRoadLib.Serialization
{
    public class XRoadMessageWriter : IDisposable
    {
        public const string NewLine = "\r\n";

        private readonly CountingStream _outputStream;

        private TextWriter _writer;

        public XRoadMessageWriter(Stream outputStream)
        {
            _outputStream = new CountingStream(outputStream);
            _writer = new StreamWriter(_outputStream);
        }

        public void Write(XRoadMessage source, Action<string> setContentType, Action<string, string> appendHeader, IMessageFormatter messageFormatter)
        {
            source.ContentStream.Position = 0;

            if (!source.MultipartContentAttachments.Any())
            {
                WriteContent(source);
                _writer.Flush();
                source.ContentLength = _outputStream.WriteCount;
                return;
            }

            var boundaryMarker = Guid.NewGuid().ToString();

            var contentId = Convert.ToBase64String(MD5.Create().ComputeHash(source.ContentStream));

            var contentTypeType = messageFormatter.ContentType;
            var startInfo = string.Empty;
            if (source.BinaryMode == BinaryMode.Xml)
            {
                contentTypeType = ContentTypes.Xop;
                startInfo = $@"start-info=""{messageFormatter.ContentType}""; ";
            }

            setContentType($@"{ContentTypes.Multipart}; type=""{contentTypeType}""; start=""{contentId}""; {startInfo}boundary=""{boundaryMarker}""");
            appendHeader("MIME-Version", "1.0");

            source.ContentStream.Position = 0;
            SerializeMessage(source, contentId, boundaryMarker, messageFormatter);
            _writer.Flush();

            foreach (var attachment in source.MultipartContentAttachments)
            {
                if (source.BinaryMode == BinaryMode.Xml)
                    SerializeXopAttachment(attachment, boundaryMarker);
                else SerializeAttachment(attachment, boundaryMarker);
            }

            _writer.Write(NewLine);
            _writer.Write("--{0}--", boundaryMarker);
            _writer.Write(NewLine);
            _writer.Flush();

            source.ContentLength = _outputStream.WriteCount;
        }

        public void Dispose()
        {
            _writer.Dispose();
            _writer = null;
        }

        private void WriteContent(XRoadMessage source)
        {
            _writer.Write(new StreamReader(source.ContentStream).ReadToEnd());
        }

        private void SerializeMessage(XRoadMessage source, string contentId, string boundaryMarker, IMessageFormatter messageFormatter)
        {
            _writer.Write(NewLine);
            _writer.Write("--{0}", boundaryMarker);
            _writer.Write(NewLine);
            _writer.Write(
                source.BinaryMode == BinaryMode.Attachment
                    ? $"Content-Type: {messageFormatter.ContentType}; charset=UTF-8"
                    : $"Content-Type: {ContentTypes.Xop}; charset=UTF-8; type=\"{messageFormatter.ContentType}\""
            );
            _writer.Write(NewLine);
            _writer.Write("Content-Transfer-Encoding: 8bit");
            _writer.Write(NewLine);
            _writer.Write("Content-ID: <{0}>", contentId.Trim('<', '>', ' '));
            _writer.Write(NewLine);
            _writer.Write(NewLine);
            WriteContent(source);
            _writer.Write(NewLine);
        }

        private void SerializeAttachment(XRoadAttachment attachment, string boundaryMarker)
        {
            _writer.Write(NewLine);
            _writer.Write("--{0}", boundaryMarker);
            _writer.Write(NewLine);
            _writer.Write("Content-Disposition: attachment; filename=notAnswering");
            _writer.Write(NewLine);
            _writer.Write("Content-Type: application/octet-stream");
            _writer.Write(NewLine);
            _writer.Write("Content-Transfer-Encoding: base64");
            _writer.Write(NewLine);
            _writer.Write("Content-ID: <{0}>", attachment.ContentId.Trim('<', '>', ' '));
            _writer.Write(NewLine);
            _writer.Write(NewLine);
            attachment.WriteAsBase64(_writer);
        }

        private void SerializeXopAttachment(XRoadAttachment attachment, string boundaryMarker)
        {
            _writer.Write(NewLine);
            _writer.Write("--{0}", boundaryMarker);
            _writer.Write(NewLine);
            _writer.Write("Content-Type: application/octet-stream");
            _writer.Write(NewLine);
            _writer.Write("Content-Transfer-Encoding: binary");
            _writer.Write(NewLine);
            _writer.Write("Content-ID: <{0}>", attachment.ContentId.Trim('<', '>', ' '));
            _writer.Write(NewLine);
            _writer.Write(NewLine);
            _writer.Flush();

            attachment.ContentStream.Position = 0;
            attachment.ContentStream.CopyTo(_outputStream);
            _outputStream.Flush();
        }
    }
}
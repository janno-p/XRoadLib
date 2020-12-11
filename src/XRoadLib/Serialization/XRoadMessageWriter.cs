using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using XRoadLib.Schema;
using XRoadLib.Soap;

namespace XRoadLib.Serialization
{
    public class XRoadMessageWriter : IDisposable
    {
        private const string NewLine = "\r\n";
        private const int Base64LineLength = 76;

        private StreamCounter _streamCounter;

        public XRoadMessageWriter(Stream outputStream)
        {
            _streamCounter = new StreamCounter(outputStream);
        }

        public async Task WriteAsync(XRoadMessage source, Action<string> setContentType, Action<string, string> appendHeader, IMessageFormatter messageFormatter)
        {
            source.ContentStream.Position = 0;

            if (!source.MultipartContentAttachments.Any())
            {
                await WriteContentAsync(source).ConfigureAwait(false);
                await _streamCounter.FlushAsync().ConfigureAwait(false);
                source.ContentLength = _streamCounter.WriteCount;
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
            await SerializeMessageAsync(source, contentId, boundaryMarker, messageFormatter).ConfigureAwait(false);
            await _streamCounter.FlushAsync().ConfigureAwait(false);

            foreach (var attachment in source.MultipartContentAttachments)
            {
                if (source.BinaryMode == BinaryMode.Xml)
                    await SerializeXopAttachmentAsync(attachment, boundaryMarker).ConfigureAwait(false);
                else await SerializeAttachmentAsync(attachment, boundaryMarker).ConfigureAwait(false);
            }

            var endMarker = XRoadEncoding.Utf8.GetBytes($"{NewLine}--{boundaryMarker}--{NewLine}");
            await _streamCounter.WriteAsync(endMarker, 0, endMarker.Length).ConfigureAwait(false);

            await _streamCounter.FlushAsync().ConfigureAwait(false);

            source.ContentLength = _streamCounter.WriteCount;
        }

        public void Dispose()
        {
            _streamCounter.Dispose();
            _streamCounter = null;
        }

        private Task WriteContentAsync(XRoadMessage source) =>
            _streamCounter.WriteAsync(source.ContentStream);

        private async Task SerializeMessageAsync(XRoadMessage source, string contentId, string boundaryMarker, IMessageFormatter messageFormatter)
        {
            var headers = new StringBuilder()
                .Append(NewLine)
                .Append($"--{boundaryMarker}")
                .Append(NewLine)
                .Append(source.BinaryMode == BinaryMode.Attachment
                    ? $"Content-Type: {messageFormatter.ContentType}; charset=UTF-8"
                    : $"Content-Type: {ContentTypes.Xop}; charset=UTF-8; type=\"{messageFormatter.ContentType}\"")
                .Append(NewLine)
                .Append("Content-Transfer-Encoding: 8bit")
                .Append(NewLine)
                .Append($"Content-ID: <{contentId.Trim('<', '>', ' ')}>")
                .Append(NewLine)
                .Append(NewLine)
                .ToString();

            var headerBytes = XRoadEncoding.Utf8.GetBytes(headers);
            await _streamCounter.WriteAsync(headerBytes, 0, headerBytes.Length).ConfigureAwait(false);

            await WriteContentAsync(source).ConfigureAwait(false);

            var newLineBytes = XRoadEncoding.Utf8.GetBytes(NewLine);

            await _streamCounter.WriteAsync(newLineBytes, 0, newLineBytes.Length).ConfigureAwait(false);
        }

        private async Task SerializeAttachmentAsync(XRoadAttachment attachment, string boundaryMarker)
        {
            var headers = new StringBuilder()
                .Append(NewLine)
                .Append($"--{boundaryMarker}")
                .Append(NewLine)
                .Append("Content-Disposition: attachment; filename=notAnswering")
                .Append(NewLine)
                .Append("Content-Type: application/octet-stream")
                .Append(NewLine)
                .Append("Content-Transfer-Encoding: base64")
                .Append(NewLine)
                .Append($"Content-ID: <{attachment.ContentId.Trim('<', '>', ' ')}>")
                .Append(NewLine)
                .Append(NewLine)
                .ToString();

            var headerBytes = XRoadEncoding.Utf8.GetBytes(headers);
            await _streamCounter.WriteAsync(headerBytes, 0, headerBytes.Length).ConfigureAwait(false);

            await WriteAttachmentAsBase64Async(attachment).ConfigureAwait(false);
        }

        private async Task SerializeXopAttachmentAsync(XRoadAttachment attachment, string boundaryMarker)
        {
            var headers = new StringBuilder()
                .Append(NewLine)
                .Append($"--{boundaryMarker}")
                .Append(NewLine)
                .Append("Content-Type: application/octet-stream")
                .Append(NewLine)
                .Append("Content-Transfer-Encoding: binary")
                .Append(NewLine)
                .Append($"Content-ID: <{attachment.ContentId.Trim('<', '>', ' ')}>")
                .Append(NewLine)
                .Append(NewLine)
                .ToString();

            var headerBytes = XRoadEncoding.Utf8.GetBytes(headers);
            await _streamCounter.WriteAsync(headerBytes, 0, headerBytes.Length).ConfigureAwait(false);
            await _streamCounter.FlushAsync().ConfigureAwait(false);

            attachment.ContentStream.Position = 0;
            await _streamCounter.WriteAsync(attachment.ContentStream).ConfigureAwait(false);

            await _streamCounter.FlushAsync().ConfigureAwait(false);
        }

        internal async Task WriteAttachmentAsBase64Async(XRoadAttachment attachment)
        {
            const int bufferSize = Base64LineLength / 4 * 3;
            var buffer = new byte[bufferSize];

            attachment.ContentStream.Position = 0;

            var noContent = true;

            int bytesRead;
            while ((bytesRead = await attachment.ContentStream.ReadAsync(buffer, 0, bufferSize).ConfigureAwait(false)) > 0)
            {
                noContent = false;

                var row = new StringBuilder(Convert.ToBase64String(buffer, 0, bytesRead)).Append(NewLine).ToString();
                var rowBytes = XRoadEncoding.Utf8.GetBytes(row);

                await _streamCounter.WriteAsync(rowBytes, 0, rowBytes.Length).ConfigureAwait(false);
            }

            if (noContent)
            {
                var newLineBytes = XRoadEncoding.Utf8.GetBytes(NewLine);
                await _streamCounter.WriteAsync(newLineBytes, 0, newLineBytes.Length).ConfigureAwait(false);
            }
        }
    }
}
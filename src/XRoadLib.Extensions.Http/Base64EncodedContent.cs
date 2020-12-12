using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace XRoadLib.Extensions.Http
{
    public class Base64EncodedContent : HttpContent
    {
        private const string NewLine = "\r\n";
        private const int Base64LineLength = 76;
        private const int NewLineLength = 2;
        private const int BufferSize = Base64LineLength / 4 * 3;

        private readonly Stream _contentStream;
        private readonly long _start;

        public Base64EncodedContent(Stream contentStream)
        {
            _contentStream = contentStream;

            if (_contentStream.CanSeek)
                _start = _contentStream.Position;

            Headers.Add("Content-Transfer-Encoding", "base64");
        }

        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            var buffer = new byte[BufferSize];

            if (_contentStream.CanSeek)
                _contentStream.Position = _start;

            var noContent = true;

            int bytesRead;
            while ((bytesRead = await _contentStream.ReadAsync(buffer, 0, BufferSize).ConfigureAwait(false)) > 0)
            {
                noContent = false;

                var row = new StringBuilder(Convert.ToBase64String(buffer, 0, bytesRead)).Append(NewLine).ToString();
                var rowBytes = Encoding.ASCII.GetBytes(row);

                await stream.WriteAsync(rowBytes, 0, rowBytes.Length).ConfigureAwait(false);
            }

            if (noContent)
            {
                var newLineBytes = Encoding.ASCII.GetBytes(NewLine);
                await stream.WriteAsync(newLineBytes, 0, newLineBytes.Length).ConfigureAwait(false);
            }
        }

        protected override bool TryComputeLength(out long length)
        {
            length = 0;

            if (!_contentStream.CanSeek)
                return false;

            var numBytes = _contentStream.Length - _start;
            var numRows = numBytes / BufferSize;

            length = numRows * (Base64LineLength + NewLineLength) + ((4 * (numBytes % BufferSize ) / 3 + 3) & ~3) + NewLineLength;

            return true;

        }
    }
}
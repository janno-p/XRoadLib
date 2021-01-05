using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using XRoadLib.Extensions;
using XRoadLib.Headers;
using XRoadLib.Schema;
using XRoadLib.Soap;

namespace XRoadLib.Serialization
{
    internal class XRoadMessageReader : IDisposable
    {
        internal enum ChunkStop
        {
            BufferLimit,
            NewLine,
            EndOfStream
        }

        private const int BufferSize = 1024;

        private static readonly byte[] NewLine = { (byte)'\r', (byte)'\n' };

        private readonly ICollection<IServiceManager> _serviceManagers;
        private readonly IMessageFormatter _messageFormatter;
        private readonly string _storagePath;
        private readonly string _contentTypeHeader;

        private DataReader _dataReader;

        public XRoadMessageReader(DataReader dataReader, IMessageFormatter messageFormatter, string contentTypeHeader, string storagePath, IEnumerable<IServiceManager> serviceManagers)
        {
            _contentTypeHeader = contentTypeHeader;
            _dataReader = dataReader;
            _storagePath = storagePath;
            _serviceManagers = serviceManagers.ToList();
            _messageFormatter = messageFormatter;
        }

        public async Task ReadAsync(XRoadMessage target, bool isResponse = false)
        {
            target.IsMultipartContainer = XRoadHelper.IsMultipartMsg(_contentTypeHeader);
            if (target.IsMultipartContainer)
                target.MultipartContentType = XRoadHelper.GetMultipartContentType(_contentTypeHeader);

            if (!_dataReader.Reset())
                return;

            target.ContentEncoding = GetContentEncoding();
            target.ContentStream = new MemoryStream();

            await ReadMessagePartsAsync(target).ConfigureAwait(false);

            target.ContentStream.Position = 0;

            using (var reader = XmlReader.Create(target.ContentStream, new XmlReaderSettings { Async = true, CloseInput = false }))
            {
                var serviceManager = await DetectServiceManagerAsync(reader, target).ConfigureAwait(false);
                await ParseXRoadHeaderAsync(target, reader, serviceManager).ConfigureAwait(false);

                target.RootElementName = await ParseMessageRootElementNameAsync(reader).ConfigureAwait(false);

                if (target.ServiceManager == null && target.RootElementName != null)
                    target.ServiceManager = _serviceManagers.SingleOrDefault(p => p.IsHeaderNamespace(target.RootElementName.NamespaceName));
            }

            if (target.Header is IXRoadHeader xrh && xrh.ProtocolVersion?.Trim() != "4.0")
                throw new InvalidQueryException($"Unsupported X-Road v6 protocol version value `{xrh.ProtocolVersion ?? string.Empty}`.");

            if (target.IsMultipartContainer)
                target.BinaryMode = BinaryMode.Attachment;

            if (ContentTypes.Xop.Equals(target.MultipartContentType))
                target.BinaryMode = BinaryMode.Xml;

            if (isResponse)
                return;

            var serviceCode = (target.Header as IXRoadHeader)?.Service?.ServiceCode;

            if (target.RootElementName == null || string.IsNullOrWhiteSpace(serviceCode))
                return;

            if (!Equals(target.RootElementName.LocalName, serviceCode))
                throw new InvalidQueryException($"X-Road operation name `{serviceCode}` does not match request wrapper element name `{target.RootElementName}`.");
        }

        public void Dispose()
        {
            _dataReader.Dispose();
            _dataReader = null;
        }

        private async Task ReadMessagePartsAsync(XRoadMessage target)
        {
            if (!target.IsMultipartContainer)
            {
                await ReadNextPartAsync(target.ContentStream, GetByteDecoder(null), target.ContentEncoding, null).ConfigureAwait(false);
                target.ContentLength = _dataReader.Position;
                return;
            }

            var multipartStartContentId = GetMultipartStartContentId();
            var multipartBoundary = GetMultipartBoundary();
            var multipartBoundaryMarker = target.ContentEncoding.GetBytes("--" + multipartBoundary);
            var multipartEndMarker = target.ContentEncoding.GetBytes("--" + multipartBoundary + "--");

            byte[] lastLine = null;

            do
            {
                if (!BufferStartsWith(lastLine, multipartBoundaryMarker))
                {
                    lastLine = await ReadLineAsync().ConfigureAwait(false);
                    continue;
                }

                var (partId, partTransferEncoding) = await ExtractMultipartHeaderAsync(target.ContentEncoding).ConfigureAwait(false);

                var targetStream = target.ContentStream;
                if (targetStream.Length > 0 || !string.IsNullOrEmpty(multipartStartContentId) && !multipartStartContentId.Contains(partId))
                {
                    var attachment = new XRoadAttachment(partId, Path.Combine(_storagePath, Path.GetRandomFileName()));
                    target.AllAttachments.Add(attachment);
                    targetStream = attachment.ContentStream;
                }

                lastLine = await ReadNextPartAsync(targetStream, GetByteDecoder(partTransferEncoding), target.ContentEncoding, multipartBoundaryMarker).ConfigureAwait(false);
            } while (await _dataReader.PeekByteAsync().ConfigureAwait(false) != -1 && !BufferStartsWith(lastLine, multipartEndMarker));

            target.ContentLength = _dataReader.Position;
        }

        private async Task<byte[]> ReadNextPartAsync(Stream targetStream, Func<byte[], Encoding, byte[]> decoder, Encoding useEncoding, IList<byte> boundaryMarker)
        {
            var addNewLine = false;

            while (true)
            {
                var (chunkStop, buffer) = await ReadChunkOrLineAsync(BufferSize).ConfigureAwait(false);

                if (boundaryMarker != null && BufferStartsWith(buffer, boundaryMarker))
                    return buffer;

                if (boundaryMarker != null && chunkStop == ChunkStop.EndOfStream)
                    throw new InvalidQueryException("Unexpected end of MIME multipart message: the end marker of message part was not found in incoming request.");

                if (decoder != null)
                    buffer = decoder(buffer, useEncoding);

                if (decoder == null && addNewLine)
                    await targetStream.WriteAsync(NewLine, 0, NewLine.Length).ConfigureAwait(false);

                await targetStream.WriteAsync(buffer, 0, buffer.Length).ConfigureAwait(false);

                if (chunkStop == ChunkStop.EndOfStream)
                    return buffer;

                addNewLine = chunkStop == ChunkStop.NewLine;
            }
        }

        private async Task<(string, string)> ExtractMultipartHeaderAsync(Encoding contentEncoding)
        {
            string partId = null;
            string partTransferEncoding = null;

            while (true)
            {
                var buffer = await ReadLineAsync().ConfigureAwait(false);

                var lastLine = contentEncoding.GetString(buffer).Trim();
                if (string.IsNullOrEmpty(lastLine))
                    break;

                var tempContentId = XRoadHelper.ExtractValue("content-id:", lastLine, null);
                if (tempContentId != null)
                    partId = tempContentId.Trim().Trim('<', '>');

                var tempTransferEncoding = XRoadHelper.ExtractValue("content-transfer-encoding:", lastLine, null);
                if (tempTransferEncoding != null)
                    partTransferEncoding = tempTransferEncoding;
            }

            return (partId, partTransferEncoding);
        }

        private async Task<byte[]> ReadLineAsync()
        {
            var chunk = new byte[0];

            while (true)
            {
                var (chunkStop, buffer) = await ReadChunkOrLineAsync(BufferSize).ConfigureAwait(false);

                Array.Resize(ref chunk, chunk.Length + buffer.Length);
                Array.Copy(buffer, chunk, buffer.Length);

                if (chunkStop == ChunkStop.EndOfStream || chunkStop == ChunkStop.NewLine)
                    break;
            }

            return chunk;
        }

        internal async Task<(ChunkStop, byte[])> ReadChunkOrLineAsync(int chunkSize)
        {
            var result = ChunkStop.BufferLimit;
            var buffer = new byte[chunkSize];

            var curPos = -1;
            while (curPos < chunkSize - 1)
            {
                var lastByte = await _dataReader.ReadByteAsync().ConfigureAwait(false);
                if (lastByte == -1)
                {
                    result = ChunkStop.EndOfStream;
                    break;
                }

                if (lastByte == '\r' && await _dataReader.PeekByteAsync().ConfigureAwait(false) == '\n')
                {
                    await _dataReader.ReadByteAsync().ConfigureAwait(false);
                    result = ChunkStop.NewLine;
                    break;
                }

                buffer[++curPos] = (byte)lastByte;
            }

            var chunk = new byte[curPos + 1];
            Array.Copy(buffer, chunk, curPos + 1);

            return (result, chunk);
        }

        private Encoding GetContentEncoding()
        {
            var contentType = XRoadHelper.ExtractValue("charset=", _contentTypeHeader, ";")?.Trim().Trim('"');
            return string.IsNullOrWhiteSpace(contentType) || contentType.ToUpper().Equals("UTF-8") ? XRoadEncoding.Utf8 : Encoding.GetEncoding(contentType);
        }

        private string GetMultipartStartContentId()
        {
            return XRoadHelper.ExtractValue("start=", _contentTypeHeader, ";")?.Trim().Trim('"');
        }

        private string GetMultipartBoundary()
        {
            return XRoadHelper.ExtractValue("boundary=", _contentTypeHeader, ";")?.Trim().Trim('"');
        }

        private static Func<byte[], Encoding, byte[]> GetByteDecoder(string contentTransferEncoding)
        {
            if (string.IsNullOrEmpty(contentTransferEncoding))
                return null;

            switch (contentTransferEncoding.ToLower())
            {
                case "quoted-printable":
                case "7bit":
                case "8bit":
                case "binary":
                    return null;

                case "base64":
                    return DecodeFromBase64;

                default:
                    throw new UnsupportedContentTransferEncodingException(contentTransferEncoding);
            }
        }

        private static byte[] DecodeFromBase64(byte[] buffer, Encoding encoding)
        {
            if (buffer == null || buffer.Length == 0)
                return new byte[0];

            var encodedNewLine = encoding.GetBytes("\r\n");
            if (BufferEndsWith(buffer, encodedNewLine, null))
                Array.Resize(ref buffer, buffer.Length - encodedNewLine.Length);

            return buffer.Length > 0 ? Convert.FromBase64CharArray(encoding.GetChars(buffer), 0, encoding.GetCharCount(buffer)) : new byte[0];
        }

        private static bool BufferStartsWith(IList<byte> arr1, IList<byte> arr2)
        {
            if (arr1 == null || arr2 == null || arr1.Count < arr2.Count)
                return false;

            var curPos = 0;
            while (curPos < arr2.Count && arr1[curPos] == arr2[curPos])
                curPos += 1;

            return curPos == arr2.Count;
        }

        private static bool BufferEndsWith(IList<byte> arr1, IList<byte> arr2, int? checkPos)
        {
            if (arr1 == null || arr2 == null)
                return false;

            var posArr1 = checkPos ?? arr1.Count - 1;
            var posArr2 = arr2.Count - 1;

            while (posArr2 > -1 && posArr1 >= posArr2 && arr1[posArr1] == arr2[posArr2])
            {
                posArr1 -= 1;
                posArr2 -= 1;
            }

            return posArr2 == -1;
        }

        private async Task<IServiceManager> DetectServiceManagerAsync(XmlReader reader, XRoadMessage target)
        {
            await _messageFormatter.MoveToEnvelopeAsync(reader).ConfigureAwait(false);

            if (target.ServiceManager != null)
                return target.ServiceManager;

            foreach (var serviceManager in _serviceManagers)
                if (await serviceManager.IsDefinedByEnvelopeAsync(reader).ConfigureAwait(false))
                    return serviceManager;

            return null;
        }

        private async Task ParseXRoadHeaderAsync(XRoadMessage target, XmlReader reader, IServiceManager serviceManager)
        {
            if (!await _messageFormatter.TryMoveToHeaderAsync(reader).ConfigureAwait(false))
                return;

            var header = serviceManager?.CreateHeader();
            var xRoadHeader = header as IXRoadHeader;

            var unresolved = new List<XElement>();

            while (await reader.MoveToElementAsync(2).ConfigureAwait(false))
            {
                if (serviceManager == null)
                {
                    serviceManager = _serviceManagers.SingleOrDefault(p => p.IsHeaderNamespace(reader.NamespaceURI));
                    header = serviceManager?.CreateHeader();
                    xRoadHeader = header as IXRoadHeader;
                }

                if (serviceManager == null || xRoadHeader == null || !serviceManager.IsHeaderNamespace(reader.NamespaceURI))
                {
                    unresolved.Add((XElement)XNode.ReadFrom(reader));
                    continue;
                }

                await xRoadHeader.ReadHeaderValueAsync(reader).ConfigureAwait(false);
            }

            xRoadHeader?.Validate();

            target.Header = header;
            target.UnresolvedHeaders = unresolved;
            target.ServiceManager = serviceManager;
        }

        private async Task<XName> ParseMessageRootElementNameAsync(XmlReader reader)
        {
            return await _messageFormatter.TryMoveToBodyAsync(reader).ConfigureAwait(false) && await reader.MoveToElementAsync(2).ConfigureAwait(false)
                ? reader.GetXName()
                : null;
        }
    }
}
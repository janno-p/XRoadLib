using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using XRoadLib.Extensions;
using XRoadLib.Headers;
using XRoadLib.Schema;
using XRoadLib.Serialization.Mapping;

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

        private const int BUFFER_SIZE = 1024;

        private static readonly byte[] newLine = { (byte)'\r', (byte)'\n' };

        private readonly ICollection<IXRoadProtocol> supportedProtocols;
        private readonly string storagePath;
        private readonly string contentTypeHeader;

        private Stream stream;
        private int? peekedByte;
        private long streamPosition;

        public XRoadMessageReader(Stream stream, string contentTypeHeader, string storagePath, IEnumerable<IXRoadProtocol> supportedProtocols)
        {
            this.contentTypeHeader = contentTypeHeader;
            this.storagePath = storagePath;
            this.stream = stream;
            this.supportedProtocols = new List<IXRoadProtocol>(supportedProtocols);
        }

        public void Read(XRoadMessage target, bool isResponse = false)
        {
            if (stream.CanSeek && stream.Length == 0)
                return;

            if (stream.CanSeek)
                stream.Position = 0;

            streamPosition = 0;

            target.ContentEncoding = GetContentEncoding();
            target.ContentStream = new MemoryStream();

            target.IsMultipartContainer = ReadMessageParts(target);

            target.ContentStream.Position = 0;

            using (var reader = XmlReader.Create(target.ContentStream, new XmlReaderSettings { CloseInput = false }))
            {
                var protocol = ParseXRoadProtocol(reader);
                ParseXRoadHeader(target, reader, protocol);

                target.RootElementName = ParseMessageRootElementName(reader);

                if (target.Protocol == null && target.RootElementName != null)
                    target.Protocol = supportedProtocols.SingleOrDefault(p => p.IsHeaderNamespace(target.RootElementName.NamespaceName));

                target.MetaServiceMap = GetMetaServiceMap(target.RootElementName);
            }

            var xrh4 = target.Header as IXRoadHeader40;
            if (xrh4 != null && xrh4.ProtocolVersion?.Trim() != "4.0")
                throw XRoadException.InvalidQuery($"Unsupported X-Road v6 protocol version value `{xrh4.ProtocolVersion ?? string.Empty}`.");

            if (target.IsMultipartContainer)
                target.BinaryMode = BinaryMode.Attachment;

            if (XRoadMessage.MULTIPART_CONTENT_TYPE_XOP.Equals(target.MultipartContentType))
                target.BinaryMode = BinaryMode.Xml;

            if (isResponse)
                return;

            if (target.RootElementName == null || string.IsNullOrWhiteSpace(target.Header?.Service?.ServiceCode))
                return;

            if (!Equals(target.RootElementName.LocalName, target.Header.Service.ServiceCode))
                throw XRoadException.InvalidQuery($"Teenuse nimi `{target.Header.Service.ServiceCode}` ei ole vastavuses päringu sisuga `{target.RootElementName}`.");
        }

        public void Dispose()
        {
            stream.Dispose();
            stream = null;
        }

        private bool ReadMessageParts(XRoadMessage target)
        {
            if (!IsMultipartMsg(contentTypeHeader))
            {
                ReadNextPart(target.ContentStream, GetByteDecoder(null), target.ContentEncoding, null);
                target.ContentLength = streamPosition;
                return false;
            }

            target.MultipartContentType = GetMultipartContentType();
            var multipartStartContentID = GetMultipartStartContentID();
            var multipartBoundary = GetMultipartBoundary();
            var multipartBoundaryMarker = target.ContentEncoding.GetBytes("--" + multipartBoundary);
            var multipartEndMarker = target.ContentEncoding.GetBytes("--" + multipartBoundary + "--");

            byte[] lastLine = null;

            do
            {
                if (!BufferStartsWith(lastLine, multipartBoundaryMarker))
                {
                    lastLine = ReadLine();
                    continue;
                }

                string partID, partTransferEncoding;
                ExtractMultipartHeader(target.ContentEncoding, out partID, out partTransferEncoding);

                var targetStream = target.ContentStream;
                if (targetStream.Length > 0 || (!string.IsNullOrEmpty(multipartStartContentID) && !multipartStartContentID.Contains(partID)))
                {
                    var attachment = new XRoadAttachment(partID, Path.Combine(storagePath, Path.GetRandomFileName()));
                    target.AllAttachments.Add(attachment);
                    targetStream = attachment.ContentStream;
                }

                lastLine = ReadNextPart(targetStream, GetByteDecoder(partTransferEncoding), target.ContentEncoding, multipartBoundaryMarker);
            } while (PeekByte() != -1 && !BufferStartsWith(lastLine, multipartEndMarker));

            target.ContentLength = streamPosition;

            return true;
        }

        private byte[] ReadNextPart(Stream targetStream, Func<byte[], Encoding, byte[]> decoder, Encoding useEncoding, byte[] boundaryMarker)
        {
            var addNewLine = false;

            while (true)
            {
                byte[] buffer;
                var chunkStop = ReadChunkOrLine(out buffer, BUFFER_SIZE);

                if (boundaryMarker != null && BufferStartsWith(buffer, boundaryMarker))
                    return buffer;

                if (boundaryMarker != null && chunkStop == ChunkStop.EndOfStream)
                    throw XRoadException.MultipartManusegaSõnumiOotamatuLõpp();

                if (decoder != null)
                    buffer = decoder(buffer, useEncoding);

                if (decoder == null && addNewLine)
                    targetStream.Write(newLine, 0, newLine.Length);

                targetStream.Write(buffer, 0, buffer.Length);

                if (chunkStop == ChunkStop.EndOfStream)
                    return buffer;

                addNewLine = chunkStop == ChunkStop.NewLine;
            }
        }

        private void ExtractMultipartHeader(Encoding contentEncoding, out string partID, out string partTransferEncoding)
        {
            partID = partTransferEncoding = null;

            while (true)
            {
                var buffer = ReadLine();

                var lastLine = contentEncoding.GetString(buffer).Trim();
                if (string.IsNullOrEmpty(lastLine))
                    break;

                var tempContentID = ExtractValue("content-id:", lastLine, null);
                if (tempContentID != null)
                    partID = tempContentID.Trim().Trim('<', '>');

                var tempTransferEncoding = ExtractValue("content-transfer-encoding:", lastLine, null);
                if (tempTransferEncoding != null)
                    partTransferEncoding = tempTransferEncoding;
            }
        }

        private byte[] ReadLine()
        {
            var chunk = new byte[0];

            while (true)
            {
                byte[] buffer;
                var chunkStop = ReadChunkOrLine(out buffer, BUFFER_SIZE);

                Array.Resize(ref chunk, chunk.Length + buffer.Length);
                Array.Copy(buffer, chunk, buffer.Length);

                if (chunkStop == ChunkStop.EndOfStream || chunkStop == ChunkStop.NewLine)
                    break;
            }

            return chunk;
        }

        internal ChunkStop ReadChunkOrLine(out byte[] chunk, int chunkSize)
        {
            var result = ChunkStop.BufferLimit;
            var buffer = new byte[chunkSize];

            var curPos = -1;
            while (curPos < chunkSize - 1)
            {
                var lastByte = ReadByte();
                if (lastByte == -1)
                {
                    result = ChunkStop.EndOfStream;
                    break;
                }

                if (lastByte == '\r' && PeekByte() == '\n')
                {
                    ReadByte();
                    result = ChunkStop.NewLine;
                    break;
                }

                buffer[++curPos] = (byte)lastByte;
            }

            chunk = new byte[curPos + 1];
            Array.Copy(buffer, chunk, curPos + 1);

            return result;
        }

        private int ReadByte()
        {
            if (peekedByte == null)
            {
                var @byte = stream.ReadByte();
                if (@byte >= 0)
                    streamPosition++;
                return @byte;
            }

            var result = peekedByte.Value;
            if (result >= 0)
                streamPosition++;

            peekedByte = null;
            return result;
        }

        private int PeekByte()
        {
            peekedByte = peekedByte ?? stream.ReadByte();
            return peekedByte.Value;
        }

        private Encoding GetContentEncoding()
        {
            var contentType = ExtractValue("charset=", contentTypeHeader, ";")?.Trim().Trim('"');
            return string.IsNullOrWhiteSpace(contentType) || contentType.ToUpper().Equals("UTF-8") ? XRoadEncoding.UTF8 : Encoding.GetEncoding(contentType);
        }

        private string GetMultipartContentType()
        {
            return ExtractValue("type=", contentTypeHeader, ";")?.Trim().Trim('"');
        }

        private string GetMultipartStartContentID()
        {
            return ExtractValue("start=", contentTypeHeader, ";")?.Trim().Trim('"');
        }

        private string GetMultipartBoundary()
        {
            return ExtractValue("boundary=", contentTypeHeader, ";")?.Trim().Trim('"');
        }

        private static bool IsMultipartMsg(string contentType)
        {
            return contentType.ToLower().Contains("multipart/related");
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
                    throw XRoadException.ToetamataKodeering(contentTransferEncoding);
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

            return (posArr2 == -1);
        }

        private static string ExtractValue(string key, string keyValuePair, string separator)
        {
            if (string.IsNullOrEmpty(keyValuePair))
                return null;

            // Mis positsioonilt küsitud key üldse hakkab ..
            var indxOfKey = keyValuePair.ToLower().IndexOf(key.ToLower(), StringComparison.Ordinal);
            if (indxOfKey < 0)
                return null;

            var fromIndx = indxOfKey + key.Length;
            var toIndx = keyValuePair.Length;
            if (separator != null && keyValuePair.IndexOf(separator, fromIndx, StringComparison.Ordinal) > -1)
                toIndx = keyValuePair.IndexOf(separator, fromIndx, StringComparison.Ordinal);

            return keyValuePair.Substring(fromIndx, toIndx - fromIndx).Trim();
        }

        private static string NormalizeCharset(string charset)
        {
            if (charset == null)
                return null;

            charset = charset.ToLower().Replace("cp", "");

            if (charset.StartsWith("125"))
                charset = "windows-" + charset;

            return charset;
        }

        private IXRoadProtocol ParseXRoadProtocol(XmlReader reader)
        {
            if (!reader.MoveToElement(0, "Envelope", NamespaceConstants.SOAP_ENV))
                throw XRoadException.InvalidQuery("Päringus puudub SOAP-ENV:Envelope element.");

            return supportedProtocols.SingleOrDefault(p => p.IsDefinedByEnvelope(reader));
        }

        private void ParseXRoadHeader(XRoadMessage target, XmlReader reader, IXRoadProtocol protocol)
        {
            if (!reader.MoveToElement(1) || !reader.IsCurrentElement(1, "Header", NamespaceConstants.SOAP_ENV))
                return;

            var header = protocol?.HeaderDefinition.CreateInstance();
            var unresolved = new List<XElement>();

            while (reader.MoveToElement(2))
            {
                if (protocol == null)
                {
                    protocol = supportedProtocols.SingleOrDefault(p => p.IsHeaderNamespace(reader.NamespaceURI));
                    header = protocol?.HeaderDefinition.CreateInstance();
                }

                if (protocol == null || header == null || !protocol.IsHeaderNamespace(reader.NamespaceURI))
                {
                    unresolved.Add((XElement)XNode.ReadFrom(reader));
                    continue;
                }

                header.ReadHeaderValue(reader);
            }

            header?.Validate();

            target.Header = header;
            target.UnresolvedHeaders = unresolved;
            target.Protocol = protocol;
        }

        private static IServiceMap GetMetaServiceMap(XName rootElementName)
        {
            if (rootElementName == null || !NamespaceConstants.MetaServiceNamespaces.Contains(rootElementName.NamespaceName))
                return null;

            switch (rootElementName.LocalName)
            {
                case "listMethods":
                    return new ListMethodsServiceMap(rootElementName);

                case "testSystem":
                    return new TestSystemServiceMap(rootElementName);

                default:
                    return null;
            }
        }

        private static XName ParseMessageRootElementName(XmlReader reader)
        {
            return (reader.IsCurrentElement(1, "Body", NamespaceConstants.SOAP_ENV) || reader.MoveToElement(1, "Body", NamespaceConstants.SOAP_ENV)) && reader.MoveToElement(2)
                ? reader.GetXName()
                : null;
        }
    }
}
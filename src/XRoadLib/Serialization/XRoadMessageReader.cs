using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using XRoadLib.Header;
using XRoadLib.Extensions;

namespace XRoadLib.Serialization
{
    internal class XRoadMessageReader : IDisposable
    {
        private enum ChunkStop
        {
            BufferLimit,
            NewLine,
            EndOfStream
        }

        private const int BUFFER_SIZE = 1024;

        private static readonly byte[] newLine = { (byte)'\r', (byte)'\n' };

        private readonly NameValueCollection headers;
        private readonly Encoding contentEncoding;
        private readonly string storagePath;

        private Stream stream;
        private string contentType;
        private int? peekedByte;

        private long StreamPosition => stream.Position - (peekedByte.HasValue ? 1 : 0);

        public XRoadMessageReader(Stream stream, NameValueCollection headers, Encoding contentEncoding, string storagePath)
        {
            this.contentEncoding = contentEncoding;
            this.headers = headers;
            this.storagePath = storagePath;
            this.stream = stream;
        }

        public void Read(XRoadMessage target, bool isResponse = false)
        {
            if (stream.Length == 0)
                return;

            stream.Position = 0;

            target.ContentEncoding = contentEncoding;
            target.ContentStream = new MemoryStream();

            ReadMessageParts(target);

            target.ContentStream.Position = 0;

            using (var reader = XmlReader.Create(target.ContentStream, new XmlReaderSettings { CloseInput = false }))
            {
                var protocol = ParseXRoadProtocol(reader);

                target.Header = ParseXRoadHeader(reader, ref protocol);
                target.Protocol = protocol.GetValueOrDefault(XRoadProtocol.Version31);
                target.RootElementName = ParseMessageRootElementName(reader);
            }

            var teenuseNimi = !string.IsNullOrWhiteSpace(target.Header?.Nimi?.Method)
                ? target.Header.Nimi.Method
                : (target.RootElementName != null ? target.RootElementName.Name : "");

            if (target.Protocol != XRoadProtocol.Version20 && target.IsMultipart && !target.MultipartContentType.Equals("application/xop+xml"))
                throw XRoadException.ViganePäring("Teenuse `{0}` multipart päringu sisuks oodati `application/xop+xml`, kuid edastati `{1}`.", teenuseNimi, target.MultipartContentType);

            if (isResponse)
                return;

            if (target.RootElementName == null || string.IsNullOrWhiteSpace(target.Header?.Nimi?.Method))
                return;

            if (!Equals(target.RootElementName.Name, target.Header.Nimi.Method))
                throw XRoadException.ViganePäring("Teenuse nimi `{0}` ei ole vastavuses päringu sisuga `{1}`.", target.Header.Nimi.Method, target.RootElementName);
        }

        public void Dispose()
        {
            stream.Dispose();
            stream = null;
        }

        private string GetContentType()
        {
            var contentTypeKey = headers.Keys
                                        .Cast<string>()
                                        .FirstOrDefault(key => key.Trim()
                                                                  .ToLower()
                                                                  .Equals("content-type"));

            return contentTypeKey == null ? "text/xml; charset=UTF-8" : headers[contentTypeKey];
        }

        private void ReadMessageParts(XRoadMessage target)
        {
            contentType = GetContentType();

            target.IsMultipart = IsMultipartMsg(contentType);
            if (!target.IsMultipart)
            {
                ReadNextPart(target.ContentStream, GetByteDecoder(null), contentEncoding, null);
                return;
            }

            target.MultipartContentType = GetMultipartContentType(contentType);
            var multipartStartContentID = GetMultipartStartContentID(contentType);
            var multipartBoundary = GetMultipartBoundary(contentType);
            var multipartBoundaryMarker = contentEncoding.GetBytes("--" + multipartBoundary);
            var multipartEndMarker = contentEncoding.GetBytes("--" + multipartBoundary + "--");

            byte[] lastLine = null;

            do
            {
                if (!BufferStartsWith(lastLine, multipartBoundaryMarker))
                {
                    lastLine = ReadLine();
                    continue;
                }

                string partID, partCharset, partTransferEncoding;
                ExtractMultipartHeader(out partID, out partCharset, out partTransferEncoding);

                var targetStream = target.ContentStream;
                if (targetStream.Length > 0 || (!string.IsNullOrEmpty(multipartStartContentID) && !multipartStartContentID.Contains(partID)))
                {
                    var attachment = new XRoadAttachment(partID, Path.Combine(storagePath, Path.GetRandomFileName()));
                    target.AllAttachments.Add(attachment);
                    targetStream = attachment.ContentStream;
                }

                lastLine = ReadNextPart(targetStream, GetByteDecoder(partTransferEncoding), contentEncoding, multipartBoundaryMarker);
            } while (StreamPosition < stream.Length && !BufferStartsWith(lastLine, multipartEndMarker));
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

        private void ExtractMultipartHeader(out string partID, out string partCharset, out string partTransferEncoding)
        {
            partID = partCharset = partTransferEncoding = null;

            while (true)
            {
                var buffer = ReadLine();

                var lastLine = contentEncoding.GetString(buffer).Trim();
                if (string.IsNullOrEmpty(lastLine))
                    break;

                var tempContentType = ExtractValue("content-type:", lastLine, null);
                if (tempContentType != null)
                    partCharset = NormalizeCharset(ExtractValue("chartset=", tempContentType, ";"));

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

        private ChunkStop ReadChunkOrLine(out byte[] chunk, int chunkSize)
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

            stream.Flush();

            return result;
        }

        private int ReadByte()
        {
            if (peekedByte == null)
                return stream.ReadByte();

            var result = peekedByte.Value;
            peekedByte = null;
            return result;
        }

        private int PeekByte()
        {
            peekedByte = peekedByte ?? stream.ReadByte();
            return peekedByte.Value;
        }

        private static bool IsMultipartMsg(string contentType)
        {
            return contentType.ToLower().Contains("multipart/related");
        }

        private static string GetMultipartBoundary(string contentType)
        {
            var value = ExtractValue("boundary=", contentType, ";");
            return value?.Replace("\"", "");
        }

        private static string GetMultipartStartContentID(string contentType)
        {
            var value = ExtractValue("start=", contentType, ";");
            return value?.Replace("\"", "");
        }

        private static string GetMultipartContentType(string contentType)
        {
            var value = ExtractValue("type=", contentType, ";");
            return value?.Replace("\"", "");
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

        private static XRoadProtocol? ParseXRoadProtocol(XmlReader reader)
        {
            if (!reader.MoveToElement(0, "Envelope", NamespaceHelper.SOAP_ENV))
                throw XRoadException.ViganePäring("Päringus puudub SOAP-ENV:Envelope element.");

            return reader.GetAttribute("encodingStyle", NamespaceHelper.SOAP_ENV) != null ? XRoadProtocol.Version20 : (XRoadProtocol?)null;
        }

        private static IXRoadHeader ParseXRoadHeader(XmlReader reader, ref XRoadProtocol? protocol)
        {
            if (!reader.MoveToElement(1) || !reader.IsCurrentElement(1, "Header", NamespaceHelper.SOAP_ENV))
                return null;

            var header = new XRoadHeader();

            while (reader.MoveToElement(2))
                SetHeader(reader, header, ref protocol);

            return header;
        }

        private static XmlQualifiedName ParseMessageRootElementName(XmlReader reader)
        {
            return (reader.IsCurrentElement(1, "Body", NamespaceHelper.SOAP_ENV) || reader.MoveToElement(1, "Body", NamespaceHelper.SOAP_ENV)) && reader.MoveToElement(2)
                ? new XmlQualifiedName(reader.LocalName, reader.NamespaceURI)
                : null;
        }

        private static void SetHeader(XmlReader reader, IXRoadHeader header, ref XRoadProtocol? protocol)
        {
            if (protocol == null)
                protocol = (reader.NamespaceURI == NamespaceHelper.XTEE ? XRoadProtocol.Version20 : (XRoadProtocol?)null)
                           ?? (reader.NamespaceURI == NamespaceHelper.XROAD ? XRoadProtocol.Version31 : (XRoadProtocol?)null);

            if (protocol == null || reader.NamespaceURI != NamespaceHelper.GetXRoadNamespace(protocol.Value))
            {
                header.Unresolved.Add(new XmlQualifiedName(reader.LocalName, reader.NamespaceURI), reader.ReadInnerXml());
                return;
            }

            if (protocol.Value == XRoadProtocol.Version20)
                switch (reader.LocalName)
                {
                    case "autentija": header.Autentija = reader.ReadInnerXml(); return;
                    case "ametniknimi": header.AmetnikNimi = reader.ReadInnerXml(); return;
                    case "amet": header.Amet = reader.ReadInnerXml(); return;
                    case "allasutus": header.Allasutus = reader.ReadInnerXml(); return;
                    case "toimik": header.Toimik = reader.ReadInnerXml(); return;
                    case "nimi": header.Nimi = new XRoadServiceName(reader.ReadInnerXml()); return;
                    case "id": header.Id = reader.ReadInnerXml(); return;
                    case "isikukood": header.Isikukood = reader.ReadInnerXml(); return;
                    case "andmekogu": header.Andmekogu = reader.ReadInnerXml(); return;
                    case "asutus": header.Asutus = reader.ReadInnerXml(); return;
                    case "ametnik": header.Ametnik = reader.ReadInnerXml(); return;
                }

            if (protocol.Value == XRoadProtocol.Version31)
                switch (reader.LocalName)
                {
                    case "authenticator": header.Autentija = reader.ReadInnerXml(); return;
                    case "userName": header.AmetnikNimi = reader.ReadInnerXml(); return;
                    case "position": header.Amet = reader.ReadInnerXml(); return;
                    case "unit": header.Allasutus = reader.ReadInnerXml(); return;
                    case "issue": header.Toimik = reader.ReadInnerXml(); return;
                    case "service": header.Nimi = new XRoadServiceName(reader.ReadInnerXml()); return;
                    case "id": header.Id = reader.ReadInnerXml(); return;
                    case "userId": header.Isikukood = reader.ReadInnerXml(); return;
                    case "producer": header.Andmekogu = reader.ReadInnerXml(); return;
                    case "consumer": header.Asutus = reader.ReadInnerXml(); return;
                }

            throw XRoadException.ViganePäring("Tundmatu X-tee päise element {0}.", new XmlQualifiedName(reader.LocalName, reader.NamespaceURI));
        }
    }
}
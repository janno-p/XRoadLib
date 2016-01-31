using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Xml.Linq;
using XRoadLib.Protocols;
using XRoadLib.Protocols.Headers;
using XRoadLib.Schema;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization
{
    public class XRoadMessage : IAttachmentManager
    {
        public const string MULTIPART_CONTENT_TYPE_SOAP = "text/xml";
        public const string MULTIPART_CONTENT_TYPE_XOP = "application/xop+xml";

        private readonly List<XRoadAttachment> attachments = new List<XRoadAttachment>();

        public bool EnableFiltering { get; set; }
        public IXmlTemplate XmlTemplate { get; set; }
        public string MultipartContentType { get; set; }
        public Encoding ContentEncoding { get; set; }
        public Stream ContentStream { get; set; }
        public IProtocol Protocol { get; set; }
        public IXRoadHeader Header { get; set; }
        public IList<XElement> UnresolvedHeaders { get; set; }
        public XName RootElementName { get; set; }
        public BinaryMode BinaryContentMode { get; set; }

        public IList<XRoadAttachment> AllAttachments => attachments;
        public IEnumerable<XRoadAttachment> MultipartContentAttachments { get { return attachments.Where(x => x.IsMultipartContent); } }
        public uint Version => Header == null || Header.Service == null || !Header.Service.Version.HasValue ? 1u : Header.Service.Version.Value;

        public XRoadMessage()
        {
            ContentEncoding = Encoding.UTF8;
        }

        public XRoadMessage(IProtocol protocol, IXRoadHeader header)
            : this(new MemoryStream())
        {
            Protocol = protocol;
            Header = header;
        }

        public XRoadMessage(Stream contentStream)
        {
            ContentStream = contentStream;
        }

        public XRoadAttachment GetAttachment(string contentID)
        {
            return attachments.FirstOrDefault(attachment => attachment.ContentID.Contains(contentID));
        }

        public void LoadRequest(HttpContext httpContext, string storagePath, IEnumerable<IProtocol> supportedProtocols)
        {
            LoadRequest(httpContext.Request.InputStream, httpContext.Request.Headers, httpContext.Request.ContentEncoding, storagePath, supportedProtocols);
        }

        public void LoadRequest(Stream stream, NameValueCollection headers, Encoding contentEncoding, string storagePath, IEnumerable<IProtocol> supportedProtocols)
        {
            using (var reader = new XRoadMessageReader(stream, headers, contentEncoding, storagePath, supportedProtocols))
                reader.Read(this);
        }

        public void LoadResponse(Stream stream, NameValueCollection headers, Encoding contentEncoding, string storagePath, IEnumerable<IProtocol> supportedProtocols)
        {
            using (var reader = new XRoadMessageReader(stream, headers, contentEncoding, storagePath, supportedProtocols))
                reader.Read(this, true);
        }

        public void SaveTo(HttpContext httpContext)
        {
            using (var writer = new XRoadMessageWriter(httpContext.Response.Output, httpContext.Response.OutputStream))
                writer.Write(this, contentType => httpContext.Response.ContentType = contentType, httpContext.Response.AppendHeader);
        }

        public void SaveTo(WebRequest webRequest)
        {
            using (var outputStream = webRequest.GetRequestStream())
            using (var textWriter = new StreamWriter(outputStream))
            using (var writer = new XRoadMessageWriter(textWriter, outputStream))
                writer.Write(this, contentType => webRequest.ContentType = contentType, webRequest.Headers.Add);
        }

        public void SaveTo(TextWriter textWriter, Stream outputStream, Action<string> setContentType, Action<string, string> appendHeader)
        {
            using (var writer = new XRoadMessageWriter(textWriter, outputStream))
                writer.Write(this, setContentType, appendHeader);
        }

        public void Dispose()
        {
            if (ContentStream != null)
            {
                ContentStream.Dispose();
                ContentStream = null;
            }

            foreach (var attachment in attachments)
                attachment.Dispose();

            attachments.Clear();
        }

        public MetaServiceName GetMetaServiceName()
        {
            if (Header?.Service?.ServiceCode != null)
                return Header.Service.ServiceCode == "getState" ? MetaServiceName.GetState : MetaServiceName.None;

            var legacyProtocol = Protocol as ILegacyProtocol;
            if (legacyProtocol == null || RootElementName.Namespace != legacyProtocol.XRoadNamespace)
                return MetaServiceName.Unsupported;

            switch (RootElementName.LocalName)
            {
                case "listMethods":
                    return MetaServiceName.ListMethods;
                case "testSystem":
                    return MetaServiceName.TestSystem;
                default:
                    return MetaServiceName.Unsupported;
            }
        }

        public void Copy(XRoadMessage message)
        {
            Protocol = message.Protocol;
            Header = message.Header;
        }

        public ISerializerCache GetSerializerCache()
        {
            return Protocol?.GetSerializerCache(Version);
        }

        public IXmlTemplateNode GetTemplateNode(string nodeName)
        {
            return XmlTemplate != null ? XmlTemplate.GetParameterNode(nodeName) : XRoadXmlTemplate.EmptyNode;
        }
    }
}
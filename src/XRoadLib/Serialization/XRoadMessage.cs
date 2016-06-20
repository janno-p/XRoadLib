using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;
using XRoadLib.Protocols;
using XRoadLib.Protocols.Headers;
using XRoadLib.Schema;
using XRoadLib.Serialization.Mapping;
using XRoadLib.Serialization.Template;

#if !NETSTANDARD1_5
using System.Web;
using WebHeaderCollection = System.Collections.Specialized.NameValueCollection;
#else
using WebHeaderCollection = System.Net.WebHeaderCollection;
#endif

namespace XRoadLib.Serialization
{
    public class XRoadMessage : IAttachmentManager
    {
        public const string MULTIPART_CONTENT_TYPE_SOAP = "text/xml";
        public const string MULTIPART_CONTENT_TYPE_XOP = "application/xop+xml";

        private readonly List<XRoadAttachment> attachments = new List<XRoadAttachment>();

        public bool EnableFiltering { get; private set; }
        public string FilterName { get; private set; }

        public IXmlTemplate XmlTemplate { get; set; }
        public string MultipartContentType { get; set; }
        public Encoding ContentEncoding { get; set; } = XRoadEncoding.UTF8;
        public Stream ContentStream { get; set; }
        public XRoadProtocol Protocol { get; set; }
        public IXRoadHeader Header { get; set; }
        public IList<XElement> UnresolvedHeaders { get; set; }
        public XName RootElementName { get; set; }
        public BinaryMode BinaryMode { get; set; }
        public bool IsMultipartContainer { get; set; }
        public IServiceMap MetaServiceMap { get; set; }
        public long ContentLength { get; internal set; }

        public IList<XRoadAttachment> AllAttachments => attachments;
        public IEnumerable<XRoadAttachment> MultipartContentAttachments { get { return attachments.Where(x => x.IsMultipartContent); } }
        public uint Version => Header == null || Header.Service == null || !Header.Service.Version.HasValue ? 1u : Header.Service.Version.Value;
        public IXmlTemplateNode RequestNode => XmlTemplate != null ? XmlTemplate.RequestNode : XRoadXmlTemplate.EmptyNode;
        public IXmlTemplateNode ResponseNode => XmlTemplate != null ? XmlTemplate.ResponseNode : XRoadXmlTemplate.EmptyNode;

        public XRoadMessage()
        { }

        public XRoadMessage(XRoadProtocol protocol, IXRoadHeader header)
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

#if !NETSTANDARD1_5
        public void LoadRequest(HttpContext httpContext, string storagePath, IEnumerable<XRoadProtocol> supportedProtocols)
        {
            LoadRequest(httpContext.Request.InputStream, httpContext.Request.Headers, httpContext.Request.ContentEncoding, storagePath, supportedProtocols);
        }
#endif

        public void LoadRequest(Stream stream, WebHeaderCollection headers, Encoding contentEncoding, string storagePath, IEnumerable<XRoadProtocol> supportedProtocols)
        {
            using (var reader = new XRoadMessageReader(stream, headers, contentEncoding, storagePath, supportedProtocols))
                reader.Read(this);
        }

        public void LoadResponse(Stream stream, WebHeaderCollection headers, Encoding contentEncoding, string storagePath, IEnumerable<XRoadProtocol> supportedProtocols)
        {
            using (var reader = new XRoadMessageReader(stream, headers, contentEncoding, storagePath, supportedProtocols))
                reader.Read(this, true);
        }

#if !NETSTANDARD1_5
        public void SaveTo(HttpContext httpContext)
        {
            using (var writer = new XRoadMessageWriter(httpContext.Response.OutputStream))
                writer.Write(this, contentType => httpContext.Response.ContentType = contentType, httpContext.Response.AppendHeader);
        }
#endif

        public void SaveTo(WebRequest webRequest)
        {
            using (var outputStream = webRequest.GetRequestStreamAsync().Result)
            using (var writer = new XRoadMessageWriter(outputStream))
                writer.Write(this, contentType => webRequest.ContentType = contentType, (k, v) => webRequest.Headers[k] = v);
        }

        public void SaveTo(Stream outputStream, Action<string> setContentType, Action<string, string> appendHeader)
        {
            using (var writer = new XRoadMessageWriter(outputStream))
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

        public void Copy(XRoadMessage message)
        {
            Protocol = message.Protocol;
            Header = message.Header;
        }

        public ISerializerCache GetSerializerCache()
        {
            return Protocol?.GetSerializerCache(Version);
        }

        public void EnableFilter(string filterName)
        {
            EnableFiltering = true;
            FilterName = filterName;
        }
    }
}
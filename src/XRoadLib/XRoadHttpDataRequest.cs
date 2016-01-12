using System;
using System.IO;
using System.Web;
using System.Xml;
using XRoadLib.Events;
using XRoadLib.Extensions;
using XRoadLib.Serialization;

namespace XRoadLib
{
    public delegate void MessageEventHandler(object sender, EventArgs e);

    public class XRoadHttpDataRequest : IDisposable
    {
        private const string RESPONSE_CONTENT_TYPE = "text/xml; charset=utf-8";

        private readonly HttpContext httpContext;
        private readonly ISerializerCache serializerCache;

        private XRoadMessage requestMessage;
        private XRoadMessage responseMessage;

        public event ExceptionOccuredEventHandler ExceptionOccured;
        public event MessageEventHandler RequestLoaded;

        public string StoragePath { get; set; }

        public XRoadHttpDataRequest(HttpContext httpContext, ISerializerCache serializerCache)
        {
            this.httpContext = httpContext;
            this.serializerCache = serializerCache;

            requestMessage = new XRoadMessage();
            responseMessage = new XRoadMessage(new MemoryStream());
        }

        public void Process()
        {
            httpContext.Request.InputStream.Position = 0;
            httpContext.Response.ContentType = RESPONSE_CONTENT_TYPE;

            try
            {
                ProcessRequest();
            }
            catch (Exception exception)
            {
                var e = new ExceptionOccuredEventArgs(exception);
                ExceptionOccured?.Invoke(this, e);

                using (var writer = new XmlTextWriter(httpContext.Response.OutputStream, httpContext.Response.ContentEncoding))
                    SoapMessageHelper.SerializeSoapFaultResponse(writer, e.Code, e.Message, e.Actor, e.Detail, exception);
            }
        }

        private void ProcessRequest()
        {
            if (httpContext.Request.InputStream.Length == 0)
                throw XRoadException.InvalidQuery("Empty request content");

            requestMessage.LoadRequest(httpContext, StoragePath.GetValueOrDefault(Path.GetTempPath()));
            responseMessage.Copy(requestMessage);

            if (!serializerCache.IsSupportedProtocol(requestMessage.Protocol))
                throw XRoadException.InvalidQuery("Unsupported protocol version `{0}`.", requestMessage.Protocol);

            RequestLoaded?.Invoke(this, new MessageEventArgs(requestMessage));
        }

        public void Dispose()
        {
            requestMessage?.Dispose();
            requestMessage = null;

            responseMessage?.Dispose();
            responseMessage = null;
        }
    }
}
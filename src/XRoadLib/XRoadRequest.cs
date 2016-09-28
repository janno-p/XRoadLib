using System;
using System.IO;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using XRoadLib.Events;
using XRoadLib.Extensions;
using XRoadLib.Headers;
using XRoadLib.Serialization;
using XRoadLib.Serialization.Mapping;

namespace XRoadLib
{
    /// <summary>
    /// X-Road request object which wraps single X-Road web request.
    /// </summary>
    public interface IXRoadRequest
    {
        /// <summary>
        /// Provides access to underlying protocol instance.
        /// </summary>
        IXRoadProtocol Protocol { get; }

        /// <summary>
        /// Provides access to underlying request uri.
        /// </summary>
        Uri Uri { get; }

        /// <summary>
        /// Provides access to underlying request namespace.
        /// </summary>
        string RequestNamespace { get; }

        /// <summary>
        /// Event is called before invoking WebRequest which allows to
        /// customize request options.
        /// </summary>
        event EventHandler<XRoadRequestEventArgs> BeforeRequest;

        /// <summary>
        /// Event is called before deserializing WebResponse content which allows to
        /// customize response options.
        /// </summary>
        event EventHandler<XRoadResponseEventArgs> BeforeDeserialize;

        /// <summary>
        /// Executes specified X-Road operations with given arguments.
        /// </summary>
        TResult Execute<TResult>(object arg, IXRoadHeader xRoadHeader, IServiceMap serviceMap = null);
    }

    /// <summary>
    /// X-Road request object which wraps single X-Road web request.
    /// </summary>
    public class XRoadRequest : IXRoadRequest
    {
        /// <summary>
        /// Provides access to underlying protocol instance.
        /// </summary>
        public IXRoadProtocol Protocol { get; }

        /// <summary>
        /// Provides access to underlying request uri.
        /// </summary>
        public Uri Uri { get; }

        /// <summary>
        /// Provides access to underlying request namespace.
        /// </summary>
        public string RequestNamespace { get; }

        /// <summary>
        /// Event is called before invoking WebRequest which allows to
        /// customize request options.
        /// </summary>
        public event EventHandler<XRoadRequestEventArgs> BeforeRequest;

        /// <summary>
        /// Event is called before deserializing WebResponse content which allows to
        /// customize response options.
        /// </summary>
        public event EventHandler<XRoadResponseEventArgs> BeforeDeserialize;

        /// <summary>
        /// Initialize new request object.
        /// <param name="uri">Network location of the adapter or X-Road server.</param>
        /// <param name="protocol">X-Road message protocol version.</param>
        /// <param name="requestNamespace">Overrides default producer namespace for operation payload element.</param>
        /// </summary>
        public XRoadRequest(Uri uri, IXRoadProtocol protocol, string requestNamespace = null)
        {
            Protocol = protocol;
            Uri = uri;
            RequestNamespace = requestNamespace;
        }

        /// <summary>
        /// Executes specified X-Road operations with given arguments.
        /// </summary>
        public T Execute<T>(object arg, IXRoadHeader xRoadHeader, IServiceMap serviceMap = null)
        {
            IServiceMap operationServiceMap = null;

            using (var requestMessage = new XRoadMessage(Protocol, xRoadHeader))
            {
                using (var writer = XmlWriter.Create(requestMessage.ContentStream))
                {
                    writer.WriteStartDocument();

                    Protocol.Style.WriteSoapEnvelope(writer, Protocol.ProducerNamespace);
                    if (!string.IsNullOrEmpty(RequestNamespace))
                        writer.WriteAttributeString(PrefixConstants.XMLNS, "req", NamespaceConstants.XMLNS, RequestNamespace);

                    Protocol.Style.WriteSoapHeader(writer, xRoadHeader);

                    writer.WriteStartElement("Body", NamespaceConstants.SOAP_ENV);

                    operationServiceMap = serviceMap ?? requestMessage.GetSerializerCache().GetServiceMap(XName.Get(xRoadHeader.Service.ServiceCode, Protocol.ProducerNamespace));
                    operationServiceMap.SerializeRequest(writer, arg, requestMessage, RequestNamespace);

                    writer.WriteEndElement();
                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                    writer.Flush();
                }

                var request = WebRequest.Create(Uri);

                request.ContentType = $"text/xml; charset={XRoadEncoding.UTF8.WebName}";
                request.Headers["SOAPAction"] = string.Empty;
                request.Method = "POST";

                BeforeRequest?.Invoke(this, new XRoadRequestEventArgs(request, requestMessage));

                requestMessage.SaveTo(request);

#if NET40
                using (var response = request.GetResponse())
#else
                using (var response = request.GetResponseAsync().Result)
#endif
                using (var responseStream = response.GetResponseStream())
                using (var seekableStream = new MemoryStream())
                using (var responseMessage = new XRoadMessage())
                {
                    responseStream?.CopyTo(seekableStream);
                    BeforeDeserialize?.Invoke(this, new XRoadResponseEventArgs(response, seekableStream));
                    responseMessage.LoadResponse(seekableStream, response.Headers.GetContentTypeHeader(), Path.GetTempPath(), Protocol);
                    return (T)responseMessage.DeserializeMessageContent(operationServiceMap);
                }
            }
        }
    }
}
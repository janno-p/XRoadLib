using System;
using System.IO;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using XRoadLib.Extensions;
using XRoadLib.Protocols;
using XRoadLib.Protocols.Headers;
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
        /// Executes specified X-Road operations with given arguments.
        /// </summary>
        TResult Execute<TResult>(object arg, IXRoadHeader xRoadHeader, IServiceMap serviceMap = null);
    }

    /// <summary>
    /// X-Road request object which wraps single X-Road web request.
    /// </summary>
    public class XRoadRequest : IXRoadRequest
    {
        private readonly XRoadProtocol protocol;
        private readonly Uri uri;
        private readonly string requestNamespace;

        /// <summary>
        /// Initialize new request object.
        /// <param name="uri">Network location of the adapter or X-Road server.</param>
        /// <param name="protocol">X-Road message protocol version.</param>
        /// <param name="requestNamespace">Overrides default producer namespace for operation payload element.</param>
        /// </summary>
        public XRoadRequest(Uri uri, XRoadProtocol protocol, string requestNamespace = null)
        {
            this.protocol = protocol;
            this.uri = uri;
            this.requestNamespace = requestNamespace;
        }

        /// <summary>
        /// Executes specified X-Road operations with given arguments.
        /// </summary>
        public T Execute<T>(object arg, IXRoadHeader xRoadHeader, IServiceMap serviceMap = null)
        {
            IServiceMap operationServiceMap = null;

            using (var requestMessage = new XRoadMessage(protocol, xRoadHeader))
            {
                using (var writer = XmlWriter.Create(requestMessage.ContentStream))
                {
                    writer.WriteStartDocument();

                    protocol.WriteSoapEnvelope(writer);
                    if (!string.IsNullOrEmpty(requestNamespace))
                        writer.WriteAttributeString(PrefixConstants.XMLNS, "req", NamespaceConstants.XMLNS, requestNamespace);

                    protocol.WriteSoapHeader(writer, xRoadHeader);

                    writer.WriteStartElement("Body", NamespaceConstants.SOAP_ENV);

                    operationServiceMap = serviceMap ?? requestMessage.GetSerializerCache().GetServiceMap(XName.Get(xRoadHeader.Service.ServiceCode, protocol.ProducerNamespace));
                    operationServiceMap.SerializeRequest(writer, arg, requestMessage, requestNamespace);

                    writer.WriteEndElement();
                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                    writer.Flush();
                }

                var request = WebRequest.Create(uri);

                request.ContentType = $"text/xml; charset={XRoadEncoding.UTF8.WebName}";
                request.Headers["SOAPAction"] = string.Empty;
                request.Method = "POST";

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
                    responseMessage.LoadResponse(seekableStream, response.Headers.GetContentTypeHeader(), Path.GetTempPath(), protocol);
                    return (T)responseMessage.DeserializeMessageContent(operationServiceMap);
                }
            }
        }
    }
}
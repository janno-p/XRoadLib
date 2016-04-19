using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using XRoadLib.Extensions;
using XRoadLib.Protocols;
using XRoadLib.Protocols.Headers;
using XRoadLib.Serialization;

namespace XRoadLib
{
    public interface IXRoadRequest
    {
        TResult Execute<TResult>(object arg, IXRoadHeader xRoadHeader);
    }

    public class XRoadRequest : IXRoadRequest
    {
        private readonly XRoadProtocol protocol;
        private readonly Uri uri;

        public XRoadRequest(Uri uri, XRoadProtocol protocol)
        {
            this.protocol = protocol;
            this.uri = uri;
        }

        public T Execute<T>(object arg, IXRoadHeader xRoadHeader)
        {
            using (var requestMessage = new XRoadMessage(protocol, xRoadHeader))
            {
                var writer = XmlWriter.Create(requestMessage.ContentStream);

                writer.WriteStartDocument();

                protocol.WriteSoapEnvelope(writer);
                protocol.WriteSoapHeader(writer, xRoadHeader);

                writer.WriteStartElement("Body", NamespaceConstants.SOAP_ENV);

                var serviceMap = requestMessage.GetSerializerCache()
                                               .GetServiceMap(XName.Get(xRoadHeader.Service.ServiceCode, protocol.ProducerNamespace));

                serviceMap.SerializeRequest(writer, arg, requestMessage);

                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndDocument();
                writer.Flush();

                var request = WebRequest.Create(uri);

                request.ContentType = $"text/xml; charset={XRoadEncoding.UTF8.HeaderName}";
                request.Headers.Set("SOAPAction", string.Empty);
                request.Method = "POST";

                requestMessage.SaveTo(request);

                using (var response = request.GetResponse())
                using (var responseStream = response.GetResponseStream())
                using (var seekableStream = new MemoryStream())
                using (var responseMessage = new XRoadMessage())
                {
                    responseStream?.CopyTo(seekableStream);
                    responseMessage.LoadResponse(seekableStream, response.Headers, XRoadEncoding.UTF8, Path.GetTempPath(), Enumerable.Repeat(protocol, 1));
                    return (T)responseMessage.DeserializeMessageContent(xRoadHeader.Service.ServiceCode);
                }
            }
        }
    }
}
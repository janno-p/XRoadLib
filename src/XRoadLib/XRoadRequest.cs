using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using XRoadLib.Extensions;
using XRoadLib.Protocols;
using XRoadLib.Protocols.Headers;
using XRoadLib.Serialization;

namespace XRoadLib
{
    public class XRoadRequest<THeader> where THeader : IXRoadHeader, new()
    {
        private readonly Protocol<THeader> protocol;
        private readonly THeader header;
        private readonly XName operationName;

        public IDictionary<string, object> Parameters { get; } = new Dictionary<string, object>();

        public XRoadRequest(Protocol<THeader> protocol, THeader header, XName operationName)
        {
            this.header = header;
            this.operationName = operationName;
            this.protocol = protocol;
        }

        public T Execute<T>(Uri uri)
        {
            using (var requestMessage = new XRoadMessage(protocol, header))
            {
                var writer = XmlWriter.Create(requestMessage.ContentStream);

                writer.WriteStartDocument();

                protocol.WriteSoapEnvelope(writer);
                protocol.WriteSoapHeader(writer, header);

                writer.WriteStartElement("Body", NamespaceConstants.SOAP_ENV);

                var serviceMap = requestMessage.GetSerializerCache().GetServiceMap(operationName);
                serviceMap.SerializeRequest(writer, Parameters, requestMessage);

                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndDocument();
                writer.Flush();

                var request = WebRequest.Create(uri);
                requestMessage.SaveTo(request);

                using (var response = request.GetResponse())
                using (var responseStream = response.GetResponseStream())
                using (var responseMessage = new XRoadMessage())
                {
                    responseMessage.LoadResponse(responseStream, response.Headers, Encoding.UTF8, Path.GetTempPath(), Enumerable.Repeat(protocol, 1));
                    return (T)responseMessage.DeserializeMessageContent(operationName.LocalName);
                }
            }
        }
    }
}
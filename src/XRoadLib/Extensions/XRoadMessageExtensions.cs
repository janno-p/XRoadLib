using System.Xml;
using System.Xml.XPath;
using XRoadLib.Serialization;
using XRoadLib.Serialization.Mapping;
using XRoadLib.Soap;

namespace XRoadLib.Extensions
{
    public static class XRoadMessageExtensions
    {
        public static IXRoadFault DeserializeXRoadFault(this XRoadMessage message)
        {
            message.ContentStream.Position = 0;
            using (var reader = XmlReader.Create(message.ContentStream))
            {
                reader.MoveToPayload(message.RootElementName);

                var responseElementName = message.Protocol.GetResponseElementName();
                if (!reader.MoveToElement(3, responseElementName.LocalName))
                    throw XRoadException.InvalidQuery("X-Road fault should be wrapped inside `{0}` element.", responseElementName);

                var fault = new XRoadFault();

                while (reader.Read() && reader.MoveToElement(4))
                {
                    if (reader.NodeType != XmlNodeType.Element)
                        continue;

                    if (string.IsNullOrWhiteSpace(reader.NamespaceURI) && reader.LocalName == "faultCode")
                        fault.FaultCode = reader.ReadElementContentAsString();

                    if (string.IsNullOrWhiteSpace(reader.NamespaceURI) && reader.LocalName == "faultString")
                        fault.FaultString = reader.ReadElementContentAsString();
                }

                return fault;
            }
        }

        public static object DeserializeMessageContent(this XRoadMessage message, string operationName, uint version, ISerializerCache serializerCache)
        {
            var context = new SerializationContext(message, version);
            var serviceMap = serializerCache.GetServiceMap(operationName, context.DtoVersion);

            message.ContentStream.Position = 0;
            var doc = new XPathDocument(XmlReader.Create(message.ContentStream));
            var navigator = doc.CreateNavigator();

            if (navigator.SelectSingleNode("/*[local-name()='Envelope' and namespace-uri()='http://schemas.xmlsoap.org/soap/envelope/']/*[local-name()='Body' and namespace-uri()='http://schemas.xmlsoap.org/soap/envelope/']/*/keha/faultCode | /*[local-name()='Envelope' and namespace-uri()='http://schemas.xmlsoap.org/soap/envelope/']/*[local-name()='Body' and namespace-uri()='http://schemas.xmlsoap.org/soap/envelope/']/*/keha/faultString") != null)
                throw new XRoadFaultException(message.DeserializeXRoadFault());

            message.ContentStream.Position = 0;
            using (var reader = XmlReader.Create(message.ContentStream))
            {
                var result = serviceMap.DeserializeResponse(reader, context);

                var soapFault = result as ISoapFault;
                if (soapFault != null)
                    throw new SoapFaultException(soapFault);

                return result;
            }
        }
    }
}
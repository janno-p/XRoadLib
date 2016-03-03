using System.Xml;
using System.Xml.XPath;
using XRoadLib.Serialization;
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

                var responseName = message.Protocol.ResponsePartNameInResponse;
                if (!reader.MoveToElement(3, responseName))
                    throw XRoadException.InvalidQuery("X-Road fault should be wrapped inside `{0}` element.", responseName);

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

        public static object DeserializeMessageContent(this XRoadMessage message, string operationName)
        {
            message.ContentStream.Position = 0;
            var doc = new XPathDocument(XmlReader.Create(message.ContentStream));
            var navigator = doc.CreateNavigator();

            var pathRoot = "/*[local-name()='Envelope' and namespace-uri()='http://schemas.xmlsoap.org/soap/envelope/']/*[local-name()='Body' and namespace-uri()='http://schemas.xmlsoap.org/soap/envelope/']/*";
            if (message.Protocol.NonTechnicalFaultInResponseElement)
                pathRoot = $"{pathRoot}/{message.Protocol.ResponsePartNameInResponse}";

            if (navigator.SelectSingleNode($"{pathRoot}/faultCode | {pathRoot}/faultString") != null)
                throw new XRoadFaultException(message.DeserializeXRoadFault());

            message.ContentStream.Position = 0;
            using (var reader = XmlReader.Create(message.ContentStream))
            {
                reader.MoveToBody();

                if (!reader.MoveToElement(2))
                    throw XRoadException.InvalidQuery("No payload element in SOAP message.");

                if (reader.NamespaceURI == NamespaceConstants.SOAP_ENV && reader.LocalName == "Fault")
                    throw new SoapFaultException(SoapMessageHelper.DeserializeSoapFault(reader));

                var serializerCache = message.GetSerializerCache();
                var serviceMap = serializerCache.GetServiceMap(operationName);

                return serviceMap.DeserializeResponse(reader, message);
            }
        }
    }
}
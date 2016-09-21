using System.Xml;
using System.Xml.XPath;
using XRoadLib.Serialization;
using XRoadLib.Serialization.Mapping;
using XRoadLib.Soap;

namespace XRoadLib.Extensions
{
    /// <summary>
    /// Extension methods for <see>XRoadMessage</see> class.
    /// </summary>
    public static class XRoadMessageExtensions
    {
        /// <summary>
        /// Deserializes X-Road fault from message which is known to contain fault.
        /// </summary>
        public static IXRoadFault DeserializeXRoadFault(this XRoadMessage message)
        {
            message.ContentStream.Position = 0;
            using (var reader = XmlReader.Create(message.ContentStream))
            {
                reader.MoveToPayload(message.RootElementName);

                var responseName = message.Protocol.ResponsePartNameInResponse;
                if (!reader.MoveToElement(3, responseName))
                    throw XRoadException.InvalidQuery($"X-Road fault should be wrapped inside `{responseName}` element.");

                return reader.ReadXRoadFault(4);
            }
        }

        /// <summary>
        /// Deserializes X-Road message response or throws <see>XRoadFaultException</see> when
        /// X-Road fault is parsed from the message instead of expected result value.
        /// </summary>
        public static object DeserializeMessageContent(this XRoadMessage message, IServiceMap serviceMap)
        {
            if (message.Protocol.NonTechnicalFaultInResponseElement)
                ThrowIfXRoadFault(message);

            message.ContentStream.Position = 0;
            using (var reader = XmlReader.Create(message.ContentStream))
            {
                reader.MoveToBody();

                if (!reader.MoveToElement(2))
                    throw XRoadException.InvalidQuery("No payload element in SOAP message.");

                if (reader.NamespaceURI == NamespaceConstants.SOAP_ENV && reader.LocalName == "Fault")
                    throw new SoapFaultException(SoapMessageHelper.DeserializeSoapFault(reader));

                var serializerCache = message.GetSerializerCache();

                var result = serviceMap.DeserializeResponse(reader, message);

                var fault = result as XRoadFault;
                if (fault != null)
                    throw new XRoadFaultException(fault);

                return result;
            }
        }

        private static void ThrowIfXRoadFault(XRoadMessage message)
        {
            if (message.Protocol == null)
                return;

            message.ContentStream.Position = 0;

            var pathRoot = $"/*[local-name()='Envelope' and namespace-uri()='http://schemas.xmlsoap.org/soap/envelope/']/*[local-name()='Body' and namespace-uri()='http://schemas.xmlsoap.org/soap/envelope/']/*/{message.Protocol.ResponsePartNameInResponse}";

            var doc = new XPathDocument(XmlReader.Create(message.ContentStream));
            var navigator = doc.CreateNavigator();

            if (navigator.SelectSingleNode($"{pathRoot}/faultCode | {pathRoot}/faultString") != null)
                throw new XRoadFaultException(message.DeserializeXRoadFault());
        }
    }
}
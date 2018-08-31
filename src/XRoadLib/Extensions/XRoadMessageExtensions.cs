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
        /// Deserializes X-Road message response or throws <see>XRoadFaultException</see> when
        /// X-Road fault is parsed from the message instead of expected result value.
        /// </summary>
        public static object DeserializeMessageContent(this XRoadMessage message, IServiceMap serviceMap)
        {
            if (serviceMap.ResponseDefinition.ContainsNonTechnicalFault)
                ThrowIfXRoadFault(message, serviceMap);

            message.ContentStream.Position = 0;
            using (var reader = XmlReader.Create(message.ContentStream))
            {
                reader.MoveToBody();

                if (!reader.MoveToElement(2))
                    throw new InvalidQueryException("No payload element in SOAP message.");

                if (reader.NamespaceURI == NamespaceConstants.SOAP_ENV && reader.LocalName == "Fault")
                    throw new SoapFaultException(SoapMessageHelper.DeserializeSoapFault(reader));

                var result = serviceMap.DeserializeResponse(reader, message);

                return result is XRoadFault fault ? throw new XRoadFaultException(fault) : result;
            }
        }

        private static void ThrowIfXRoadFault(XRoadMessage message, IServiceMap serviceMap)
        {
            message.ContentStream.Position = 0;

            var wrapperElement = serviceMap.ResponseDefinition.WrapperElementName;
            var responseElement = serviceMap.ResponseDefinition.Content.Name;
            var pathRoot = $"/*[local-name()='Envelope' and namespace-uri()='http://schemas.xmlsoap.org/soap/envelope/']/*[local-name()='Body' and namespace-uri()='http://schemas.xmlsoap.org/soap/envelope/']/*[local-name()='{wrapperElement.LocalName}' and namespace-uri()='{wrapperElement.NamespaceName}']/*[local-name()='{responseElement.LocalName}' and namespace-uri()='{responseElement.NamespaceName}']";

            var doc = new XPathDocument(XmlReader.Create(message.ContentStream));
            var navigator = doc.CreateNavigator();

            if (navigator.SelectSingleNode($"{pathRoot}/faultCode | {pathRoot}/faultString") != null)
                throw new XRoadFaultException(serviceMap.DeserializeXRoadFault(message));
        }
    }
}
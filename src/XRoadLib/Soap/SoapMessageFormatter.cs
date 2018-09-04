using System.Xml;
using System.Xml.Linq;
using XRoadLib.Extensions;
using XRoadLib.Serialization;

namespace XRoadLib.Soap
{
    public class SoapMessageFormatter : IMessageFormatter
    {
        private static readonly XName envelopeName = XName.Get("Envelope", NamespaceConstants.SOAP_ENV);
        private static readonly XName headerName = XName.Get("Header", NamespaceConstants.SOAP_ENV);
        private static readonly XName bodyName = XName.Get("Body", NamespaceConstants.SOAP_ENV);

        public void MoveToEnvelope(XmlReader reader)
        {
            if (!reader.MoveToElement(0, envelopeName))
                throw new InvalidQueryException($"X-Road SOAP request is missing {envelopeName} element.");
        }

        public bool TryMoveToHeader(XmlReader reader)
        {
            return reader.MoveToElement(1) && reader.IsCurrentElement(1, headerName);
        }

        public bool TryMoveToBody(XmlReader reader)
        {
            return reader.MoveToElement(1, bodyName);
        }
    }
}
using System;
using System.Xml;
using XRoadLib.Extensions;
using XRoadLib.Serialization;
using XRoadLib.Serialization.Mapping;
using XRoadLib.Soap;

namespace XRoadLib
{
    public static class SoapMessageHelper
    {
        public static void SerializeSoapFaultResponse(XmlWriter writer, FaultCode faultCode = null, string message = null, string faultActor = null, string faultDetail = null, Exception exception = null)
        {
            writer.WriteStartDocument();

            writer.WriteStartElement(PrefixConstants.SOAP_ENV, "Envelope", NamespaceConstants.SOAP_ENV);
            writer.WriteAttributeString("xmlns", PrefixConstants.SOAP_ENV, NamespaceConstants.XMLNS, NamespaceConstants.SOAP_ENV);

            writer.WriteStartElement("Body", NamespaceConstants.SOAP_ENV);
            writer.WriteStartElement("Fault", NamespaceConstants.SOAP_ENV);

            var faultFaultCode = (exception as XRoadException)?.FaultCode ?? faultCode ?? ServerFaultCode.InternalError;
            var faultMessage = !string.IsNullOrWhiteSpace(message) ? message : (exception?.Message ?? string.Empty);

            writer.WriteStartElement("faultcode");
            writer.WriteValue(faultFaultCode.Value);
            writer.WriteEndElement();

            writer.WriteStartElement("faultstring");
            writer.WriteCData(faultMessage);
            writer.WriteEndElement();

            if (!string.IsNullOrWhiteSpace(faultActor))
            {
                writer.WriteStartElement("faultactor");
                writer.WriteValue(faultActor);
                writer.WriteEndElement();
            }

            if (!string.IsNullOrWhiteSpace(faultDetail))
            {
                writer.WriteStartElement("detail");
                writer.WriteValue(faultDetail);
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.WriteEndElement();

            writer.Flush();
        }
    }
}
using System;
using System.Xml;
using XRoadLib.Extensions;
using XRoadLib.Serialization;

namespace XRoadLib.Soap
{
    public static class SoapMessageHelper
    {
        public static void SerializeSoapFaultResponse(XmlWriter writer, FaultCode faultCode = null, string message = null, string faultActor = null, string faultDetail = null, Exception exception = null)
        {
            writer.WriteStartDocument();

            writer.WriteStartElement(PrefixConstants.SOAP_ENV, "Envelope", NamespaceConstants.SOAP_ENV);
            writer.WriteAttributeString(PrefixConstants.XMLNS, PrefixConstants.SOAP_ENV, NamespaceConstants.XMLNS, NamespaceConstants.SOAP_ENV);

            writer.WriteStartElement("Body", NamespaceConstants.SOAP_ENV);
            writer.WriteStartElement("Fault", NamespaceConstants.SOAP_ENV);

            var faultFaultCode = (exception as XRoadException)?.FaultCode ?? faultCode ?? ServerFaultCode.InternalError;
            var faultMessage = !string.IsNullOrWhiteSpace(message) ? message : (exception?.Message ?? string.Empty);

            writer.WriteStartElement("faultcode");
            writer.WriteString(faultFaultCode.Value);
            writer.WriteEndElement();

            writer.WriteStartElement("faultstring");
            writer.WriteString(faultMessage);
            writer.WriteEndElement();

            if (!string.IsNullOrWhiteSpace(faultActor))
            {
                writer.WriteStartElement("faultactor");
                writer.WriteString(faultActor);
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

        public static ISoapFault DeserializeSoapFault(XmlReader reader)
        {
            const int depth = 3;

            var fault = new SoapFault();

            if (reader.IsEmptyElement || !reader.MoveToElement(depth, "faultcode"))
                throw new InvalidQueryException("SOAP Fault must have `faultcode` element.");
            fault.FaultCode = reader.ReadElementContentAsString();

            if (!reader.MoveToElement(depth, "faultstring"))
                throw new InvalidQueryException("SOAP Fault must have `faultstring` element.");
            fault.FaultString = reader.ReadElementContentAsString();

            var success = reader.MoveToElement(depth);
            if (success && reader.LocalName == "faultactor" && reader.NamespaceURI == "")
            {
                fault.FaultActor = reader.ReadElementContentAsString();
                success = reader.MoveToElement(depth);
            }

            if (success && reader.LocalName == "detail" && reader.NamespaceURI == "")
            {
                fault.Details = reader.ReadInnerXml();
                success = reader.MoveToElement(depth);
            }

            if (success)
                throw new InvalidQueryException($"Unexpected element `{reader.GetXName()}` in SOAP Fault element.");

            return fault;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using XRoadLib.Extensions;
using XRoadLib.Headers;
using XRoadLib.Schema;
using XRoadLib.Serialization;
using XRoadLib.Styles;

namespace XRoadLib.Soap
{
    public class SoapMessageFormatter : IMessageFormatter
    {
        private static readonly XName EnvelopeName = XName.Get("Envelope", NamespaceConstants.SoapEnv);
        private static readonly XName HeaderName = XName.Get("Header", NamespaceConstants.SoapEnv);
        private static readonly XName BodyName = XName.Get("Body", NamespaceConstants.SoapEnv);

        public string ContentType { get; } = ContentTypes.Soap;
        public string Namespace { get; } = NamespaceConstants.SoapEnv;

        public void MoveToEnvelope(XmlReader reader)
        {
            if (!TryMoveToEnvelope(reader))
                throw new InvalidQueryException($"Element `{EnvelopeName}` is missing from message content.");
        }

        public void MoveToBody(XmlReader reader)
        {
            MoveToEnvelope(reader);

            if (!TryMoveToBody(reader))
                throw new InvalidQueryException($"Element `{BodyName}` is missing from message content.");
        }

        public void MoveToPayload(XmlReader reader, XName payloadName)
        {
            MoveToBody(reader);

            if (!reader.MoveToElement(2, payloadName))
                throw new InvalidQueryException($"Payload element `{payloadName}` is missing from message content.");
        }

        public bool TryMoveToEnvelope(XmlReader reader)
        {
            return reader.MoveToElement(0, EnvelopeName);
        }

        public bool TryMoveToHeader(XmlReader reader)
        {
            return reader.MoveToElement(1) && reader.IsCurrentElement(1, HeaderName);
        }

        public bool TryMoveToBody(XmlReader reader)
        {
            return reader.MoveToElement(1, BodyName);
        }

        public void WriteStartEnvelope(XmlWriter writer, string prefix = null)
        {
            var prefixValue = string.IsNullOrEmpty(prefix) ? PrefixConstants.SoapEnv : prefix;
            writer.WriteStartElement(prefixValue, EnvelopeName.LocalName, EnvelopeName.NamespaceName);
        }

        public void WriteStartBody(XmlWriter writer)
        {
            writer.WriteStartElement(BodyName);
        }

        public IFault CreateFault(Exception exception)
        {
            return new SoapFault
            {
                FaultCode = ((exception as XRoadException)?.FaultCode ?? ServerFaultCode.InternalError).Value,
                FaultString = exception?.Message ?? string.Empty
            };
        }

        public void WriteSoapFault(XmlWriter writer, IFault fault)
        {
            var soapFault = (ISoapFault)fault;

            writer.WriteStartDocument();

            WriteStartEnvelope(writer);
            writer.WriteAttributeString(PrefixConstants.Xmlns, PrefixConstants.SoapEnv, NamespaceConstants.Xmlns, NamespaceConstants.SoapEnv);

            WriteStartBody(writer);
            writer.WriteStartElement("Fault", NamespaceConstants.SoapEnv);

            writer.WriteStartElement("faultcode");
            writer.WriteString(soapFault.FaultCode);
            writer.WriteEndElement();

            writer.WriteStartElement("faultstring");
            writer.WriteString(soapFault.FaultString);
            writer.WriteEndElement();

            if (!string.IsNullOrWhiteSpace(soapFault.FaultActor))
            {
                writer.WriteStartElement("faultactor");
                writer.WriteString(soapFault.FaultActor);
                writer.WriteEndElement();
            }

            if (!string.IsNullOrWhiteSpace(soapFault.Details))
            {
                writer.WriteStartElement("detail");
                writer.WriteValue(soapFault.Details);
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.WriteEndElement();

            writer.Flush();
        }

        public void WriteSoapHeader(XmlWriter writer, Style style, ISoapHeader header, HeaderDefinition definition, IEnumerable<XElement> additionalHeaders = null)
        {
            if (header == null)
                return;

            writer.WriteStartElement("Header", NamespaceConstants.SoapEnv);

            header.WriteTo(writer, style, definition);

            foreach (var additionalHeader in additionalHeaders ?? Enumerable.Empty<XElement>())
                additionalHeader.WriteTo(writer);

            writer.WriteEndElement();
        }

        public void ThrowSoapFaultIfPresent(XmlReader reader)
        {
            if (reader.NamespaceURI == NamespaceConstants.SoapEnv && reader.LocalName == "Fault")
                throw new SoapFaultException(DeserializeSoapFault(reader));
        }

        private static ISoapFault DeserializeSoapFault(XmlReader reader)
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
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
    public class Soap12MessageFormatter : IMessageFormatter
    {
        private static readonly string envPrefix = PrefixConstants.SOAP12_ENV;
        private static readonly string @namespace = NamespaceConstants.SOAP12_ENV;

        private static readonly XName envelopeName = XName.Get("Envelope", @namespace);
        private static readonly XName headerName = XName.Get("Header", @namespace);
        private static readonly XName bodyName = XName.Get("Body", @namespace);
        private static readonly XName faultCodeName = XName.Get("Code", @namespace);
        private static readonly XName faultReasonName = XName.Get("Reason", @namespace);
        private static readonly XName faultNodeName = XName.Get("Node", @namespace);
        private static readonly XName faultRoleName = XName.Get("Role", @namespace);
        private static readonly XName faultDetailName = XName.Get("Detail", @namespace);
        private static readonly XName faultCodeValueName = XName.Get("Value", @namespace);
        private static readonly XName faultCodeSubcodeName = XName.Get("Subcode", @namespace);
        private static readonly XName faultReasonTextName = XName.Get("Text", @namespace);

        public string ContentType { get; } = ContentTypes.SOAP12;
        public string Namespace { get; } = @namespace;

        public void MoveToEnvelope(XmlReader reader)
        {
            if (!TryMoveToEnvelope(reader))
                throw new InvalidQueryException($"Element `{envelopeName}` is missing from message content.");
        }

        public void MoveToBody(XmlReader reader)
        {
            MoveToEnvelope(reader);

            if (!TryMoveToBody(reader))
                throw new InvalidQueryException($"Element `{bodyName}` is missing from message content.");
        }

        public void MoveToPayload(XmlReader reader, XName payloadName)
        {
            MoveToBody(reader);

            if (!reader.MoveToElement(2, payloadName))
                throw new InvalidQueryException($"Payload element `{payloadName}` is missing from message content.");
        }

        public bool TryMoveToEnvelope(XmlReader reader)
        {
            return reader.MoveToElement(0, envelopeName);
        }

        public bool TryMoveToHeader(XmlReader reader)
        {
            return reader.MoveToElement(1) && reader.IsCurrentElement(1, headerName);
        }

        public bool TryMoveToBody(XmlReader reader)
        {
            return reader.MoveToElement(1, bodyName);
        }

        public void WriteStartEnvelope(XmlWriter writer, string prefix = null)
        {
            var prefixValue = string.IsNullOrEmpty(prefix) ? envPrefix : prefix;
            writer.WriteStartElement(prefixValue, envelopeName.LocalName, envelopeName.NamespaceName);
        }

        public void WriteStartBody(XmlWriter writer)
        {
            writer.WriteStartElement(bodyName);
        }

        public IFault CreateFault(Exception exception)
        {
            return new Soap12Fault
            {
                Code = new Soap12FaultCode
                {
                    Value = Soap12FaultCodeEnum.Receiver,
                    Subcode = new Soap12FaultSubcode { Value = ServerFaultCode.InternalError.Value }
                },
                Reason = new List<Soap12FaultReasonText>
                {
                    new Soap12FaultReasonText
                    {
                        LanguageCode = "en",
                        Text = exception?.Message ?? "Unexpected error occurred."
                    }
                }
            };
        }

        public void WriteSoapFault(XmlWriter writer, IFault fault)
        {
            var soapFault = (ISoap12Fault)fault;

            writer.WriteStartDocument();

            WriteStartEnvelope(writer);
            writer.WriteAttributeString(PrefixConstants.XMLNS, envPrefix, NamespaceConstants.XMLNS, @namespace);

            WriteStartBody(writer);
            writer.WriteStartElement("Fault", @namespace);

            WriteSoapFaultCode(writer, soapFault.Code);
            WriteSoapFaultReason(writer, soapFault.Reason);

            if (!string.IsNullOrWhiteSpace(soapFault.Node))
            {
                writer.WriteStartElement(faultNodeName);
                writer.WriteString(soapFault.Node);
                writer.WriteEndElement();
            }

            if (!string.IsNullOrWhiteSpace(soapFault.Role))
            {
                writer.WriteStartElement(faultRoleName);
                writer.WriteString(soapFault.Role);
                writer.WriteEndElement();
            }

            if (!string.IsNullOrWhiteSpace(soapFault.Detail))
            {
                writer.WriteStartElement(faultDetailName);
                writer.WriteValue(soapFault.Detail);
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.WriteEndElement();

            writer.Flush();
        }

        private static void WriteSoapFaultCode(XmlWriter writer, Soap12FaultCode faultCode)
        {
            writer.WriteStartElement(faultCodeName);

            writer.WriteStartElement(faultCodeValueName);
            WriteSoapFaultValueEnum(writer, faultCode.Value);
            writer.WriteEndElement();

            WriteSoapFaultSubcode(writer, faultCode.Subcode);

            writer.WriteEndElement();
        }

        private static void WriteSoapFaultSubcode(XmlWriter writer, Soap12FaultSubcode subcode)
        {
            if (subcode == null)
                return;

            writer.WriteStartElement("Subcode", @namespace);

            writer.WriteStartElement("Value", @namespace);
            writer.WriteString(subcode.Value);
            writer.WriteEndElement();

            WriteSoapFaultSubcode(writer, subcode.Subcode);

            writer.WriteEndElement();
        }

        private static void WriteSoapFaultValueEnum(XmlWriter writer, Soap12FaultCodeEnum value)
        {
            switch (value)
            {
                case Soap12FaultCodeEnum.Receiver:
                    writer.WriteQualifiedName("Receiver", @namespace);
                    break;

                case Soap12FaultCodeEnum.Sender:
                    writer.WriteQualifiedName("Sender", @namespace);
                    break;

                case Soap12FaultCodeEnum.MustUnderstand:
                    writer.WriteQualifiedName("MustUnderstand", @namespace);
                    break;

                case Soap12FaultCodeEnum.VersionMismatch:
                    writer.WriteQualifiedName("VersionMismatch", @namespace);
                    break;

                case Soap12FaultCodeEnum.DataEncodingUnknown:
                    writer.WriteQualifiedName("DataEncodingUnknown", @namespace);
                    break;

                default:
                    throw new ArgumentException($"Invalid SOAP 1.2 Fault Code enumeration value `{value}`.", nameof(value));
            }
        }

        private static void WriteSoapFaultReason(XmlWriter writer, IList<Soap12FaultReasonText> faultReasons)
        {
            writer.WriteStartElement(faultReasonName);

            foreach (var text in faultReasons)
            {
                writer.WriteStartElement(faultReasonTextName);
                writer.WriteAttributeString("xml", "lang", NamespaceConstants.XML, text.LanguageCode);
                writer.WriteString(text.Text);
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        public void WriteSoapHeader(XmlWriter writer, Style style, ISoapHeader header, HeaderDefinition definition, IEnumerable<XElement> additionalHeaders = null)
        {
            if (header == null)
                return;

            writer.WriteStartElement("Header", @namespace);

            header.WriteTo(writer, style, definition);

            foreach (var additionalHeader in additionalHeaders ?? Enumerable.Empty<XElement>())
                additionalHeader.WriteTo(writer);

            writer.WriteEndElement();
        }

        public void ThrowSoapFaultIfPresent(XmlReader reader)
        {
            if (reader.NamespaceURI == @namespace && reader.LocalName == "Fault")
                throw new Soap12FaultException(DeserializeSoapFault(reader));
        }

        private static ISoap12Fault DeserializeSoapFault(XmlReader reader)
        {
            const int depth = 3;

            var fault = new Soap12Fault();

            if (reader.IsEmptyElement || !reader.MoveToElement(depth, faultCodeName))
                throw new InvalidQueryException($"SOAP 1.2 Fault must have `{faultCodeName}` element.");
            fault.Code = DeserializeSoapFaultCode(reader);

            if (!reader.MoveToElement(depth, faultReasonName))
                throw new InvalidQueryException($"SOAP 1.2 Fault must have `{faultReasonName}` element.");
            fault.Reason = DeserializeSoapFaultReason(reader);

            var success = reader.MoveToElement(depth);
            if (success && reader.IsCurrentElement(depth, faultNodeName))
            {
                fault.Node = reader.ReadElementContentAsString();
                success = reader.MoveToElement(depth);
            }

            if (success && reader.IsCurrentElement(depth, faultRoleName))
            {
                fault.Role = reader.ReadInnerXml();
                success = reader.MoveToElement(depth);
            }

            if (success && reader.IsCurrentElement(depth, faultDetailName))
            {
                fault.Detail = reader.ReadInnerXml();
                success = reader.MoveToElement(depth);
            }

            if (success)
                throw new InvalidQueryException($"Unexpected element `{reader.GetXName()}` in SOAP 1.2 Fault element.");

            return fault;
        }

        private static Soap12FaultCode DeserializeSoapFaultCode(XmlReader reader)
        {
            const int depth = 4;

            var faultCode = new Soap12FaultCode();

            if (reader.IsEmptyElement || !reader.MoveToElement(depth, faultCodeValueName))
                throw new InvalidQueryException($"SOAP 1.2 Fault Code must have `{faultCodeValueName}` element.");
            faultCode.Value = DeserializeFaultCodeValue(reader);

            var success = reader.MoveToElement(depth);
            if (success && reader.IsCurrentElement(depth, faultCodeSubcodeName))
            {
                faultCode.Subcode = DeserializeFaultCodeSubcode(reader, depth + 1);
                success = reader.MoveToElement(depth);
            }

            if (success)
                throw new InvalidQueryException($"Unexpected element `{reader.GetXName()}` in SOAP 1.2 Fault Code element.");

            return faultCode;
        }

        private static Soap12FaultSubcode DeserializeFaultCodeSubcode(XmlReader reader, int depth)
        {
            var faultSubcode = new Soap12FaultSubcode();

            if (reader.IsEmptyElement || !reader.MoveToElement(depth, faultCodeValueName))
                throw new InvalidQueryException($"SOAP 1.2 Fault Subcode must have `{faultCodeValueName}` element.");
            faultSubcode.Value = reader.ReadElementContentAsString();

            var success = reader.MoveToElement(depth);
            if (success && reader.IsCurrentElement(depth, faultCodeSubcodeName))
            {
                faultSubcode.Subcode = DeserializeFaultCodeSubcode(reader, depth + 1);
                success = reader.MoveToElement(depth);
            }

            if (success)
                throw new InvalidQueryException($"Unexpected element `{reader.GetXName()}` in SOAP 1.2 Fault Subcode element.");

            return faultSubcode;
        }

        private static Soap12FaultCodeEnum DeserializeFaultCodeValue(XmlReader reader)
        {
            var qualifiedValue = reader.ReadElementContentAsString();
            var enumName = reader.ParseQualifiedValue(qualifiedValue);

            if (!enumName.NamespaceName.Equals(@namespace))
                return Soap12FaultCodeEnum.None;

            switch (enumName.LocalName)
            {
                case "DataEncodingUnknown":
                    return Soap12FaultCodeEnum.DataEncodingUnknown;
                case "MustUnderstand":
                    return Soap12FaultCodeEnum.MustUnderstand;
                case "Receiver":
                    return Soap12FaultCodeEnum.Receiver;
                case "Sender":
                    return Soap12FaultCodeEnum.Sender;
                case "VersionMismatch":
                    return Soap12FaultCodeEnum.VersionMismatch;
                default:
                    return Soap12FaultCodeEnum.None;
            }
        }

        private static IList<Soap12FaultReasonText> DeserializeSoapFaultReason(XmlReader reader)
        {
            const int depth = 4;

            var faultReasons = new List<Soap12FaultReasonText>();

            if (reader.IsEmptyElement || !reader.MoveToElement(depth, faultReasonTextName))
                throw new InvalidQueryException($"SOAP 1.2 Fault Reason must have `{faultReasonTextName}` element.");

            do
            {
                faultReasons.Add(new Soap12FaultReasonText
                {
                    LanguageCode = reader.GetAttribute("lang", NamespaceConstants.XML),
                    Text = reader.ReadElementContentAsString()
                });
            } while (reader.MoveToElement(depth, faultReasonTextName));

            if (reader.Depth > 3)
                throw new InvalidQueryException($"Unexpected element `{reader.GetXName()}` in SOAP 1.2 Fault Reason element.");

            return faultReasons;
        }
    }
}
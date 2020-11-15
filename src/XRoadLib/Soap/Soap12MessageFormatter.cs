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
        private static readonly XName EnvelopeName = XName.Get("Envelope", NamespaceConstants.Soap12Env);
        private static readonly XName HeaderName = XName.Get("Header", NamespaceConstants.Soap12Env);
        private static readonly XName BodyName = XName.Get("Body", NamespaceConstants.Soap12Env);
        private static readonly XName FaultCodeName = XName.Get("Code", NamespaceConstants.Soap12Env);
        private static readonly XName FaultReasonName = XName.Get("Reason", NamespaceConstants.Soap12Env);
        private static readonly XName FaultNodeName = XName.Get("Node", NamespaceConstants.Soap12Env);
        private static readonly XName FaultRoleName = XName.Get("Role", NamespaceConstants.Soap12Env);
        private static readonly XName FaultDetailName = XName.Get("Detail", NamespaceConstants.Soap12Env);
        private static readonly XName FaultCodeValueName = XName.Get("Value", NamespaceConstants.Soap12Env);
        private static readonly XName FaultCodeSubcodeName = XName.Get("Subcode", NamespaceConstants.Soap12Env);
        private static readonly XName FaultReasonTextName = XName.Get("Text", NamespaceConstants.Soap12Env);

        public string ContentType { get; } = ContentTypes.Soap12;
        public string Namespace { get; } = NamespaceConstants.Soap12Env;

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
            var prefixValue = string.IsNullOrEmpty(prefix) ? PrefixConstants.Soap12Env : prefix;
            writer.WriteStartElement(prefixValue, EnvelopeName.LocalName, EnvelopeName.NamespaceName);
        }

        public void WriteStartBody(XmlWriter writer)
        {
            writer.WriteStartElement(BodyName);
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
            writer.WriteAttributeString(PrefixConstants.Xmlns, PrefixConstants.Soap12Env, NamespaceConstants.Xmlns, NamespaceConstants.Soap12Env);

            WriteStartBody(writer);
            writer.WriteStartElement("Fault", NamespaceConstants.Soap12Env);

            WriteSoapFaultCode(writer, soapFault.Code);
            WriteSoapFaultReason(writer, soapFault.Reason);

            if (!string.IsNullOrWhiteSpace(soapFault.Node))
            {
                writer.WriteStartElement(FaultNodeName);
                writer.WriteString(soapFault.Node);
                writer.WriteEndElement();
            }

            if (!string.IsNullOrWhiteSpace(soapFault.Role))
            {
                writer.WriteStartElement(FaultRoleName);
                writer.WriteString(soapFault.Role);
                writer.WriteEndElement();
            }

            if (!string.IsNullOrWhiteSpace(soapFault.Detail))
            {
                writer.WriteStartElement(FaultDetailName);
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
            writer.WriteStartElement(FaultCodeName);

            writer.WriteStartElement(FaultCodeValueName);
            WriteSoapFaultValueEnum(writer, faultCode.Value);
            writer.WriteEndElement();

            WriteSoapFaultSubcode(writer, faultCode.Subcode);

            writer.WriteEndElement();
        }

        private static void WriteSoapFaultSubcode(XmlWriter writer, Soap12FaultSubcode subcode)
        {
            if (subcode == null)
                return;

            writer.WriteStartElement("Subcode", NamespaceConstants.Soap12Env);

            writer.WriteStartElement("Value", NamespaceConstants.Soap12Env);
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
                    writer.WriteQualifiedName("Receiver", NamespaceConstants.Soap12Env);
                    break;

                case Soap12FaultCodeEnum.Sender:
                    writer.WriteQualifiedName("Sender", NamespaceConstants.Soap12Env);
                    break;

                case Soap12FaultCodeEnum.MustUnderstand:
                    writer.WriteQualifiedName("MustUnderstand", NamespaceConstants.Soap12Env);
                    break;

                case Soap12FaultCodeEnum.VersionMismatch:
                    writer.WriteQualifiedName("VersionMismatch", NamespaceConstants.Soap12Env);
                    break;

                case Soap12FaultCodeEnum.DataEncodingUnknown:
                    writer.WriteQualifiedName("DataEncodingUnknown", NamespaceConstants.Soap12Env);
                    break;

                default:
                    throw new ArgumentException($"Invalid SOAP 1.2 Fault Code enumeration value `{value}`.", nameof(value));
            }
        }

        private static void WriteSoapFaultReason(XmlWriter writer, IList<Soap12FaultReasonText> faultReasons)
        {
            writer.WriteStartElement(FaultReasonName);

            foreach (var text in faultReasons)
            {
                writer.WriteStartElement(FaultReasonTextName);
                writer.WriteAttributeString("xml", "lang", NamespaceConstants.Xml, text.LanguageCode);
                writer.WriteString(text.Text);
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        public void WriteSoapHeader(XmlWriter writer, Style style, ISoapHeader header, HeaderDefinition definition, IEnumerable<XElement> additionalHeaders = null)
        {
            if (header == null)
                return;

            writer.WriteStartElement("Header", NamespaceConstants.Soap12Env);

            header.WriteTo(writer, style, definition);

            foreach (var additionalHeader in additionalHeaders ?? Enumerable.Empty<XElement>())
                additionalHeader.WriteTo(writer);

            writer.WriteEndElement();
        }

        public void ThrowSoapFaultIfPresent(XmlReader reader)
        {
            if (reader.NamespaceURI == NamespaceConstants.Soap12Env && reader.LocalName == "Fault")
                throw new Soap12FaultException(DeserializeSoapFault(reader));
        }

        private static ISoap12Fault DeserializeSoapFault(XmlReader reader)
        {
            const int depth = 3;

            var fault = new Soap12Fault();

            if (reader.IsEmptyElement || !reader.MoveToElement(depth, FaultCodeName))
                throw new InvalidQueryException($"SOAP 1.2 Fault must have `{FaultCodeName}` element.");
            fault.Code = DeserializeSoapFaultCode(reader);

            if (!reader.MoveToElement(depth, FaultReasonName))
                throw new InvalidQueryException($"SOAP 1.2 Fault must have `{FaultReasonName}` element.");
            fault.Reason = DeserializeSoapFaultReason(reader);

            var success = reader.MoveToElement(depth);
            if (success && reader.IsCurrentElement(depth, FaultNodeName))
            {
                fault.Node = reader.ReadElementContentAsString();
                success = reader.MoveToElement(depth);
            }

            if (success && reader.IsCurrentElement(depth, FaultRoleName))
            {
                fault.Role = reader.ReadInnerXml();
                success = reader.MoveToElement(depth);
            }

            if (success && reader.IsCurrentElement(depth, FaultDetailName))
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

            if (reader.IsEmptyElement || !reader.MoveToElement(depth, FaultCodeValueName))
                throw new InvalidQueryException($"SOAP 1.2 Fault Code must have `{FaultCodeValueName}` element.");
            faultCode.Value = DeserializeFaultCodeValue(reader);

            var success = reader.MoveToElement(depth);
            if (success && reader.IsCurrentElement(depth, FaultCodeSubcodeName))
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

            if (reader.IsEmptyElement || !reader.MoveToElement(depth, FaultCodeValueName))
                throw new InvalidQueryException($"SOAP 1.2 Fault Subcode must have `{FaultCodeValueName}` element.");
            faultSubcode.Value = reader.ReadElementContentAsString();

            var success = reader.MoveToElement(depth);
            if (success && reader.IsCurrentElement(depth, FaultCodeSubcodeName))
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

            if (!enumName.NamespaceName.Equals(NamespaceConstants.Soap12Env))
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

            if (reader.IsEmptyElement || !reader.MoveToElement(depth, FaultReasonTextName))
                throw new InvalidQueryException($"SOAP 1.2 Fault Reason must have `{FaultReasonTextName}` element.");

            do
            {
                faultReasons.Add(new Soap12FaultReasonText
                {
                    LanguageCode = reader.GetAttribute("lang", NamespaceConstants.Xml),
                    Text = reader.ReadElementContentAsString()
                });
            } while (reader.MoveToElement(depth, FaultReasonTextName));

            if (reader.Depth > 3)
                throw new InvalidQueryException($"Unexpected element `{reader.GetXName()}` in SOAP 1.2 Fault Reason element.");

            return faultReasons;
        }
    }
}
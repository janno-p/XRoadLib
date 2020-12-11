using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public async Task MoveToEnvelopeAsync(XmlReader reader)
        {
            if (!await TryMoveToEnvelopeAsync(reader).ConfigureAwait(false))
                throw new InvalidQueryException($"Element `{EnvelopeName}` is missing from message content.");
        }

        public async Task MoveToBodyAsync(XmlReader reader)
        {
            await MoveToEnvelopeAsync(reader).ConfigureAwait(false);

            if (!await TryMoveToBodyAsync(reader).ConfigureAwait(false))
                throw new InvalidQueryException($"Element `{BodyName}` is missing from message content.");
        }

        public async Task MoveToPayloadAsync(XmlReader reader, XName payloadName)
        {
            await MoveToBodyAsync(reader).ConfigureAwait(false);

            if (!await reader.MoveToElementAsync(2, payloadName).ConfigureAwait(false))
                throw new InvalidQueryException($"Payload element `{payloadName}` is missing from message content.");
        }

        public Task<bool> TryMoveToEnvelopeAsync(XmlReader reader)
        {
            return reader.MoveToElementAsync(0, EnvelopeName);
        }

        public async Task<bool> TryMoveToHeaderAsync(XmlReader reader)
        {
            return await reader.MoveToElementAsync(1).ConfigureAwait(false) && reader.IsCurrentElement(1, HeaderName);
        }

        public Task<bool> TryMoveToBodyAsync(XmlReader reader)
        {
            return reader.MoveToElementAsync(1, BodyName);
        }

        public Task WriteStartEnvelopeAsync(XmlWriter writer, string prefix = null)
        {
            var prefixValue = string.IsNullOrEmpty(prefix) ? PrefixConstants.Soap12Env : prefix;
            return writer.WriteStartElementAsync(prefixValue, EnvelopeName.LocalName, EnvelopeName.NamespaceName);
        }

        public Task WriteStartBodyAsync(XmlWriter writer)
        {
            return writer.WriteStartElementAsync(BodyName);
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

        public async Task WriteSoapFaultAsync(XmlWriter writer, IFault fault)
        {
            var soapFault = (ISoap12Fault)fault;

            await writer.WriteStartDocumentAsync().ConfigureAwait(false);

            await WriteStartEnvelopeAsync(writer).ConfigureAwait(false);
            await writer.WriteAttributeStringAsync(PrefixConstants.Xmlns, PrefixConstants.Soap12Env, NamespaceConstants.Xmlns, NamespaceConstants.Soap12Env).ConfigureAwait(false);

            await WriteStartBodyAsync(writer).ConfigureAwait(false);
            await writer.WriteStartElementAsync(null, "Fault", NamespaceConstants.Soap12Env).ConfigureAwait(false);

            await WriteSoapFaultCodeAsync(writer, soapFault.Code).ConfigureAwait(false);
            await WriteSoapFaultReasonAsync(writer, soapFault.Reason).ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(soapFault.Node))
            {
                await writer.WriteStartElementAsync(FaultNodeName).ConfigureAwait(false);
                await writer.WriteStringAsync(soapFault.Node).ConfigureAwait(false);
                await writer.WriteEndElementAsync().ConfigureAwait(false);
            }

            if (!string.IsNullOrWhiteSpace(soapFault.Role))
            {
                await writer.WriteStartElementAsync(FaultRoleName).ConfigureAwait(false);
                await writer.WriteStringAsync(soapFault.Role).ConfigureAwait(false);
                await writer.WriteEndElementAsync().ConfigureAwait(false);
            }

            if (!string.IsNullOrWhiteSpace(soapFault.Detail))
            {
                await writer.WriteStartElementAsync(FaultDetailName).ConfigureAwait(false);
                await writer.WriteStringAsync(soapFault.Detail).ConfigureAwait(false);
                await writer.WriteEndElementAsync().ConfigureAwait(false);
            }

            await writer.WriteEndElementAsync().ConfigureAwait(false);
            await writer.WriteEndElementAsync().ConfigureAwait(false);
            await writer.WriteEndElementAsync().ConfigureAwait(false);

            await writer.FlushAsync().ConfigureAwait(false);
        }

        private static async Task WriteSoapFaultCodeAsync(XmlWriter writer, Soap12FaultCode faultCode)
        {
            await writer.WriteStartElementAsync(FaultCodeName).ConfigureAwait(false);

            await writer.WriteStartElementAsync(FaultCodeValueName).ConfigureAwait(false);
            await WriteSoapFaultValueEnumAsync(writer, faultCode.Value).ConfigureAwait(false);
            await writer.WriteEndElementAsync().ConfigureAwait(false);

            await WriteSoapFaultSubcodeAsync(writer, faultCode.Subcode).ConfigureAwait(false);

            await writer.WriteEndElementAsync().ConfigureAwait(false);
        }

        private static async Task WriteSoapFaultSubcodeAsync(XmlWriter writer, Soap12FaultSubcode subcode)
        {
            if (subcode == null)
                return;

            await writer.WriteStartElementAsync(null, "Subcode", NamespaceConstants.Soap12Env).ConfigureAwait(false);

            await writer.WriteStartElementAsync(null, "Value", NamespaceConstants.Soap12Env).ConfigureAwait(false);
            await writer.WriteStringAsync(subcode.Value).ConfigureAwait(false);
            await writer.WriteEndElementAsync().ConfigureAwait(false);

            await WriteSoapFaultSubcodeAsync(writer, subcode.Subcode).ConfigureAwait(false);

            await writer.WriteEndElementAsync().ConfigureAwait(false);
        }

        private static Task WriteSoapFaultValueEnumAsync(XmlWriter writer, Soap12FaultCodeEnum value)
        {
            switch (value)
            {
                case Soap12FaultCodeEnum.Receiver:
                    return writer.WriteQualifiedNameAsync("Receiver", NamespaceConstants.Soap12Env);

                case Soap12FaultCodeEnum.Sender:
                    return writer.WriteQualifiedNameAsync("Sender", NamespaceConstants.Soap12Env);

                case Soap12FaultCodeEnum.MustUnderstand:
                    return writer.WriteQualifiedNameAsync("MustUnderstand", NamespaceConstants.Soap12Env);

                case Soap12FaultCodeEnum.VersionMismatch:
                    return writer.WriteQualifiedNameAsync("VersionMismatch", NamespaceConstants.Soap12Env);

                case Soap12FaultCodeEnum.DataEncodingUnknown:
                    return writer.WriteQualifiedNameAsync("DataEncodingUnknown", NamespaceConstants.Soap12Env);

                default:
                    throw new ArgumentException($"Invalid SOAP 1.2 Fault Code enumeration value `{value}`.", nameof(value));
            }
        }

        private static async Task WriteSoapFaultReasonAsync(XmlWriter writer, IList<Soap12FaultReasonText> faultReasons)
        {
            await writer.WriteStartElementAsync(FaultReasonName).ConfigureAwait(false);

            foreach (var text in faultReasons)
            {
                await writer.WriteStartElementAsync(FaultReasonTextName).ConfigureAwait(false);
                await writer.WriteAttributeStringAsync("xml", "lang", NamespaceConstants.Xml, text.LanguageCode).ConfigureAwait(false);
                await writer.WriteStringAsync(text.Text).ConfigureAwait(false);
                await writer.WriteEndElementAsync().ConfigureAwait(false);
            }

            await writer.WriteEndElementAsync().ConfigureAwait(false);
        }

        public async Task WriteSoapHeaderAsync(XmlWriter writer, Style style, ISoapHeader header, IHeaderDefinition definition, IEnumerable<XElement> additionalHeaders = null)
        {
            if (header == null)
                return;

            await writer.WriteStartElementAsync(null, "Header", NamespaceConstants.Soap12Env).ConfigureAwait(false);

            await header.WriteToAsync(writer, style, definition).ConfigureAwait(false);

            foreach (var additionalHeader in additionalHeaders ?? Enumerable.Empty<XElement>())
                additionalHeader.WriteTo(writer);

            await writer.WriteEndElementAsync().ConfigureAwait(false);
        }

        public async Task ThrowSoapFaultIfPresentAsync(XmlReader reader)
        {
            if (reader.NamespaceURI == NamespaceConstants.Soap12Env && reader.LocalName == "Fault")
                throw new Soap12FaultException(await DeserializeSoapFaultAsync(reader).ConfigureAwait(false));
        }

        private static async Task<ISoap12Fault> DeserializeSoapFaultAsync(XmlReader reader)
        {
            const int depth = 3;

            var fault = new Soap12Fault();

            if (reader.IsEmptyElement || !await reader.MoveToElementAsync(depth, FaultCodeName).ConfigureAwait(false))
                throw new InvalidQueryException($"SOAP 1.2 Fault must have `{FaultCodeName}` element.");
            fault.Code = await DeserializeSoapFaultCodeAsync(reader).ConfigureAwait(false);

            if (!await reader.MoveToElementAsync(depth, FaultReasonName).ConfigureAwait(false))
                throw new InvalidQueryException($"SOAP 1.2 Fault must have `{FaultReasonName}` element.");
            fault.Reason = await DeserializeSoapFaultReasonAsync(reader).ConfigureAwait(false);

            var success = await reader.MoveToElementAsync(depth).ConfigureAwait(false);
            if (success && reader.IsCurrentElement(depth, FaultNodeName))
            {
                fault.Node = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                success = await reader.MoveToElementAsync(depth).ConfigureAwait(false);
            }

            if (success && reader.IsCurrentElement(depth, FaultRoleName))
            {
                fault.Role = await reader.ReadInnerXmlAsync().ConfigureAwait(false);
                success = await reader.MoveToElementAsync(depth).ConfigureAwait(false);
            }

            if (success && reader.IsCurrentElement(depth, FaultDetailName))
            {
                fault.Detail = await reader.ReadInnerXmlAsync().ConfigureAwait(false);
                success = await reader.MoveToElementAsync(depth).ConfigureAwait(false);
            }

            if (success)
                throw new InvalidQueryException($"Unexpected element `{reader.GetXName()}` in SOAP 1.2 Fault element.");

            return fault;
        }

        private static async Task<Soap12FaultCode> DeserializeSoapFaultCodeAsync(XmlReader reader)
        {
            const int depth = 4;

            var faultCode = new Soap12FaultCode();

            if (reader.IsEmptyElement || !await reader.MoveToElementAsync(depth, FaultCodeValueName).ConfigureAwait(false))
                throw new InvalidQueryException($"SOAP 1.2 Fault Code must have `{FaultCodeValueName}` element.");
            faultCode.Value = DeserializeFaultCodeValue(reader);

            var success = await reader.MoveToElementAsync(depth).ConfigureAwait(false);
            if (success && reader.IsCurrentElement(depth, FaultCodeSubcodeName))
            {
                faultCode.Subcode = await DeserializeFaultCodeSubcodeAsync(reader, depth + 1).ConfigureAwait(false);
                success = await reader.MoveToElementAsync(depth).ConfigureAwait(false);
            }

            if (success)
                throw new InvalidQueryException($"Unexpected element `{reader.GetXName()}` in SOAP 1.2 Fault Code element.");

            return faultCode;
        }

        private static async Task<Soap12FaultSubcode> DeserializeFaultCodeSubcodeAsync(XmlReader reader, int depth)
        {
            var faultSubcode = new Soap12FaultSubcode();

            if (reader.IsEmptyElement || !await reader.MoveToElementAsync(depth, FaultCodeValueName).ConfigureAwait(false))
                throw new InvalidQueryException($"SOAP 1.2 Fault Subcode must have `{FaultCodeValueName}` element.");
            faultSubcode.Value = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);

            var success = await reader.MoveToElementAsync(depth).ConfigureAwait(false);
            if (success && reader.IsCurrentElement(depth, FaultCodeSubcodeName))
            {
                faultSubcode.Subcode = await DeserializeFaultCodeSubcodeAsync(reader, depth + 1).ConfigureAwait(false);
                success = await reader.MoveToElementAsync(depth).ConfigureAwait(false);
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

        private static async Task<IList<Soap12FaultReasonText>> DeserializeSoapFaultReasonAsync(XmlReader reader)
        {
            const int depth = 4;

            var faultReasons = new List<Soap12FaultReasonText>();

            if (reader.IsEmptyElement || !await reader.MoveToElementAsync(depth, FaultReasonTextName).ConfigureAwait(false))
                throw new InvalidQueryException($"SOAP 1.2 Fault Reason must have `{FaultReasonTextName}` element.");

            do
            {
                faultReasons.Add(new Soap12FaultReasonText
                {
                    LanguageCode = reader.GetAttribute("lang", NamespaceConstants.Xml),
                    Text = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false)
                });
            } while (await reader.MoveToElementAsync(depth, FaultReasonTextName).ConfigureAwait(false));

            if (reader.Depth > 3)
                throw new InvalidQueryException($"Unexpected element `{reader.GetXName()}` in SOAP 1.2 Fault Reason element.");

            return faultReasons;
        }
    }
}
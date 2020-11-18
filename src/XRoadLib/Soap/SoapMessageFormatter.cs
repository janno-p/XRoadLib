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
    public class SoapMessageFormatter : IMessageFormatter
    {
        private static readonly XName EnvelopeName = XName.Get("Envelope", NamespaceConstants.SoapEnv);
        private static readonly XName HeaderName = XName.Get("Header", NamespaceConstants.SoapEnv);
        private static readonly XName BodyName = XName.Get("Body", NamespaceConstants.SoapEnv);

        public string ContentType { get; } = ContentTypes.Soap;
        public string Namespace { get; } = NamespaceConstants.SoapEnv;

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
            var prefixValue = string.IsNullOrEmpty(prefix) ? PrefixConstants.SoapEnv : prefix;
            return writer.WriteStartElementAsync(prefixValue, EnvelopeName.LocalName, EnvelopeName.NamespaceName);
        }

        public Task WriteStartBodyAsync(XmlWriter writer)
        {
            return writer.WriteStartElementAsync(BodyName);
        }

        public IFault CreateFault(Exception exception)
        {
            return new SoapFault
            {
                FaultCode = ((exception as XRoadException)?.FaultCode ?? ServerFaultCode.InternalError).Value,
                FaultString = exception?.Message ?? string.Empty
            };
        }

        public async Task WriteSoapFaultAsync(XmlWriter writer, IFault fault)
        {
            var soapFault = (ISoapFault)fault;

            await writer.WriteStartDocumentAsync().ConfigureAwait(false);

            await WriteStartEnvelopeAsync(writer).ConfigureAwait(false);
            await writer.WriteAttributeStringAsync(PrefixConstants.Xmlns, PrefixConstants.SoapEnv, NamespaceConstants.Xmlns, NamespaceConstants.SoapEnv).ConfigureAwait(false);

            await WriteStartBodyAsync(writer).ConfigureAwait(false);
            await writer.WriteStartElementAsync(null, "Fault", NamespaceConstants.SoapEnv).ConfigureAwait(false);

            await writer.WriteStartElementAsync("faultcode").ConfigureAwait(false);
            await writer.WriteStringAsync(soapFault.FaultCode).ConfigureAwait(false);
            await writer.WriteEndElementAsync().ConfigureAwait(false);

            await writer.WriteStartElementAsync("faultstring").ConfigureAwait(false);
            await writer.WriteStringAsync(soapFault.FaultString).ConfigureAwait(false);
            await writer.WriteEndElementAsync().ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(soapFault.FaultActor))
            {
                await writer.WriteStartElementAsync("faultactor").ConfigureAwait(false);
                await writer.WriteStringAsync(soapFault.FaultActor).ConfigureAwait(false);
                await writer.WriteEndElementAsync().ConfigureAwait(false);
            }

            if (!string.IsNullOrWhiteSpace(soapFault.Details))
            {
                await writer.WriteStartElementAsync("detail").ConfigureAwait(false);
                await writer.WriteStringAsync(soapFault.Details).ConfigureAwait(false);
                await writer.WriteEndElementAsync().ConfigureAwait(false);
            }

            await writer.WriteEndElementAsync().ConfigureAwait(false);
            await writer.WriteEndElementAsync().ConfigureAwait(false);
            await writer.WriteEndElementAsync().ConfigureAwait(false);

            await writer.FlushAsync().ConfigureAwait(false);
        }

        public async Task WriteSoapHeaderAsync(XmlWriter writer, Style style, ISoapHeader header, HeaderDefinition definition, IEnumerable<XElement> additionalHeaders = null)
        {
            if (header == null)
                return;

            await writer.WriteStartElementAsync(null, "Header", NamespaceConstants.SoapEnv).ConfigureAwait(false);

            await header.WriteToAsync(writer, style, definition).ConfigureAwait(false);

            foreach (var additionalHeader in additionalHeaders ?? Enumerable.Empty<XElement>())
                additionalHeader.WriteTo(writer);

            await writer.WriteEndElementAsync().ConfigureAwait(false);
        }

        public async Task ThrowSoapFaultIfPresentAsync(XmlReader reader)
        {
            if (reader.NamespaceURI == NamespaceConstants.SoapEnv && reader.LocalName == "Fault")
                throw new SoapFaultException(await DeserializeSoapFaultAsync(reader).ConfigureAwait(false));
        }

        private static async Task<ISoapFault> DeserializeSoapFaultAsync(XmlReader reader)
        {
            const int depth = 3;

            var fault = new SoapFault();

            if (reader.IsEmptyElement || !await reader.MoveToElementAsync(depth, "faultcode").ConfigureAwait(false))
                throw new InvalidQueryException("SOAP Fault must have `faultcode` element.");
            fault.FaultCode = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);

            if (!await reader.MoveToElementAsync(depth, "faultstring").ConfigureAwait(false))
                throw new InvalidQueryException("SOAP Fault must have `faultstring` element.");
            fault.FaultString = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);

            var success = await reader.MoveToElementAsync(depth).ConfigureAwait(false);
            if (success && reader.LocalName == "faultactor" && reader.NamespaceURI == "")
            {
                fault.FaultActor = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                success = await reader.MoveToElementAsync(depth).ConfigureAwait(false);
            }

            if (success && reader.LocalName == "detail" && reader.NamespaceURI == "")
            {
                fault.Details = await reader.ReadInnerXmlAsync().ConfigureAwait(false);
                success = await reader.MoveToElementAsync(depth).ConfigureAwait(false);
            }

            if (success)
                throw new InvalidQueryException($"Unexpected element `{reader.GetXName()}` in SOAP Fault element.");

            return fault;
        }
    }
}
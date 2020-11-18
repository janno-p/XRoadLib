using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using XRoadLib.Headers;
using XRoadLib.Schema;
using XRoadLib.Styles;

namespace XRoadLib.Soap
{
    public interface IMessageFormatter
    {
        string ContentType { get; }
        string Namespace { get; }

        Task MoveToEnvelopeAsync(XmlReader reader);
        Task MoveToBodyAsync(XmlReader reader);
        Task MoveToPayloadAsync(XmlReader reader, XName payloadName);

        Task<bool> TryMoveToEnvelopeAsync(XmlReader reader);
        Task<bool> TryMoveToHeaderAsync(XmlReader reader);
        Task<bool> TryMoveToBodyAsync(XmlReader reader);

        Task WriteStartEnvelopeAsync(XmlWriter writer, string prefix = null);
        Task WriteStartBodyAsync(XmlWriter writer);

        Task WriteSoapFaultAsync(XmlWriter writer, IFault fault);
        Task WriteSoapHeaderAsync(XmlWriter writer, Style style, ISoapHeader header, HeaderDefinition definition, IEnumerable<XElement> additionalHeaders = null);

        Task ThrowSoapFaultIfPresentAsync(XmlReader reader);

        IFault CreateFault(Exception exception);
    }
}
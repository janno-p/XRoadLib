using System;
using System.Collections.Generic;
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

        void MoveToEnvelope(XmlReader reader);
        void MoveToBody(XmlReader reader);
        void MoveToPayload(XmlReader reader, XName payloadName);

        bool TryMoveToEnvelope(XmlReader reader);
        bool TryMoveToHeader(XmlReader reader);
        bool TryMoveToBody(XmlReader reader);

        void WriteStartEnvelope(XmlWriter writer, string prefix = null);
        void WriteStartBody(XmlWriter writer);

        void WriteSoapFault(XmlWriter writer, IFault fault);
        void WriteSoapHeader(XmlWriter writer, Style style, ISoapHeader header, HeaderDefinition definition, IEnumerable<XElement> additionalHeaders = null);

        void ThrowSoapFaultIfPresent(XmlReader reader);

        IFault CreateFault(Exception exception);
    }
}
using System;
using XRoadLib.Protocols.Styles;
using XRoadLib.Schema;

namespace XRoadLib.Protocols
{
    public interface IXRoadLegacyProtocol
    {
        string XRoadNamespace { get; }
    }

    public abstract class XRoadLegacyProtocol : XRoadProtocol, IXRoadLegacyProtocol
    {
        public string ProducerName { get; }

        string IXRoadLegacyProtocol.XRoadNamespace => XRoadNamespace;

        protected XRoadLegacyProtocol(string producerName, string producerNamespace, Style style, ISchemaExporter schemaExporter)
            : base(producerNamespace, style, schemaExporter)
        {
            if (string.IsNullOrWhiteSpace(producerName))
                throw new ArgumentNullException(nameof(producerName));
            ProducerName = producerName;
        }
    }
}
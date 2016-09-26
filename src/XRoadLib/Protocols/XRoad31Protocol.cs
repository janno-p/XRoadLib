using XRoadLib.Protocols.Styles;
using XRoadLib.Schema;

namespace XRoadLib.Protocols
{
    public class XRoad31Protocol : XRoadProtocol
    {
        public override string Name => "3.1";

        public XRoad31Protocol(string producerNamespace, Style style = null, ISchemaExporter schemaExporter = null)
            : base(producerNamespace, style ?? new DocLiteralStyle(), schemaExporter)
        { }
    }
}
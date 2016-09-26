using XRoadLib.Protocols.Styles;
using XRoadLib.Schema;

namespace XRoadLib.Protocols
{
    public class XRoad40Protocol : XRoadProtocol
    {
        public override string Name => "4.0";

        public XRoad40Protocol(string producerNamespace, Style style = null, ISchemaExporter schemaExporter = null)
            : base(producerNamespace, style ?? new DocLiteralStyle(), schemaExporter)
        { }
    }
}
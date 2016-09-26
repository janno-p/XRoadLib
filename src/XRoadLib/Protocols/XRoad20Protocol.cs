using System.Xml;
using XRoadLib.Protocols.Styles;
using XRoadLib.Schema;

namespace XRoadLib.Protocols
{
    public class XRoad20Protocol : XRoadProtocol
    {
        public override string Name => "2.0";

        public XRoad20Protocol(string producerNamespace, Style style = null, ISchemaExporter schemaExporter = null)
            : base(producerNamespace, style ?? new RpcEncodedStyle(), schemaExporter)
        { }

        internal override bool IsDefinedByEnvelope(XmlReader reader)
        {
            var attributeValue = reader.GetAttribute("encodingStyle", NamespaceConstants.SOAP_ENV);

            return NamespaceConstants.SOAP_ENC.Equals(attributeValue);
        }

        internal override void WriteSoapEnvelope(XmlWriter writer)
        {
            base.WriteSoapEnvelope(writer);

            writer.WriteAttributeString(PrefixConstants.XMLNS, PrefixConstants.SOAP_ENC, NamespaceConstants.XMLNS, NamespaceConstants.SOAP_ENC);
            writer.WriteAttributeString("encodingStyle", NamespaceConstants.SOAP_ENV, NamespaceConstants.SOAP_ENC);
        }
    }
}
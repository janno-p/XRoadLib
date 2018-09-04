using System.Reflection;
using System.Xml.Linq;
using XRoadLib.Headers;
using XRoadLib.Styles;

namespace XRoadLib.Schema
{
    /// <inheritdoc />
    public class SchemaExporterXRoad20 : SchemaExporterXRoadLegacy
    {
        /// <inheritdoc />
        public override string XRoadPrefix => PrefixConstants.XTEE;

        /// <inheritdoc />
        public override string XRoadNamespace => NamespaceConstants.XTEE;

        /// <inheritdoc />
        public SchemaExporterXRoad20(string producerName, Assembly contractAssembly, string producerNamespace = null)
            : base(producerName, contractAssembly, producerNamespace ?? $"http://producers.{producerName}.xtee.riik.ee/producer/{producerName}")
        { }

        /// <inheritdoc />
        public override void ExportRequestDefinition(RequestDefinition requestDefinition)
        {
            base.ExportRequestDefinition(requestDefinition);

            requestDefinition.Content.Name = XName.Get("keha");
        }

        /// <inheritdoc />
        public override void ExportResponseDefinition(ResponseDefinition responseDefinition)
        {
            base.ExportResponseDefinition(responseDefinition);

            responseDefinition.RequestContentName = XName.Get("paring");
            responseDefinition.Content.Name = XName.Get("keha");
        }

        /// <inheritdoc />
        public override void ExportHeaderDefinition(HeaderDefinition headerDefinition)
        {
            base.ExportHeaderDefinition(headerDefinition);

            headerDefinition.Use<XRoadHeader20>()
                            .WithRequiredHeader(x => x.Asutus)
                            .WithRequiredHeader(x => x.Andmekogu)
                            .WithRequiredHeader(x => x.Nimi)
                            .WithRequiredHeader(x => x.Isikukood)
                            .WithRequiredHeader(x => x.Id)
                            .WithRequiredHeader(x => x.AmetnikNimi)
                            .WithHeaderNamespace(NamespaceConstants.XTEE);
        }

        /// <summary>
        /// Configure protocol global settings.
        /// </summary>
        public override void ExportProtocolDefinition(ProtocolDefinition protocolDefinition)
        {
            base.ExportProtocolDefinition(protocolDefinition);

            protocolDefinition.DetectEnvelope = reader => NamespaceConstants.SOAP_ENC.Equals(reader.GetAttribute("encodingStyle", NamespaceConstants.SOAP_ENV));
            protocolDefinition.Style = new RpcEncodedStyle();

            protocolDefinition.GlobalNamespacePrefixes.Add(XNamespace.Get(NamespaceConstants.SOAP_ENC), PrefixConstants.SOAP_ENC);
        }
    }
}
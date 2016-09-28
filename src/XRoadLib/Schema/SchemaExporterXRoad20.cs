using System.Reflection;
using System.Web.Services.Description;
using XRoadLib.Headers;
using XRoadLib.Styles;

namespace XRoadLib.Schema
{
    /// <summary>
    /// Schema exporter for X-Road message protocol version 2.0.
    /// </summary>
    public class SchemaExporterXRoad20 : SchemaExporterXRoadLegacy
    {
        /// <summary>
        /// Preferred X-Road namespace prefix of the message protocol version.
        /// </summary>
        public override string XRoadPrefix => PrefixConstants.XTEE;

        /// <summary>
        /// X-Road specification namespace of the message protocol version.
        /// </summary>
        public override string XRoadNamespace => NamespaceConstants.XTEE;

        /// <summary>
        /// Initializes schema exporter for X-Road message protocol version 2.0.
        /// </summary>
        public SchemaExporterXRoad20(string producerName, Assembly contractAssembly, string producerNamespace = null)
            : base(producerName, contractAssembly, producerNamespace ?? $"http://producers.{producerName}.xtee.riik.ee/producer/{producerName}")
        { }

        /// <summary>
        /// Configure request elements of X-Road message protocol version 2.0 messages.
        /// </summary>
        public override void ExportRequestValueDefinition(RequestValueDefinition requestValueDefinition)
        {
            base.ExportRequestValueDefinition(requestValueDefinition);

            requestValueDefinition.RequestElementName = "keha";
        }

        /// <summary>
        /// Configure response elements of X-Road message protocol version 2.0 messages.
        /// </summary>
        public override void ExportResponseValueDefinition(ResponseValueDefinition responseValueDefinition)
        {
            base.ExportResponseValueDefinition(responseValueDefinition);

            responseValueDefinition.RequestElementName = "paring";
            responseValueDefinition.ResponseElementName = "keha";
        }

        /// <summary>
        /// Configure SOAP header of the messages.
        /// </summary>
        public override void ExportHeaderDefinition(HeaderDefinition headerDefinition)
        {
            base.ExportHeaderDefinition(headerDefinition);

            headerDefinition.Use(() => new XRoadHeader20(headerDefinition, new RpcEncodedStyle()))
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
        }
    }
}
using System.Web.Services.Description;

namespace XRoadLib.Schema
{
    /// <summary>
    /// Helper base class for various schema exporters.
    /// </summary>
    public abstract class SchemaExporterBase : ISchemaExporter
    {
        /// <summary>
        /// Preferred X-Road namespace prefix of the message protocol version.
        /// </summary>
        public abstract string XRoadPrefix { get; }

        /// <summary>
        /// X-Road specification namespace of the message protocol version.
        /// </summary>
        public abstract string XRoadNamespace { get; }

        /// <summary>
        /// Configuration hook for overriding default operation settings.
        /// </summary>
        public virtual void ExportOperationDefinition(OperationDefinition operationDefinition) { }

        /// <summary>
        /// Configuration hook for overriding default property settings.
        /// </summary>
        public virtual void ExportPropertyDefinition(PropertyDefinition propertyDefinition) { }

        /// <summary>
        /// Configuration hook for overriding default type settings.
        /// </summary>
        public virtual void ExportTypeDefinition(TypeDefinition typeDefinition) { }

        /// <summary>
        /// Configuration hook for overriding default response element settings.
        /// </summary>
        public virtual void ExportResponseValueDefinition(ResponseValueDefinition responseValueDefinition) { }

        /// <summary>
        /// Configuration hook for overriding default request element settings.
        /// </summary>
        public virtual void ExportRequestValueDefinition(RequestValueDefinition requestValueDefinition) { }

        /// <summary>
        /// Configuration hook for overriding default non-technical fault settings.
        /// Provide your own override to alter default behavior.
        /// </summary>
        public virtual void ExportFaultDefinition(FaultDefinition faultDefinition) { }

        /// <summary>
        /// Provide schema location of specified schema. When no location is provided, system schemas
        /// will be assigned to default location and other schemas will be explicitly defined
        /// in service description.
        /// </summary>
        public virtual string ExportSchemaLocation(string namespaceName) => null;

        /// <summary>
        /// Allows each message protocol implementation to customize service description document
        /// before publishing.
        /// </summary>
        public virtual void ExportServiceDescription(ServiceDescription serviceDescription) { }

        /// <summary>
        /// Configure SOAP header of the messages.
        /// </summary>
        public virtual void ExportHeaderDefinition(HeaderDefinition headerDefinition) { }

        /// <summary>
        /// Configure protocol global settings.
        /// </summary>
        public virtual void ExportProtocolDefinition(ProtocolDefinition protocolDefinition) { }
    }
}
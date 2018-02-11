using System.Web.Services.Description;

namespace XRoadLib.Schema
{
    /// <summary>
    /// Provides configuration hooks for customizing contract serialization
    /// and definitions.
    /// </summary>
    public interface ISchemaExporter
    {
        /// <summary>
        /// Preferred X-Road namespace prefix of the message protocol version.
        /// </summary>
        string XRoadPrefix { get; }

        /// <summary>
        /// X-Road specification namespace of the message protocol version.
        /// </summary>
        string XRoadNamespace { get; }

        /// <summary>
        /// Configuration hook for overriding default operation settings.
        /// </summary>
        void ExportOperationDefinition(OperationDefinition operationDefinition);

        /// <summary>
        /// Configuration hook for overriding default property settings.
        /// </summary>
        void ExportPropertyDefinition(PropertyDefinition propertyDefinition);

        /// <summary>
        /// Configuration hook for overriding default type settings.
        /// </summary>
        void ExportTypeDefinition(TypeDefinition typeDefinition);

        /// <summary>
        /// Configuration hook for overriding default response element settings.
        /// </summary>
        void ExportResponseDefinition(ResponseDefinition responseDefinition);

        /// <summary>
        /// Configuration hook for overriding default request element settings.
        /// </summary>
        void ExportRequestDefinition(RequestDefinition requestDefinition);

        /// <summary>
        /// Configuration hook for overriding default non-technical fault settings.
        /// </summary>
        void ExportFaultDefinition(FaultDefinition faultDefinition);

        /// <summary>
        /// Provide custom schema locations.
        /// </summary>
        string ExportSchemaLocation(string namespaceName);

        /// <summary>
        /// Allows each message protocol implementation to customize service description document
        /// before publishing.
        /// </summary>
        void ExportServiceDescription(ServiceDescription serviceDescription);

        /// <summary>
        /// Customize X-Road message header elements.
        /// </summary>
        void ExportHeaderDefinition(HeaderDefinition headerDefinition);

        /// <summary>
        /// Configure protocol global settings.
        /// </summary>
        void ExportProtocolDefinition(ProtocolDefinition protocolDefinition);
    }
}
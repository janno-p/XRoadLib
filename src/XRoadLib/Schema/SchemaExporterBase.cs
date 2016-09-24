using System.Web.Services.Description;

namespace XRoadLib.Schema
{
    public abstract class SchemaExporterBase : ISchemaExporter
    {
        public virtual void ExportOperationDefinition(OperationDefinition operationDefinition) { }

        public virtual void ExportPropertyDefinition(PropertyDefinition propertyDefinition) { }

        public virtual void ExportTypeDefinition(TypeDefinition typeDefinition) { }

        public virtual void ExportResponseValueDefinition(ResponseValueDefinition responseValueDefinition) { }

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
    }
}
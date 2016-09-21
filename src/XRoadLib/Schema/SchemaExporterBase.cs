﻿namespace XRoadLib.Schema
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
    }
}
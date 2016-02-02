namespace XRoadLib.Schema
{
    public abstract class SchemaExporterBase : ISchemaExporter
    {
        public virtual void ExportOperationTypeDefinition(OperationTypeDefinition operationTypeDefinition) { }

        public virtual void ExportOperationDefinition(OperationDefinition operationDefinition) { }

        public virtual void ExportParameterDefinition(ParameterDefinition parameterDefinition) { }

        public virtual void ExportPropertyDefinition(PropertyDefinition propertyDefinition) { }

        public virtual void ExportTypeDefinition(TypeDefinition typeDefinition) { }
    }
}
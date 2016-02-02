namespace XRoadLib.Schema
{
    public abstract class SchemaExporterBase : ISchemaExporter
    {
        public virtual void ExportOperationDefinition(OperationDefinition operation) { }

        public virtual void ExportParameterDefinition(ParameterDefinition parameter) { }

        public virtual void ExportPropertyDefinition(PropertyDefinition propertyDefinition) { }

        public virtual void ExportTypeDefinition(TypeDefinition typeDefinition) { }
    }
}
namespace XRoadLib.Schema
{
    public abstract class SchemaExporterBase : ISchemaExporter
    {
        public virtual void ExportOperationDefinition(OperationDefinition operationDefinition) { }

        public virtual void ExportPropertyDefinition(PropertyDefinition propertyDefinition) { }

        public virtual void ExportTypeDefinition(TypeDefinition typeDefinition) { }
    }
}
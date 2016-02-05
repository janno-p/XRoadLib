namespace XRoadLib.Schema
{
    public interface ISchemaExporter
    {
        void ExportOperationDefinition(OperationDefinition operationDefinition);

        void ExportPropertyDefinition(PropertyDefinition propertyDefinition);

        void ExportTypeDefinition(TypeDefinition typeDefinition);

        void ExportResponseValueDefinition(ResponseValueDefinition responseValueDefinition);
    }
}
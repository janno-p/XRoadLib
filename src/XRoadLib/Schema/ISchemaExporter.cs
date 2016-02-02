namespace XRoadLib.Schema
{
    public interface ISchemaExporter
    {
        void ExportOperationDefinition(OperationDefinition operation);

        void ExportParameterDefinition(ParameterDefinition parameter);

        void ExportPropertyDefinition(PropertyDefinition propertyDefinition);

        void ExportTypeDefinition(TypeDefinition typeDefinition);
    }
}

namespace XRoadLib.Schema
{
    public interface ISchemaExporter
    {
        void ExportOperationTypeDefinition(OperationTypeDefinition operationTypeDefinition);

        void ExportOperationDefinition(OperationDefinition operationDefinition);

        void ExportParameterDefinition(ParameterDefinition parameterDefinition);

        void ExportPropertyDefinition(PropertyDefinition propertyDefinition);

        void ExportTypeDefinition(TypeDefinition typeDefinition);
    }
}
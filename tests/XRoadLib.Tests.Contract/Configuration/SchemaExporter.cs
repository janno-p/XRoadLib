using XRoadLib.Schema;

namespace XRoadLib.Tests.Contract.Configuration
{
    public class SchemaExporter : ISchemaExporter
    {
        public void ExportOperationDefinition(OperationDefinition operationDefinition)
        {
            // Customize operation message names:
            operationDefinition.InputMessageName = operationDefinition.Name.LocalName;
            operationDefinition.OutputMessageName = $"{operationDefinition.Name.LocalName}Response";
        }

        public void ExportTypeDefinition(TypeDefinition typeDefinition)
        {
            // Customize type content model:
            if (typeDefinition.Type == typeof(ParamType1))
                typeDefinition.HasStrictContentOrder = false;
        }

        public void ExportPropertyDefinition(PropertyDefinition propertyDefinition)
        {
            propertyDefinition.UseXop = false;
        }
    }
}

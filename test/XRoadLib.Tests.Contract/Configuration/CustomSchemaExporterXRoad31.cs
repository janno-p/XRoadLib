using XRoadLib.Schema;

namespace XRoadLib.Tests.Contract.Configuration
{
    public class CustomSchemaExporterXRoad31 : SchemaExporterXRoad31
    {
        public override void ExportOperationDefinition(OperationDefinition operationDefinition)
        {
            // Customize operation message names:
            operationDefinition.InputMessageName = operationDefinition.Name.LocalName;
            operationDefinition.OutputMessageName = $"{operationDefinition.Name.LocalName}Response";
        }

        public override void ExportTypeDefinition(TypeDefinition typeDefinition)
        {
            // Customize type content model:
            if (typeDefinition.Type == typeof(ParamType1))
                typeDefinition.HasStrictContentOrder = false;
        }

        public override void ExportPropertyDefinition(PropertyDefinition propertyDefinition)
        {
            propertyDefinition.UseXop = false;
        }
    }
}

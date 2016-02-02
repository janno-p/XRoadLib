using System.Xml.Linq;
using XRoadLib.Schema;

namespace XRoadLib.Tests.Contract.Configuration
{
    public class SchemaExporter : ISchemaExporter
    {
        public void ExportOperationDefinition(OperationDefinition operationDefinition)
        {
            // Customize root type name for operations (when applicable):
            operationDefinition.RequestTypeName = XName.Get($"{operationDefinition.Name.LocalName}Request", operationDefinition.RequestTypeName.NamespaceName);
            operationDefinition.ResponseTypeName = XName.Get($"{operationDefinition.Name.LocalName}Response", operationDefinition.RequestTypeName.NamespaceName);

            // Customize operation message names:
            operationDefinition.RequestMessageName = operationDefinition.Name.LocalName;
            operationDefinition.ResponseMessageName = $"{operationDefinition.Name.LocalName}Response";
        }

        public void ExportTypeDefinition(TypeDefinition typeDefinition)
        {
            // Customize type content model:
            if (typeDefinition.Type == typeof(ParamType1))
                typeDefinition.HasStrictContentOrder = false;
        }

        public void ExportParameterDefinition(ParameterDefinition parameterDefinition)
        {
            parameterDefinition.IsOptional = true;
        }

        public void ExportPropertyDefinition(PropertyDefinition propertyDefinition)
        {
            propertyDefinition.UseXop = false;
        }
    }
}

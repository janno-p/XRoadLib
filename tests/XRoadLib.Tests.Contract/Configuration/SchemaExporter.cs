using System.Xml.Linq;
using XRoadLib.Schema;

namespace XRoadLib.Tests.Contract.Configuration
{
    public class SchemaExporter : ISchemaExporter
    {
        public void ExportOperationTypeDefinition(OperationTypeDefinition operationTypeDefinition)
        {
            var name = operationTypeDefinition.InputName;

            // Customize root type name for operations (when applicable):
            operationTypeDefinition.InputName = XName.Get($"{name.LocalName}Request", name.NamespaceName);
            operationTypeDefinition.OutputName = XName.Get($"{name.LocalName}Response", name.NamespaceName);
        }

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

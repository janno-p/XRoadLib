using System;
using System.Reflection;
using System.Xml.Linq;

namespace XRoadLib.Schema
{
    public interface ISchemaExporter
    {
        OperationDefinition GetOperationDefinition(MethodInfo methodInfo, XName qualifiedName);

        ParameterDefinition GetParameterDefinition(ParameterInfo parameterInfo, OperationDefinition operationDefinition);

        PropertyDefinition GetPropertyDefinition(PropertyInfo propertyInfo, TypeDefinition typeDefinition);

        TypeDefinition GetTypeDefinition(Type type);

        void ExportOperationDefinition(OperationDefinition operation);

        void ExportParameterDefinition(ParameterDefinition parameter);

        void ExportPropertyDefinition(PropertyDefinition propertyDefinition);

        void ExportTypeDefinition(TypeDefinition typeDefinition);
    }
}

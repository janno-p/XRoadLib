using System;
using System.Reflection;
using System.Xml.Linq;
using XRoadLib.Extensions;

namespace XRoadLib.Schema
{
    public static class MetaDataConverter
    {
        public static TypeDefinition ConvertType(Type type)
        {
            throw new NotImplementedException();
        }

        public static PropertyDefinition ConvertProperty(PropertyInfo propertyInfo, TypeDefinition ownerDefinition)
        {
            var definition = new PropertyDefinition(ownerDefinition);

            definition.Name = propertyInfo.GetElementName();
            if (definition.Name == null)
            {
                var start = propertyInfo.Name.LastIndexOf('.');
                definition.Name = XName.Get(start >= 0 ? propertyInfo.Name.Substring(start + 1) : propertyInfo.Name);
            }

            throw new NotImplementedException();

            return definition;
        }

        public static OperationDefinition ConvertOperation(MethodInfo type)
        {
            throw new NotImplementedException();
        }

        public static ParameterDefinition ConvertParameter(ParameterInfo parameterInfo, OperationDefinition ownerDefinition)
        {
            var definition = new ParameterDefinition(ownerDefinition);

            definition.Name = parameterInfo.GetElementName() ?? XName.Get(parameterInfo.Name.GetValueOrDefault("response"));

            throw new NotImplementedException();

            return definition;
        }
    }
}
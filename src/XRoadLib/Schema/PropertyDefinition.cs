using System.Reflection;

namespace XRoadLib.Schema
{
    public class PropertyDefinition : ContentDefinition
    {
        public TypeDefinition DeclaringTypeDefinition { get; }

        public PropertyInfo PropertyInfo { get; }

        public override string ContainerName => $"{PropertyInfo.DeclaringType?.FullName}";

        public override string RuntimeName
        {
            get
            {
                var startIndex = PropertyInfo.Name.LastIndexOf('.');
                return startIndex >= 0 ? PropertyInfo.Name.Substring(startIndex + 1) : PropertyInfo.Name;
            }
        }

        public PropertyDefinition(PropertyInfo propertyInfo, TypeDefinition declaringTypeDefinition)
        {
            DeclaringTypeDefinition = declaringTypeDefinition;
            PropertyInfo = propertyInfo;
        }
    }
}
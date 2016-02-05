using System.Reflection;

namespace XRoadLib.Schema
{
    public class PropertyDefinition : ContentDefinition
    {
        public TypeDefinition DeclaringTypeDefinition { get; }

        public PropertyInfo PropertyInfo { get; }

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
            RuntimeType = NormalizeType(propertyInfo.PropertyType);

            InitializeContentDefinition(propertyInfo);
        }

        public override string ToString()
        {
            return $"Return value of {PropertyInfo.DeclaringType?.FullName ?? "<null>"}.{RuntimeName} ({Name})";
        }
    }
}
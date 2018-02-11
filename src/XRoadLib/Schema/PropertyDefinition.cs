using System.Reflection;
using XRoadLib.Extensions;

namespace XRoadLib.Schema
{
    public class PropertyDefinition : ParticleDefinition
    {
        public TypeDefinition DeclaringTypeDefinition { get; }
        public PropertyInfo PropertyInfo { get; }
        public string TemplateName { get; set; }

        public PropertyDefinition(PropertyInfo propertyInfo, TypeDefinition declaringTypeDefinition)
        {
            DeclaringTypeDefinition = declaringTypeDefinition;
            PropertyInfo = propertyInfo;

            Content = new ContentDefinition(
                this,
                propertyInfo,
                propertyInfo.PropertyType.NormalizeType(),
                propertyInfo.GetRuntimeName()
            );

            TemplateName = Content.Name?.LocalName;
        }

        public override string ToString()
        {
            return $"Property `{PropertyInfo.GetRuntimeName()}` of type `{PropertyInfo.DeclaringType?.FullName ?? "<null>"}`";
        }
    }
}
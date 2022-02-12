using System.Reflection;
using XRoadLib.Extensions;

namespace XRoadLib.Schema;

public class PropertyDefinition : ParticleDefinition
{
    public TypeDefinition DeclaringTypeDefinition { get; }
    public PropertyInfo PropertyInfo { get; }
    public string RuntimeName { get; }

    [UsedImplicitly]
    public string TemplateName { get; set; }

    public PropertyDefinition(PropertyInfo propertyInfo, TypeDefinition declaringTypeDefinition, Func<string, bool> isQualifiedElementDefault)
    {
        DeclaringTypeDefinition = declaringTypeDefinition;
        PropertyInfo = propertyInfo;
        RuntimeName = propertyInfo.GetRuntimeName();

        Content = ContentDefinition.FromType(
            this,
            propertyInfo,
            propertyInfo.PropertyType.NormalizeType(),
            RuntimeName,
            declaringTypeDefinition.TargetNamespace,
            isQualifiedElementDefault(declaringTypeDefinition.TargetNamespace)
        );

        TemplateName = Content.Name?.LocalName;
    }

    public override string ToString()
    {
        return $"Property `{RuntimeName}` of type `{PropertyInfo.DeclaringType?.FullName ?? "<null>"}`";
    }
}
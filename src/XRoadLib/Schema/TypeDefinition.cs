using System.Reflection;

namespace XRoadLib.Schema;

public class TypeDefinition : Definition
{
    public Type Type { get; }

    public string TargetNamespace { get; }

    [UsedImplicitly]
    public bool CanHoldNullValues { get; set; }

    [UsedImplicitly]
    public bool IsAbstract { get; set; }

    public bool IsAnonymous { get; set; }

    public bool IsSimpleType { get; set; }

    [UsedImplicitly]
    public Type? TypeMapType { get; set; }

    public bool HasStrictContentOrder { get; set; }

    public IComparer<PropertyDefinition> ContentComparer { get; set; }

    public bool IsInheritable => !IsAnonymous && !IsSimpleType;

    public bool IsCompositeType => !Type.GetTypeInfo().IsEnum && !Type.GetTypeInfo().IsAbstract;

    public TypeDefinition(Type type, string targetNamespace)
    {
        Documentation = new DocumentationDefinition(type.GetTypeInfo());
        TargetNamespace = targetNamespace;
        Type = type;
    }
}
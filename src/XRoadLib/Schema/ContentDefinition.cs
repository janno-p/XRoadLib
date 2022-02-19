using System.Reflection;
using System.Xml.Schema;
using XRoadLib.Serialization;

namespace XRoadLib.Schema;

public abstract class ContentDefinition : Definition
{
    public ParticleDefinition Particle { get; }

    [UsedImplicitly]
    public bool IgnoreExplicitType { get; set; }

    [UsedImplicitly]
    public bool MergeContent { get; set; }

    [UsedImplicitly]
    public bool IsNullable { get; set; }

    [UsedImplicitly]
    public EmptyTagHandlingMode? EmptyTagHandlingMode { get; set; }

    [UsedImplicitly]
    public virtual bool IsOptional { get; set; }

    public bool UseXop { get; set; }

    [UsedImplicitly]
    public int Order { get; set; } = -1;

    public XName? TypeName { get; set; }

    public Type RuntimeType { get; set; }

    public virtual XName? SerializedName => Name;

    protected ContentDefinition(ParticleDefinition particle)
    {
        Particle = particle;
    }

    public static ContentDefinition FromType(ParticleDefinition particle, ICustomAttributeProvider customAttributeProvider, Type runtimeType, string runtimeName, string targetNamespace, bool defaultQualifiedElement)
    {
        if (customAttributeProvider == null)
            return new EmptyContentDefinition(particle);

        if (runtimeType.IsArray)
            return new ArrayContentDefiniton(particle, customAttributeProvider, runtimeType, runtimeName, targetNamespace, defaultQualifiedElement);
            
        return new SingularContentDefinition(particle, customAttributeProvider, runtimeType, runtimeName, targetNamespace, defaultQualifiedElement);
    }

    public static string GetQualifiedNamespace(string elementNamespace, XmlSchemaForm? elementForm, string targetNamespace, bool defaultQualifiedElement) => elementForm.GetValueOrDefault() switch
    {
        XmlSchemaForm.Qualified => string.IsNullOrEmpty(elementNamespace) ? targetNamespace : elementNamespace,
        XmlSchemaForm.Unqualified => string.Empty,
        _ => string.IsNullOrEmpty(elementNamespace) ? GetDefaultNamespace(targetNamespace, defaultQualifiedElement) : elementNamespace
    };

    private static string GetDefaultNamespace(string targetNamespace, bool defaultQualifiedElement) =>
        defaultQualifiedElement ? targetNamespace : string.Empty;
}
using XRoadLib.Attributes;

namespace XRoadLib.Schema;

public class ArrayItemDefinition : ParticleDefinition
{
    public ParticleDefinition Array { get; }

    [UsedImplicitly]
    public bool AcceptsAnyName { get; set; }

    [UsedImplicitly]
    public uint MinOccurs { get; set; }

    [UsedImplicitly]
    public uint? MaxOccurs { get; set; }

    [UsedImplicitly]
    public Type ItemTypeMapType { get; set; }

    public ArrayItemDefinition(ParticleDefinition array, XmlArrayItemAttribute arrayItemAttribute, Type runtimeType, string runtimeName, string targetNamespace, bool defaultQualifiedElement)
    {
        Array = array;

        var xroadArrayItemAttribute = arrayItemAttribute as XRoadXmlArrayItemAttribute;

        MinOccurs = xroadArrayItemAttribute?.MinOccurs ?? 0u;
        MaxOccurs = xroadArrayItemAttribute?.MaxOccurs;

        Content = new SingularContentDefinition(
            this,
            arrayItemAttribute,
            runtimeType,
            runtimeName,
            targetNamespace,
            defaultQualifiedElement
        );
    }
}
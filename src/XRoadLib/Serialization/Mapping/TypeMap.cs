using XRoadLib.Schema;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping;

public abstract class TypeMap : ITypeMap
{
    public TypeDefinition Definition { get; }

    protected TypeMap(TypeDefinition typeDefinition)
    {
        Definition = typeDefinition;
    }

    public abstract Task<object> DeserializeAsync(XmlReader reader, IXmlTemplateNode templateNode, ContentDefinition content, XRoadMessage message);

    public abstract Task SerializeAsync(XmlWriter writer, IXmlTemplateNode templateNode, object value, ContentDefinition content, XRoadMessage message);
}
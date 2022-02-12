using XRoadLib.Schema;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping;

public class AnyContentTypeMap : TypeMap
{
    private readonly ISerializer _serializer;

    public AnyContentTypeMap(TypeDefinition typeDefinition, ISerializer serializer)
        : base(typeDefinition)
    {
        _serializer = serializer;
    }

    public override Task<object> DeserializeAsync(XmlReader reader, IXmlTemplateNode templateNode, ContentDefinition content, XRoadMessage message)
    {
        throw new NotImplementedException();
    }

    public override Task SerializeAsync(XmlWriter writer, IXmlTemplateNode templateNode, object value, ContentDefinition content, XRoadMessage message)
    {
        var typeMap = _serializer.GetTypeMap(value.GetType());
        if (typeMap == null)
            throw new NotImplementedException($"No type definition provided for runtime type: {value.GetType().FullName}");

        return typeMap.SerializeAsync(writer, templateNode, value, content, message);
    }
}
using System.Xml;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping
{
    public interface IParameterMap
    {
        string Name { get; }

        object Deserialize(XmlReader reader, IXmlTemplateNode parameterNode, SerializationContext context);

        object DeserializeRoot(XmlReader reader, IXmlTemplateNode parameterNode, SerializationContext context);

        void Serialize(XmlWriter writer, IXmlTemplateNode parameterNode, object value, SerializationContext context);

        void SerializeRoot(XmlWriter writer, IXmlTemplateNode parameterNode, object value, SerializationContext context);
    }
}
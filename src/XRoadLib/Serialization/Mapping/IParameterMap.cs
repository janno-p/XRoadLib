using System.Xml;
using XRoadLib.Schema;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping
{
    public interface IParameterMap
    {
        ParameterDefinition Definition { get; }

        bool TryDeserialize(XmlReader reader, IXmlTemplateNode parameterNode, SerializationContext context, out object value);

        object DeserializeRoot(XmlReader reader, IXmlTemplateNode parameterNode, SerializationContext context);

        void Serialize(XmlWriter writer, IXmlTemplateNode parameterNode, object value, SerializationContext context);

        void SerializeRoot(XmlWriter writer, IXmlTemplateNode parameterNode, object value, SerializationContext context);
    }
}
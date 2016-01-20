using System.Reflection;
using System.Xml;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping
{
    public interface IParameterMap
    {
        string Name { get; }

        ParameterInfo ParameterInfo { get; }

        bool TryDeserialize(XmlReader reader, IXmlTemplateNode parameterNode, SerializationContext context, out object value);

        object DeserializeRoot(XmlReader reader, IXmlTemplateNode parameterNode, SerializationContext context);

        void Serialize(XmlWriter writer, IXmlTemplateNode parameterNode, object value, SerializationContext context);

        void SerializeRoot(XmlWriter writer, IXmlTemplateNode parameterNode, object value, SerializationContext context);
    }
}